using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// ================================================================
// ROBOT ARM AGENT (PICK-AND-PLACE)
// ---------------------------------------------------------------
// The agent learns to pick a block from the white area and place
// it on the green target area. It uses discrete actions to control
// the arm joints and the gripper toggle.
// ================================================================

public class RobotArmAgent : Agent
{
    [Header("References")]
    public RobotController robot;       // Manual robot movement logic
    public Transform gripperEnd;        // End of the gripper
    public Transform block;             // Block to pick
    public Transform targetZone;        // Green target zone
    public Transform startZone;         // White starting zone

    [Header("Randomization")]
    public float spawnRadius = 0.3f;

    private Vector3 blockStartPos;
    private Quaternion blockStartRot;

    private Vector3 robotInitialPos;
    private Quaternion robotInitialRot;

    private bool holdingBlock = false;

    // -------------------------------------------------------------
    // Initialize (called once)
    // -------------------------------------------------------------
    public override void Initialize()
    {
        robotInitialPos = robot.transform.localPosition;
        robotInitialRot = robot.transform.localRotation;

        blockStartPos = block.localPosition;
        blockStartRot = block.localRotation;
    }

    // -------------------------------------------------------------
    // Reset environment at each episode
    // -------------------------------------------------------------
    public override void OnEpisodeBegin()
    {
        // Reset robot
        robot.transform.localPosition = robotInitialPos;
        robot.transform.localRotation = robotInitialRot;

        // Reset block
        block.localPosition = blockStartPos + new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0,
            Random.Range(-spawnRadius, spawnRadius)
        );
        block.localRotation = blockStartRot;

        // Reset gripper state
        holdingBlock = false;

        // Detach block if necessary
        block.SetParent(null);
        var rb = block.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    // -------------------------------------------------------------
    // Collect observations for the agent
    // -------------------------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        // Robot / gripper observations
        sensor.AddObservation(gripperEnd.localPosition);

        // Block position
        sensor.AddObservation(block.localPosition);

        // Target zone
        sensor.AddObservation(targetZone.localPosition);

        // Whether the agent is holding the block
        sensor.AddObservation(holdingBlock ? 1f : 0f);

        // Distances (helpful for faster training)
        sensor.AddObservation(Vector3.Distance(gripperEnd.position, block.position));
        sensor.AddObservation(Vector3.Distance(block.position, targetZone.position));
    }

    // -------------------------------------------------------------
    // Actions: what the agent can do
    // -------------------------------------------------------------
    // Actions:
    // 0 - Move base joint (-1, 0, +1)
    // 1 - Move vertical joint (-1, 0, +1)
    // 2 - Toggle gripper (0 or 1)
    // -------------------------------------------------------------
    public override void OnActionReceived(ActionBuffers actions)
    {
        int baseAction = actions.DiscreteActions[0];
        int verticalAction = actions.DiscreteActions[1];
        int gripAction = actions.DiscreteActions[2];

        // Apply rotation (-1, 0, +1)
        float baseMove = ActionToMovement(baseAction);
        float vertMove = ActionToMovement(verticalAction);

        robot.bone.Rotate(0f, baseMove * robot.speed * Time.deltaTime, 0f, Space.Self);
        robot.bone006.Rotate(-vertMove * robot.speed * Time.deltaTime, 0f, 0f, Space.Self);

        // Gripper toggle
        if (gripAction == 1)
        {
            robot.SendMessage("ToggleGrip", SendMessageOptions.DontRequireReceiver);
        }

        holdingBlock = robot.GetHoldingStatus();

        // ---------- REWARDS ----------
        float distToBlock = Vector3.Distance(gripperEnd.position, block.position);
        float distBlockToTarget = Vector3.Distance(block.position, targetZone.position);

        // Reward for approaching the block
        AddReward(-distToBlock * 0.001f);

        // Reward for approaching the target while holding
        if (holdingBlock)
            AddReward(-distBlockToTarget * 0.002f);

        // Reward for successfully picking
        if (holdingBlock && distToBlock < 0.05f)
            AddReward(+2.0f);

        // Reward for placing block inside green zone
        if (!holdingBlock && distBlockToTarget < 0.1f)
        {
            AddReward(+5.0f);
            EndEpisode();
        }

        // Slight penalty for time spent
        AddReward(-0.0005f);
    }

    // Convert discrete action to movement multiplier
    float ActionToMovement(int action)
    {
        switch (action)
        {
            case 0: return -1f;  // move negative
            case 1: return 0f;   // no move
            case 2: return +1f;  // move positive
        }
        return 0f;
    }

    // -------------------------------------------------------------
    // Manual control debugging (optional)
    // -------------------------------------------------------------
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        // Base joint
        if (Input.GetKey(KeyCode.LeftArrow)) d[0] = 0;
        else if (Input.GetKey(KeyCode.RightArrow)) d[0] = 2;
        else d[0] = 1;

        // Vertical joint
        if (Input.GetKey(KeyCode.UpArrow)) d[1] = 2;
        else if (Input.GetKey(KeyCode.DownArrow)) d[1] = 0;
        else d[1] = 1;

        // Gripper
        d[2] = Input.GetKey(KeyCode.G) ? 1 : 0;
    }
}
