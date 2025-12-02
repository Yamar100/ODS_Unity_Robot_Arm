using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RobotArmAgent : Agent
{
    [Header("Robot Joints (same as RobotController)")]
    public Transform baseJoint;   // Bone
    public Transform elbowJoint;  // Bone.006

    [Header("Objects")]
    public Transform endEffector;   // tip of the robot arm
    public Transform cube;          // object to pick

    [Header("Zones")]
    public Transform blueZone;      // spawn area
    public Transform greenZone;     // target area

    [Header("Settings")]
    public float rotationSpeed = 60f;

    private bool isHoldingCube = false;

    public override void OnEpisodeBegin()
    {
        // Reset joint rotations
        baseJoint.localRotation = Quaternion.identity;
        elbowJoint.localRotation = Quaternion.identity;

        // Reset cube in blue zone
        PlaceCubeInBlueZone();

        // Reset grip state
        isHoldingCube = false;
        cube.SetParent(null);
    }

    private void PlaceCubeInBlueZone()
    {
        Renderer r = blueZone.GetComponent<Renderer>();
        Vector3 size = r.bounds.size;

        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float z = Random.Range(-size.z / 2f, size.z / 2f);

        cube.position = blueZone.position + new Vector3(x, 0.1f, z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 2 joints rotation
        sensor.AddObservation(baseJoint.localEulerAngles.y);
        sensor.AddObservation(elbowJoint.localEulerAngles.x);

        // End effector position
        sensor.AddObservation(endEffector.position);

        // Cube position
        sensor.AddObservation(cube.position);

        // Green zone position
        sensor.AddObservation(greenZone.position);

        // Holding object
        sensor.AddObservation(isHoldingCube ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float a0 = actions.ContinuousActions[0];  // base joint rotation
        float a1 = actions.ContinuousActions[1];  // elbow joint rotation

        // Apply actions
        baseJoint.Rotate(0, a0 * rotationSpeed * Time.deltaTime, 0);
        elbowJoint.Rotate(a1 * rotationSpeed * Time.deltaTime, 0, 0);

        float reward = 0f;

        // ---- PHASE 1: go to cube ----
        if (!isHoldingCube)
        {
            float dCube = Vector3.Distance(endEffector.position, cube.position);
            reward += -dCube * 0.1f;

            if (dCube < 0.07f)
            {
                isHoldingCube = true;
                cube.SetParent(endEffector);
                AddReward(1.0f);
            }
        }
        // ---- PHASE 2: move cube to green zone ----
        else
        {
            float dGreen = Vector3.Distance(cube.position, greenZone.position);
            reward += -dGreen * 0.1f;

            if (dGreen < 0.1f)
            {
                AddReward(3.0f);
                EndEpisode();
            }
        }

        AddReward(reward);
    }
}
