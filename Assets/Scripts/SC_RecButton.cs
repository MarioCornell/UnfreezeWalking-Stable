using Meta.XR.MRUtilityKit;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SC_RecButton : MonoBehaviour
{
    [Header("Transforms to Log")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform leftLegTransform;
    [SerializeField] private Transform rightLegTransform;

    private Button recButton;
    [SerializeField] private Color _recordingColor;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Image _recImage;

    [SerializeField] private CustomInputField _sessionNameInput;

    void Start()
    {
        headTransform = GameObject.Find("Head").transform;
        leftLegTransform = GameObject.Find("LeftLeg").transform;
        rightLegTransform = GameObject.Find("RightLeg").transform;


        recButton = GetComponent<Button>();

        if (headTransform == null || leftLegTransform == null || rightLegTransform == null)
        {
            Debug.LogError("One or more transforms have not been assigned in the Inspector for SC_RecButton!", this);
            recButton.interactable = false;
            return;
        }

        DataLogger.Initialize(headTransform, leftLegTransform, rightLegTransform);
        recButton.onClick.AddListener(OnRecButtonPressed);
        UpdateButtonVisuals();
    }

    void Update()
    {
        DataLogger.UpdateFrameData();
    }

    public void ToggleLogging()
    {
        if (!DataLogger.IsLogging)
        {
            Debug.Log("not logging so start logging");
            string sessionName = (_sessionNameInput != null) ? _sessionNameInput.inputText.text : null;
            DataLogger.StartLogging(sessionName);
            UpdateButtonVisuals();
        }
        else
        {
            Debug.Log("stop logging");
            DataLogger.StopLogging();
            UpdateButtonVisuals();
        }
    }

    [ContextMenu("Start Logging")]
    public void StartLogging()
    {
        if (DataLogger.IsLogging)
        {
            Debug.LogWarning("Logging already started");
            return;
        }

        string sessionName = (_sessionNameInput != null) ? _sessionNameInput.inputText.text : null;
        DataLogger.StartLogging(sessionName);
        UpdateButtonVisuals();
    }

    [ContextMenu("Stop Logging")]
    public void StopLogging()
    {
        if (DataLogger.IsLogging)
        {
            DataLogger.StopLogging();
            UpdateButtonVisuals();

        }
    }

    private void OnRecButtonPressed()
    {
        if (DataLogger.IsLogging)
        {
            DataLogger.StopLogging();
        }
        else
        {
            string sessionName = (_sessionNameInput != null) ? _sessionNameInput.inputText.text : null;
            DataLogger.StartLogging(sessionName);
            // FindObjectOfType<SC_TeleportToFront>().Teleport();
        }
        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        if (DataLogger.IsLogging)
        {
            _recImage.color = _recordingColor;
        }
        else
        {
            _recImage.color = _normalColor;
        }
    }

    void OnDestroy()
    {
        if (recButton != null)
        {
            recButton.onClick.RemoveListener(OnRecButtonPressed);
        }
    }
}