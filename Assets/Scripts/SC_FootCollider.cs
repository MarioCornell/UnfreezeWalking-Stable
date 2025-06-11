using UnityEngine;

public class SC_FootCollider : MonoBehaviour
{
    private int overlappingSteps = 0;

    public bool IsHitting()
    {
        return overlappingSteps > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // We check for both SC_Step and SC_StepReference as per the project's structure
        if (other.GetComponent<SC_Step>() != null || other.GetComponent<SC_StepReference>() != null)
        {
            overlappingSteps++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<SC_Step>() != null || other.GetComponent<SC_StepReference>() != null)
        {
            // Ensure we don't go into negative counts, though this shouldn't happen in normal physics.
            if (overlappingSteps > 0)
            {
                overlappingSteps--;
            }
        }
    }
}