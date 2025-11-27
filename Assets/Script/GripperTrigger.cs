using UnityEngine;

public class GripperTrigger : MonoBehaviour
{
    public bool isTouchingBlock = false;
    public Transform currentBlock = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Block"))
        {
            isTouchingBlock = true;
            currentBlock = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Block"))
        {
            isTouchingBlock = false;

            if (currentBlock == other.transform)
                currentBlock = null;
        }
    }
}
