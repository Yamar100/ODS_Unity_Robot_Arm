using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Robot Joints")]
    public Transform baseJoint;       // Bone
    public Transform shoulderJoint;   // Bone.001
    public Transform elbowJoint;      // Bone.003
    public Transform forearmJoint;    // Bone.005
    public Transform wristYaw;        // Bone.007
    public Transform wristRoll;       // Bone.009

    [Header("Settings")]
    public float speed = 60f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");   // Arrow Left/Right
        float vertical = Input.GetAxis("Vertical");       // Arrow Up/Down

        // -----------------------
        // BASE ROTATION
        // -----------------------
        baseJoint.Rotate(0, horizontal * speed * Time.deltaTime, 0);

        // -----------------------
        // SHOULDER (Y)
        // -----------------------
        if (Input.GetKey(KeyCode.Y))
            shoulderJoint.Rotate(vertical * speed * Time.deltaTime, 0, 0);

        // -----------------------
        // ELBOW (X)
        // -----------------------
        if (Input.GetKey(KeyCode.X))
            elbowJoint.Rotate(vertical * speed * Time.deltaTime, 0, 0);

        // -----------------------
        // FOREARM (C)
        // -----------------------
        if (Input.GetKey(KeyCode.C))
            forearmJoint.Rotate(vertical * speed * Time.deltaTime, 0, 0);

        // -----------------------
        // WRIST YAW (V)
        // -----------------------
        if (Input.GetKey(KeyCode.V))
            wristYaw.Rotate(0, vertical * speed * Time.deltaTime, 0);

        // -----------------------
        // WRIST ROLL (B)
        // -----------------------
        if (Input.GetKey(KeyCode.B))
            wristRoll.Rotate(0, 0, vertical * speed * Time.deltaTime);
    }
}




/*
    ============================
    ROBOT ARM KEYBOARD CONTROLS
    ============================

    Simple manual control of the 6-DOF robotic arm.

    --- BASE ROTATION ---
    (← →) Left / Right Arrows  -> Rotate the base joint

    --- JOINT MOVEMENT ---
    NOTE: Use Up / Down arrows together with the keys below

    Q + ↑/↓   -> Move the Shoulder Joint
    W + ↑/↓   -> Move the Elbow Joint
    E + ↑/↓   -> Move the Forearm Joint

    --- WRIST CONTROL ---
    A + ↑/↓   -> Wrist Yaw rotation
    S + ↑/↓  -> Wrist Roll rotation

    These controls allow manual testing of the robot arm before 
    implementing IK, ML-Agents, or automated trajectories.
*/
