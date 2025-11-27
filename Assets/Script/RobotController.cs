using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Joints to control")]
    public Transform bone;      // left/right rotation (base joint)
    public Transform bone006;   // up/down rotation (vertical joint)

    [Header("Settings")]
    public float speed = 60f;

    [Header("Gripper System")]
    public Transform gripperEnd;          // End point of the gripper
    public GripperTrigger gripperTrigger; // Block detection script

    private bool isAttached = false;      // True if a block is currently held
    private Transform grabbedBlock = null;
    private float grabbedBlockInitialY;   // Initial height of the block

    public bool GetHoldingStatus()
    {
        return isAttached;
    }


    void Update()
    {
        float vertical = Input.GetAxis("Vertical");     // ↑ ↓ arrow keys
        float horizontal = Input.GetAxis("Horizontal"); // ← → arrow keys

        // -----------------------
        // UP/DOWN ROTATION : Bone.006
        // -----------------------
        if (bone006 != null)
        {
            bone006.Rotate(-vertical * speed * Time.deltaTime, 0f, 0f, Space.Self);
        }

        // -----------------------
        // LEFT/RIGHT ROTATION : Bone
        // -----------------------
        if (bone != null)
        {
            bone.Rotate(0f, horizontal * speed * Time.deltaTime, 0f, Space.Self);
        }

        // ----- Grip / Release using G -----
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGrip();
        }
    }

    ////////////////////////////////////////////////////////
    private void ToggleGrip()
    {
        // 1) If nothing is attached → attempt to grab
        if (!isAttached)
        {
            if (gripperTrigger == null || !gripperTrigger.isTouchingBlock)
                return;

            Transform block = gripperTrigger.currentBlock;
            if (block == null) 
                return;

            grabbedBlock = block;

            // Store the block's initial height
            grabbedBlockInitialY = grabbedBlock.position.y;

            // Disable physics while holding the block
            Rigidbody rb = grabbedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Attach block to the gripper
            grabbedBlock.SetParent(gripperEnd);
            grabbedBlock.localPosition = Vector3.zero;

            isAttached = true;
        }
        // 2) If a block is already attached → release it
        else
        {
            if (grabbedBlock == null)
            {
                isAttached = false;
                return;
            }

            // Detach the block from the gripper
            grabbedBlock.SetParent(null);

            // Restore the block’s original height (Y axis)
            Vector3 pos = grabbedBlock.position;
            pos.y = grabbedBlockInitialY;
            grabbedBlock.position = pos;

            // Re-enable physics
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



    

}


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

