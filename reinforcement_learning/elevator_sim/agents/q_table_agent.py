import json
from collections import defaultdict
from pathlib import Path
from typing import Any, TypeAlias, Annotated

import gymnasium as gym
import numpy as np
from pydantic import BaseModel, Field, PlainSerializer, field_validator
from pydantic_numpy.model import NumpyModel


ObservationKey: TypeAlias = tuple[
    tuple[tuple[Any, ...], ...],
    tuple[tuple[Any, ...], ...],
    tuple[tuple[Any, ...], ...]
]

# Custom serializer for numpy.int64
SerializableInt64 = Annotated[
    np.int64,
    PlainSerializer(lambda x: int(x), return_type=int, when_used='json')
]

# Custom serializer for numpy.float64
SerializableFloat64 = Annotated[
    np.float64,
    PlainSerializer(lambda x: float(x), return_type=float, when_used='json')
]

# Update QValueKey to use serializable types
QValueKey: TypeAlias = tuple[
    tuple[SerializableFloat64 | SerializableInt64, ...],
    tuple[SerializableFloat64 | SerializableInt64, ...],
    tuple[SerializableFloat64 | SerializableInt64, ...]
]

class AgentState(NumpyModel):
    """Pydantic model for serializing agent state."""
    q_values: dict[QValueKey, list[float]] = Field(
        description="Q-table as dict with string keys"
    )
    lr: float
    discount_factor: float
    epsilon: float
    epsilon_decay: float
    final_epsilon: float
    training_error: list[float] = Field(default_factory=list)

    class Config:
        # Allow arbitrary types (needed for compatibility)
        arbitrary_types_allowed = True

    @field_validator('q_values', mode='before')
    @classmethod
    def deserialize_q_values(cls, value: Any) -> dict:
        """Convert comma-separated string keys back to tuple keys."""
        if isinstance(value, dict):
            result = {}
            for k, v in value.items():
                if isinstance(k, str):
                    # Split comma-separated string and convert to appropriate numpy types
                    parts = k.split(',')
                    # Convert each part to float first, then to numpy.float64 or numpy.int64
                    key = tuple(
                        tuple([np.int64(float(x)) if float(x).is_integer() else np.float64(x)])
                        for x in parts
                    )
                    result[key] = v
                else:
                    result[k] = v
            return result
        return value

class QTableAgent:
    def __init__(
        self,
        env: gym.Env,
        learning_rate: float,
        initial_epsilon: float,
        epsilon_decay: float,
        final_epsilon: float,
        discount_factor: float = 0.95,
    ):
        """Initialize a Q-Learning agent.

        Args:
            env: The training environment
            learning_rate: How quickly to update Q-values (0-1)
            initial_epsilon: Starting exploration rate (usually 1.0)
            epsilon_decay: How much to reduce epsilon each episode
            final_epsilon: Minimum exploration rate (usually 0.1)
            discount_factor: How much to value future rewards (0-1)
        """
        self.env = env

        # Q-table: maps (state, action) to expected reward
        # defaultdict automatically creates entries with zeros for new states
        self.q_values = defaultdict(lambda: np.zeros(env.action_space.n))

        self.lr = learning_rate
        self.discount_factor = discount_factor  # How much we care about future rewards

        # Exploration parameters
        self.epsilon = initial_epsilon
        self.epsilon_decay = epsilon_decay
        self.final_epsilon = final_epsilon

        # Track learning progress
        self.training_error = []

    def get_action(self, obs: dict[str, np.ndarray]) -> int:
        """Choose an action using epsilon-greedy strategy.

        Returns:
            action: 0 (stand) or 1 (hit)
        """
        # Convert observation dict to hashable tuple for Q-table lookup
        obs_key = self._get_obs_key(obs)

        # With probability epsilon: explore (random action)
        if np.random.random() < self.epsilon:
            return self.env.action_space.sample()

        # With probability (1-epsilon): exploit (best known action)
        else:
            return int(np.argmax(self.q_values[obs_key]))

    @staticmethod
    def _get_obs_key(obs: dict[str, np.ndarray[tuple[Any, ...], np.dtype[Any]]]) -> ObservationKey:
        return (tuple(obs["elevator"]), tuple(obs["current_elevator_target"]), tuple(obs["target"]))

    def update(
        self,
        obs: dict[str, np.ndarray],
        action: int,
        reward: float,
        terminated: bool,
        next_obs: dict[str, np.ndarray],
    ):
        """Update Q-value based on experience.

        This is the heart of Q-learning: learn from (state, action, reward, next_state)
        """
        # Convert observation dicts to hashable tuples for Q-table lookup
        obs_key = self._get_obs_key(obs)
        next_obs_key = self._get_obs_key(next_obs)

        # What's the best we could do from the next state?
        # (Zero if episode terminated - no future rewards possible)
        future_q_value = (not terminated) * np.max(self.q_values[next_obs_key])

        # What should the Q-value be? (Bellman equation)
        target = reward + self.discount_factor * future_q_value

        # How wrong was our current estimate?
        temporal_difference = target - self.q_values[obs_key][action]

        # Update our estimate in the direction of the error
        # Learning rate controls how big steps we take
        self.q_values[obs_key][action] = (
            self.q_values[obs_key][action] + self.lr * temporal_difference
        )

        # Track learning progress (useful for debugging)
        self.training_error.append(temporal_difference)

    def decay_epsilon(self):
        """Reduce exploration rate after each episode."""
        self.epsilon = max(self.final_epsilon, self.epsilon - self.epsilon_decay)


    def save(self, filepath: str | Path):
        """Save agent state to JSON file using Pydantic."""
        filepath = Path(filepath)
        filepath.parent.mkdir(parents=True, exist_ok=True)

        # Create Pydantic model
        state = AgentState(
            q_values=self.q_values,
            lr=self.lr,
            discount_factor=self.discount_factor,
            epsilon=self.epsilon,
            epsilon_decay=self.epsilon_decay,
            final_epsilon=self.final_epsilon,
            training_error=self.training_error
        )

        # Save as JSON
        with open(filepath, 'w') as f:
            f.write(state.model_dump_json(indent=2))

        print(f"Agent saved to {filepath}")

    @classmethod
    def load(cls, filepath: str | Path, env: gym.Env):
        """Load agent from JSON file using Pydantic."""
        with open(filepath, 'r') as f:
            state = AgentState.model_validate_json(f.read())

        # Create agent with loaded parameters
        agent = cls(
            env=env,
            learning_rate=state.lr,
            initial_epsilon=state.epsilon,
            epsilon_decay=state.epsilon_decay,
            final_epsilon=state.final_epsilon,
            discount_factor=state.discount_factor
        )

        # Restore Q-table from JSON format
        agent.q_values = defaultdict(
            lambda: np.zeros(env.action_space.n),
            {
                k: np.array(v)
                for k, v in state.q_values.items()
            }
        )
        agent.training_error = state.training_error

        print(f"Agent loaded from {filepath}")
        return agent