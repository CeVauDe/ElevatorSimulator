from collections import defaultdict
from typing import Any

import gymnasium as gym
from gymnasium.wrappers import RecordEpisodeStatistics, RecordVideo
import numpy as np
from tqdm import tqdm
from matplotlib import pyplot as plt

import envs.elevator_sim


class ElevatorSimAgent:
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
    def _get_obs_key(obs: dict[str, np.ndarray[tuple[Any, ...], np.dtype[Any]]]) -> tuple[
        tuple[tuple[Any, ...], ...], tuple[tuple[Any, ...], ...], tuple[tuple[Any, ...], ...]]:
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


if __name__ == "__main__":
    # Training hyperparameters
    learning_rate = 0.01  # How fast to learn (higher = faster but less stable)
    n_episodes = 10_000  # Number of episodes to practice
    start_epsilon = 1.0  # Start with 100% random actions
    epsilon_decay = start_epsilon / (n_episodes / 2)  # Reduce exploration over time
    final_epsilon = 0.0  # Always keep some exploration

    num_eval_episodes = 1_000

    env = envs.envs.ElevatorSimEnv(render_mode="rgb_array", num_floors=10)
    # Add video recording for every episode
    env = RecordVideo(
        env,
        video_folder="gridworld-agent",  # Folder to save videos
        name_prefix="eval",  # Prefix for video filenames
        episode_trigger=lambda x: x in [0, 1, 2, n_episodes - 2] # Record first and last episodes
    )

    # Add episode statistics tracking
    env = RecordEpisodeStatistics(env, buffer_length=num_eval_episodes)

    agent = ElevatorSimAgent(
        env=env,
        learning_rate=learning_rate,
        initial_epsilon=start_epsilon,
        epsilon_decay=epsilon_decay,
        final_epsilon=final_epsilon,
    )


    for episode in tqdm(range(n_episodes)):
        # Start a new hand
        obs, info = env.reset()
        done = False

        # Play one complete hand
        while not done:
            # Agent chooses action (initially random, gradually more intelligent)
            action = agent.get_action(obs)

            # Take action and observe result
            next_obs, reward, terminated, truncated, info = env.step(action)

            # Learn from this experience
            agent.update(obs, action, reward, terminated, next_obs)

            # Move to next state
            done = terminated or truncated
            obs = next_obs

        # Reduce exploration rate (agent becomes less random over time)
        agent.decay_epsilon()

    # Print summary statistics
    print(f'\nEvaluation Summary:')
    print(f'Episode durations: {list(env.time_queue)}')
    print(f'Episode rewards: {list(env.return_queue)}')
    print(f'Episode lengths: {list(env.length_queue)}')

    # Calculate some useful metrics
    avg_reward = np.sum(env.return_queue)
    avg_length = np.sum(env.length_queue)
    std_reward = np.std(env.return_queue)

    print(f'\nAverage reward: {avg_reward:.2f} ± {std_reward:.2f}')
    print(f'Average episode length: {avg_length:.1f} steps')
    print(f'Success rate: {sum(1 for r in env.return_queue if r > 0) / len(env.return_queue):.1%}')

    plt.show()