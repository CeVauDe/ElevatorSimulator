from gymnasium.envs.registration import register

register(
    id="env_tutorial/GridWorld-v0",
    entry_point="env_tutorial.envs:GridWorldEnv",
)
