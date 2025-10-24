using UnityEngine;
using System.Collections;

public class SC_FootCollider : MonoBehaviour
{
    public enum ControllerHand { Left, Right }
    [SerializeField] private ControllerHand hand;

    [Header("Haptics Settings")]
    [SerializeField] private float vibrationFrequency = 0.5f;
    [SerializeField] private float vibrationAmplitude = 1.0f;
    [SerializeField] private float vibrationDuration = 0.15f;

    private int overlappingSteps = 0;
    private OVRInput.Controller controllerMask;
    private Coroutine activeVibrationCoroutine;

    private void Start()
    {
        controllerMask = (hand == ControllerHand.Left) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
    }

    public bool IsHitting()
    {
        return overlappingSteps > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SC_Step>() != null || other.GetComponent<SC_StepReference>() != null)
        {
            overlappingSteps++;

            if (overlappingSteps == 1)
            {
                if (activeVibrationCoroutine != null)
                {
                    StopCoroutine(activeVibrationCoroutine);
                }
                activeVibrationCoroutine = StartCoroutine(VibratePulse());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<SC_Step>() != null || other.GetComponent<SC_StepReference>() != null)
        {
            if (overlappingSteps > 0)
            {
                overlappingSteps--;
                if (overlappingSteps == 0)
                {
                    if (activeVibrationCoroutine != null)
                    {
                        StopCoroutine(activeVibrationCoroutine);
                        activeVibrationCoroutine = null;
                    }
                    OVRInput.SetControllerVibration(0, 0, controllerMask);
                }
            }
        }
    }

    private IEnumerator VibratePulse()
    {
        OVRInput.SetControllerVibration(vibrationFrequency, vibrationAmplitude, controllerMask);
        yield return new WaitForSeconds(vibrationDuration);
        OVRInput.SetControllerVibration(0, 0, controllerMask);
        activeVibrationCoroutine = null;
    }
}