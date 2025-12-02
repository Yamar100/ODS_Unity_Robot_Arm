using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Joints to control")]
    public Transform bone;      // left/right rotation (base joint)
    public Transform bone006;   // up/down rotation (vertical joint)

    [Header("Settings")]
    public float speed = 30f;

    [Header("Gripper System")]
    public Transform gripperEnd;
    public GripperTrigger gripperTrigger;

    private bool isAttached = false;
    private Transform grabbedBlock = null;
    private float grabbedBlockInitialY;

    // ------------------------------
    // Needed for ResetRobot()
    // ------------------------------
    private Quaternion initialBoneRot;
    private Quaternion initialBone006Rot;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialBoneRot = bone.localRotation;
        initialBone006Rot = bone006.localRotation;

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public bool GetHoldingStatus()
    {
        return isAttached;
    }

    void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Bone006 movement
        if (bone006 != null)
            bone006.Rotate(-vertical * speed * Time.deltaTime, 0f, 0f, Space.Self);

        // Base movement
        if (bone != null)
            bone.Rotate(0f, horizontal * speed * Time.deltaTime, 0f, Space.Self);

        // Manual grip toggle
        if (Input.GetKeyDown(KeyCode.G))
            ToggleGrip();
    }

    // ----------------------------------------------------
    // 1. PUBLIC ToggleGrip (your same code, unchanged)
    // ----------------------------------------------------
    public void ToggleGrip()
    {
        if (!isAttached)
        {
            if (gripperTrigger == null || !gripperTrigger.isTouchingBlock)
                return;

            Transform block = gripperTrigger.currentBlock;
            if (block == null)
                return;

            grabbedBlock = block;

            grabbedBlockInitialY = grabbedBlock.position.y;

            Rigidbody rb = grabbedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            grabbedBlock.SetParent(gripperEnd);
            grabbedBlock.localPosition = Vector3.zero;

            isAttached = true;
        }
        else
        {
            if (grabbedBlock == null)
            {
                isAttached = false;
                return;
            }

            grabbedBlock.SetParent(null);

            Vector3 pos = grabbedBlock.position;
            pos.y = grabbedBlockInitialY;
            grabbedBlock.position = pos;

            Rigidbody rb = grabbedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            grabbedBlock = null;
            isAttached = false;
        }
    }

    // ----------------------------------------------------
    // 2. ADDED: ResetRobot()   (NO CHANGE → only added)
    // ----------------------------------------------------
    public void ResetRobot()
    {
        // Reset arm transform
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;

        // Reset joints
        bone.localRotation = initialBoneRot;
        bone006.localRotation = initialBone006Rot;

        // Release block if holding
        if (isAttached && grabbedBlock != null)
        {
            grabbedBlock.SetParent(null);

            Rigidbody rb = grabbedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        grabbedBlock = null;
        isAttached = false;
    }

    // ----------------------------------------------------
    // 3. ADDED: ApplyDiscreteActions() 
    // ----------------------------------------------------
    public void ApplyDiscreteActions(int baseAction, int verticalAction)
    {
        float baseMove = ActionToMovement(baseAction);
        float vertMove = ActionToMovement(verticalAction);

        // Base rotation
        bone.Rotate(0f, baseMove * speed * Time.deltaTime, 0f, Space.Self);

        // Arm vertical rotation
        bone006.Rotate(-vertMove * speed * Time.deltaTime, 0f, 0f, Space.Self);
    }

    private float ActionToMovement(int action)
    {
        switch (action)
        {
            case 0: return -1f;  
            case 1: return 0f;   
            case 2: return +1f;  
        }
        return 0f;
    }
}
