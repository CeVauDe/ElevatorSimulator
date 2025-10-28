from gymnasium.wrappers import RecordEpisodeStatistics, RecordVideo
import numpy as np
from tqdm import tqdm
from matplotlib import pyplot as plt

import elevator_sim.envs
from elevator_sim.agents import QTableAgent


if __name__ == "__main__":
    # Training hyperparameters
    learning_rate = 0.01  # How fast to learn (higher = faster but less stable)
    n_episodes = 10_000  # Number of episodes to practice
    start_epsilon = 1.0  # Start with 100% random actions
    epsilon_decay = start_epsilon / (n_episodes / 2)  # Reduce exploration over time
    final_epsilon = 0.0  # Always keep some exploration

    num_eval_episodes = 1_000

    env = elevator_sim.envs.ElevatorSimEnv(render_mode="rgb_array", num_floors=10)
    # Add video recording for every episode
    env = RecordVideo(
        env,
        video_folder="gridworld-agent",  # Folder to save videos
        name_prefix="eval",  # Prefix for video filenames
        episode_trigger=lambda x: x in [0, 1, 2, n_episodes - 2] # Record first and last episodes
    )

    # Add episode statistics tracking
    env = RecordEpisodeStatistics(env, buffer_length=num_eval_episodes)

    agent = QTableAgent(
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