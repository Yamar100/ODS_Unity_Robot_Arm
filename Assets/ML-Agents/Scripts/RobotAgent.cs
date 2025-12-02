using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RobotAgent : Agent
{
    [Header("Robot Joints")]
    public Transform bone;        // Base rotation (Y-axis)
    public Transform bone006;     // Main vertical joint (X-axis)

    [Header("Gripper")]
    public Transform gripperEnd;  // End of the arm
    public float rotationSpeed = 60f;

    [Header("Target")]
    public Transform targetCube;
    public Vector3 targetAreaMin = new Vector3(-1.5f, 0f, -1.5f);
    public Vector3 targetAreaMax = new Vector3(1.5f, 0f, 1.5f);

    public override void CollectObservations(VectorSensor sensor)
    {
        // --- Observations (SpaceSize = 6) ---
        sensor.AddObservation(gripperEnd.position); // 3 values
        sensor.AddObservation(targetCube.position); // 3 values
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float baseInput = actions.ContinuousActions[0];  // [-1, 1]
        float jointInput = actions.ContinuousActions[1]; // [-1, 1]

        // Base rotation
        bone.Rotate(0f, baseInput * rotationSpeed * Time.deltaTime, 0f, Space.Self);

        // Vertical joint movement
        bone006.Rotate(jointInput * rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);

        // ---- REWARDS ----

        float distance = Vector3.Distance(gripperEnd.position, targetCube.position);

        // Reward shaping
        AddReward(-distance * 0.001f);  // encourage reducing the distance

        // Bonus if close
        if (distance < 0.2f)
        {
            AddReward(0.5f); // good!
        }

        // Episode success
        if (distance < 0.05f)
        {
            AddReward(2.0f);
            EndEpisode();
        }

        // Timeout handled by MaxStep
    }

    public override void OnEpisodeBegin()
    {
        // Reset cube position
        targetCube.localPosition = new Vector3(
            Random.Range(targetAreaMin.x, targetAreaMax.x),
            0.0f,
            Random.Range(targetAreaMin.z, targetAreaMax.z)
        );

        // Reset joints
        bone.localRotation = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f);
        bone006.localRotation = Quaternion.Euler(Random.Range(-30f, 30f), 0f, 0f);
    }
}
