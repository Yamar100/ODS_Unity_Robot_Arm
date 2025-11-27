/*
    ============================
    SIMPLE ROBOT ARM INTERACTION
    ============================

    Manual control of a robot arm to move and manipulate blocks.

    --- ARM MOVEMENT ---
    (← →) Left / Right Arrows  -> Rotate the base joint
    (↑ ↓) Up / Down Arrows     -> Move the vertical joint

    --- GRIPPER ACTION ---
    (G) Press G to grab or release a block
    - When released, the block is automatically restored to its original height
      regardless of the drop location.

    Notes:
    - Designed for simple testing and user-controlled interaction.
    - Ensures consistent block placement for calibration and training purposes.
*/




////////////////////////////////////////
to do record dat + ML agents 
/////////////////////////////////////



run data collection (todo - 25 episodes)
mlagents-learn robot_arm_bc_ppo.yaml --run-id=RobotArmBC --initialize-from= --record-demonstrations


data save in :
Assets/Demos/robot_arm.demo

ML Traning:
mlagents-learn robot_arm_bc_ppo.yaml --run-id=RobotArmBCPPO --resume

