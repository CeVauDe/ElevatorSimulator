from enum import Enum
import gymnasium as gym
from gymnasium import spaces
import pygame
import numpy as np



class ElevatorSimEnv(gym.Env):
    metadata = {"render_modes": ["human", "rgb_array"], "render_fps": 4}

    def __init__(self, render_mode=None, num_floors=5):
        self.num_floors = num_floors  # The number of floors of the building
        self.window_size = 512  # The size of the PyGame window
        self.current_step = 0
        self.direction = 0

        # Observations are dictionaries with the agent's and the target's location.
        # Each location is encoded as an element of {0, ..., `size`}^2,
        # i.e. MultiDiscrete([size, size]).
        self.observation_space = spaces.Dict(
            {
                "elevator": spaces.Box(0, num_floors - 1, shape=(1,), dtype=int),
                "current_elevator_target": spaces.Box(0, num_floors - 1, shape=(1,), dtype=int),
                "target": spaces.Box(0, num_floors - 1, shape=(1,), dtype=int),
            }
        )

        # We have actions equivalent to the number of floors
        self.action_space = spaces.Discrete(self.num_floors + 1, start=-1)

        assert render_mode is None or render_mode in self.metadata["render_modes"]
        self.render_mode = render_mode

        """
        If human-rendering is used, `self.window` will be a reference
        to the window that we draw to. `self.clock` will be a clock that is used
        to ensure that the environment is rendered at the correct framerate in
        human-mode. They will remain `None` until human-mode is used for the
        first time.
        """
        self.window = None
        self.clock = None

    def _get_obs(self):
        return {"elevator": self._elevator_location, "current_elevator_target": self._current_elevator_target, "target": self._target_location}

    def _get_info(self):
        return {
            "distance": np.linalg.norm(
                self._elevator_location - self._target_location, ord=1
            )
        }

    def reset(self, seed=None, options=None):
        # We need the following line to seed self.np_random
        super().reset(seed=seed)

        # Choose the agent's location uniformly at random
        self._elevator_location = self.np_random.integers(0, self.num_floors, size=1, dtype=int)
        self._current_elevator_target = np.array([-1])  # No target at the beginning

        # We will sample the target's location randomly until it does not
        # coincide with the agent's location
        self._target_location = self._elevator_location
        while np.array_equal(self._target_location, self._elevator_location):
            self._target_location = self.np_random.integers(
                0, self.num_floors, size=1, dtype=int
            )

        self.current_step = 0
        self.direction = 0

        observation = self._get_obs()
        info = self._get_info()

        if self.render_mode == "human":
            self._render_frame()

        return observation, info

    def step(self, action):
        if action != -1:
            self.direction = np.sign(action - self._elevator_location)

        # We use `np.clip` to make sure we don't leave the grid
        self._elevator_location = np.clip(
            self._elevator_location + 0.25 * self.direction, 0, self.num_floors - 1
        )
        # An episode is done iff the agent has reached the target
        terminated = np.array_equal(self._elevator_location, self._target_location)

        # Non-linear reward function (exponential decay with distance)
        distance = np.abs(self._elevator_location[0] - self._target_location[0])
        max_distance = self.num_floors - 1

        # Option 1: Exponential reward (very close = much better)
        reward = np.exp(-2 * distance / max_distance) - 1
        # Add bonus for reaching target
        if terminated:
            reward += 10  # Large bonus for reaching the goal

        if action == -1:
            reward += 1  # Small bonus for not changing the direction

        observation = self._get_obs()
        info = self._get_info()

        if self.render_mode == "human":
            self._render_frame()


        self.current_step += 1
        truncated = self.current_step >= 1_000  # Max steps per episode

        return observation, reward, terminated, truncated, info

    def render(self):
        if self.render_mode == "rgb_array":
            return self._render_frame()

    def _render_frame(self):
        if self.window is None and self.render_mode == "human":
            pygame.init()
            pygame.display.init()
            self.window = pygame.display.set_mode((self.window_size, self.window_size))
        if self.clock is None and self.render_mode == "human":
            self.clock = pygame.time.Clock()

        canvas = pygame.Surface((self.window_size, self.window_size))
        canvas.fill((255, 255, 255))
        pix_square_size = (
            self.window_size / self.num_floors
        )  # The size of a single grid square in pixels

        # First we draw the target
        target = np.array([1, self._target_location[0]])
        pygame.draw.rect(
            canvas,
            (255, 0, 0),
            pygame.Rect(
                pix_square_size * target,
                (pix_square_size, pix_square_size),
            ),
        )
        # Now we draw the agent
        center = np.array([1, self._elevator_location[0]])
        pygame.draw.circle(
            canvas,
            (0, 0, 255),
            (center + 0.5) * pix_square_size,
            pix_square_size / 3,
        )

        # Finally, add some gridlines
        for x in range(self.num_floors + 1):
            pygame.draw.line(
                canvas,
                0,
                (0, pix_square_size * x),
                (self.window_size, pix_square_size * x),
                width=3,
            )
            pygame.draw.line(
                canvas,
                0,
                (pix_square_size * x, 0),
                (pix_square_size * x, self.window_size),
                width=3,
            )

        if self.render_mode == "human":
            # The following line copies our drawings from `canvas` to the visible window
            self.window.blit(canvas, canvas.get_rect())
            pygame.event.pump()
            pygame.display.update()

            # We need to ensure that human-rendering occurs at the predefined framerate.
            # The following line will automatically add a delay to
            # keep the framerate stable.
            self.clock.tick(self.metadata["render_fps"])
        else:  # rgb_array
            return np.transpose(
                np.array(pygame.surfarray.pixels3d(canvas)), axes=(1, 0, 2)
            )

    def close(self):
        if self.window is not None:
            pygame.display.quit()
            pygame.quit()
