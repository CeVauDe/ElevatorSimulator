from gymnasium.envs.registration import register

register(
    id="elevator_sim/GridWorld-v0",
    entry_point="elevator_sim.elevator_sim:GridWorldEnv",
)
