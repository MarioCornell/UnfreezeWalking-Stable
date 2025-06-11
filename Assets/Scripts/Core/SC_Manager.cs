using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Michsky.MUIP;
using Unity.Netcode;
using Sirenix.OdinInspector;

public class SC_Manager : NetworkBehaviour
{
    public SO_SceneConfig CurrentSceneConfig;
    public List<SO_SceneConfig> SceneConfigPresets;
    public List<ButtonManager> SceneConfigButtons;
    public List<ButtonManager> SaveSceneConfigButtons;
    
    public SliderManager TotalDistanceSlider;
    public SliderManager DistanceBetweenStepsSlider;
    public SliderManager DoorScaleSlider;
    public SliderManager DepthOffsetSlider;
    public RadialSlider RotationOffsetSlider;
    
    public Transform StepDoorParent;
    public GameObject ReferenceStep;
    public GameObject StepParent;
    public SC_Step StepPrefab;
    public GameObject Door;
    public CustomInputField SceneNameInputField;

    public CustomInputField ScaleXInputField; // name "ScaleXInputField"
    public CustomInputField ScaleYInputField; // name "ScaleYInputField"
    public CustomInputField ScaleZInputField; // name "ScaleZInputField"
    
    private bool SetupComplete = false;
    private ObjectPool<SC_Step> stepPool;
    private List<SC_Step> activeSteps = new List<SC_Step>();

    private OVRManager ovrManager;

    [ContextMenu("Open Persistent Data Path")]
    public void OpenPersistentDataPath(){
        string path = Application.persistentDataPath;
        Debug.Log($"Opening Persistent Data Path: {path}");
        
        #if UNITY_EDITOR_OSX
        System.Diagnostics.Process.Start("open", path);
        #endif
        
        #if UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start("explorer.exe", path);
        #endif
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ServerSetup();
        }
        else
        {
            ClientSetup();
        }
    }
    
    // --- NEW --- Unsubscribe from the static event when the object is destroyed
    private void OnDestroy()
    {
        if (IsServer)
        {
            SC_StepReference.OnScaleChanged -= OnReferenceStepScaleChanged;
        }
    }

    private void ServerSetup()
    {
        // Reset depth and rotation offsets to default values on server startup.
        // This prevents them from persisting across sessions in the editor.
        CurrentSceneConfig.DepthOffset = 1f;
        CurrentSceneConfig.RotationOffset = 0f;

        stepPool = new ObjectPool<SC_Step>(
            CreateStep,
            OnGetStep,
            OnReleaseStep,
            OnDestroyStep,
            maxSize: 100
        );
        
        TotalDistanceSlider = GameObject.Find("TrialDistance").GetComponent<SliderManager>();
        DistanceBetweenStepsSlider = GameObject.Find("StrideLength").GetComponent<SliderManager>();
        DoorScaleSlider = GameObject.Find("DoorScale").GetComponent<SliderManager>();
        DepthOffsetSlider = GameObject.Find("DepthOffset").GetComponent<SliderManager>();
        RotationOffsetSlider = GameObject.Find("RotationOffset").GetComponentInChildren<RadialSlider>();
        SceneNameInputField = GameObject.Find("ScenarioNameInput").GetComponent<CustomInputField>();

        // --- NEW --- Find and assign scale input fields
        ScaleXInputField = GameObject.Find("ScaleXInputField").GetComponentInChildren<CustomInputField>();
        ScaleYInputField = GameObject.Find("ScaleYInputField").GetComponentInChildren<CustomInputField>();
        ScaleZInputField = GameObject.Find("ScaleZInputField").GetComponentInChildren<CustomInputField>();

        TotalDistanceSlider.mainSlider.onValueChanged.AddListener(OnTotalDistanceChanged);
        DistanceBetweenStepsSlider.mainSlider.onValueChanged.AddListener(OnDistanceBetweenStepsChanged);
        DoorScaleSlider.mainSlider.onValueChanged.AddListener(OnDoorScaleChanged);
        DepthOffsetSlider.mainSlider.onValueChanged.AddListener(OnDepthOffsetChanged);
        RotationOffsetSlider.onValueChanged.AddListener(OnRotationOffsetChanged);
        
        // --- NEW --- Add listeners for when the user edits the scale input fields
        ScaleXInputField.inputText.onEndEdit.AddListener(OnScaleXInput);
        ScaleYInputField.inputText.onEndEdit.AddListener(OnScaleYInput);
        ScaleZInputField.inputText.onEndEdit.AddListener(OnScaleZInput);

        // Find and assign all button references
        SceneConfigButtons[0] = GameObject.Find("Config").GetComponent<ButtonManager>();
        SceneConfigButtons[1] = GameObject.Find("Config (1)").GetComponent<ButtonManager>();
        SceneConfigButtons[2] = GameObject.Find("Config (2)").GetComponent<ButtonManager>();
        SceneConfigButtons[3] = GameObject.Find("Config (3)").GetComponent<ButtonManager>();
        SceneConfigButtons[4] = GameObject.Find("Config (4)").GetComponent<ButtonManager>();

        SaveSceneConfigButtons[0] = GameObject.Find("Save").GetComponent<ButtonManager>();
        SaveSceneConfigButtons[1] = GameObject.Find("Save (1)").GetComponent<ButtonManager>();
        SaveSceneConfigButtons[2] = GameObject.Find("Save (2)").GetComponent<ButtonManager>();
        SaveSceneConfigButtons[3] = GameObject.Find("Save (3)").GetComponent<ButtonManager>();
        SaveSceneConfigButtons[4] = GameObject.Find("Save (4)").GetComponent<ButtonManager>();

        // Set button texts
        for (int i = 0; i < SceneConfigButtons.Count; i++)
        {
            SceneConfigButtons[i].SetText(BuildStringFromConfig(i));
        }

        // Assign button click events to load/save methods
        for (int i = 0; i < SceneConfigButtons.Count; i++)
        {
            int index = i;
            SceneConfigButtons[i].onClick.AddListener(() => OnLoadSceneConfigButton(index));
        }

        for (int i = 0; i < SaveSceneConfigButtons.Count; i++)
        {
            Debug.Log($"Setting save button {SceneConfigButtons[i].name}");
            int index = i;
            SaveSceneConfigButtons[i].onClick.AddListener(() => OnSaveSceneConfigButton(index));
        }

        // --- NEW --- Subscribe to scale changes from the ReferenceStep (e.g., from editor gizmos)
        SC_StepReference.OnScaleChanged += OnReferenceStepScaleChanged;

        // Assign UI references
        ReferenceStep.transform.localScale = CurrentSceneConfig.StepScale;
        
        // Set sliders from current config
        UpdateUIFromConfig();
        
        StartCoroutine(IEInitializeSteps());
    }
    
    private void ClientSetup()
    {
        ovrManager = FindObjectOfType<OVRManager>();
    }

    private string BuildStringFromConfig(int index)
    {
        var config = SceneConfigPresets[index];
    
        string result = ""; 
        result += $"{config.PresetName}\n";
        result += $"Trial Distance: {config.TotalDistance:F1}\n";
        result += $"Stride Length: {config.DistanceBetweenSteps:F1}\n";
        result += $"Door Scale: {config.DoorScale:F1}\n";
        // only keep one decimal place for the scale
        result += $"Step Scale: {config.StepScale.x:F1} x {config.StepScale.y:F1} x {config.StepScale.z:F1}\n";
    
        return result;
    }

    private SC_Step CreateStep()
    {
        var step = Instantiate(StepPrefab, StepParent.transform);
        step.NetworkObject.Spawn();
        step.NetworkObject.TrySetParent(StepParent.GetComponent<NetworkObject>());
        return step;
    }

    private void OnGetStep(SC_Step step)
    {
        step.gameObject.SetActive(true);
        step.NetworkObject.TrySetParent(StepParent.GetComponent<NetworkObject>());
        UpdateStepActiveStateClientRpc(step.NetworkObject.NetworkObjectId, true);
    }

    private void OnReleaseStep(SC_Step step)
    {
        step.gameObject.SetActive(false);
        UpdateStepActiveStateClientRpc(step.NetworkObject.NetworkObjectId, false);
    }

    private void OnDestroyStep(SC_Step step)
    {
        step.NetworkObject.Despawn();
        Destroy(step.gameObject);
    }

    private IEnumerator IEInitializeSteps()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Initializing steps");
        UpdateSteps();
    }

    private void UpdateSteps()
    {
        int stepsCount = (int)(CurrentSceneConfig.TotalDistance / CurrentSceneConfig.DistanceBetweenSteps);

        for (int i = 0; i < stepsCount; i++)
        {
            SC_Step step;
            if (i < activeSteps.Count)
            {
                step = activeSteps[i];
            }
            else
            {
                step = stepPool.Get();
                activeSteps.Add(step);
            }

            step.transform.localPosition = new Vector3(0, 0, (i + 1) * CurrentSceneConfig.DistanceBetweenSteps);
            step.transform.localScale = ReferenceStep.transform.localScale;
        }

        for (int i = stepsCount; i < activeSteps.Count; i++)
        {
            stepPool.Release(activeSteps[i]);
        }
        activeSteps.RemoveRange(stepsCount, activeSteps.Count - stepsCount);
        
        Door.transform.localPosition = new Vector3(0, 0, CurrentSceneConfig.TotalDistance + CurrentSceneConfig.DistanceBetweenSteps);
    }
    
    private void UpdateDoorScale(float value)
    {
        Door.transform.localScale = new Vector3(value, Door.transform.localScale.y, Door.transform.localScale.z);
    }
    
    private void OnLoadSceneConfigButton(int index)
    {  
        // Store the current rotation and depth offsets to make them persistent across preset loads.
        float persistentRotationOffset = CurrentSceneConfig.RotationOffset;
        float persistentDepthOffset = CurrentSceneConfig.DepthOffset;

        // Load the scene config from the preset
        SceneNameInputField.inputText.text = SceneConfigPresets[index].PresetName;
        CurrentSceneConfig.TotalDistance = SceneConfigPresets[index].TotalDistance;
        CurrentSceneConfig.DistanceBetweenSteps = SceneConfigPresets[index].DistanceBetweenSteps;
        CurrentSceneConfig.StepScale = SceneConfigPresets[index].StepScale;
        CurrentSceneConfig.DoorScale = SceneConfigPresets[index].DoorScale;
        
        // Restore the persistent offsets
        CurrentSceneConfig.RotationOffset = persistentRotationOffset;
        CurrentSceneConfig.DepthOffset = persistentDepthOffset;

        // Update the UI and steps after loading
        UpdateUIFromConfig();
        UpdateSteps();
        
        if (Door != null)
        {
            UpdateDoorScale(CurrentSceneConfig.DoorScale);
        }

        Debug.Log($"Loaded Scene Config {index}");
    }

    private void OnSaveSceneConfigButton(int index)
    {
        // Update CurrentSceneConfig from the UI before saving
        UpdateCurrentFromUI();

        // Save the current config into the preset, EXCLUDING rotation and depth offset.
        SceneConfigPresets[index].PresetName = SceneNameInputField.inputText.text;
        SceneConfigPresets[index].TotalDistance = CurrentSceneConfig.TotalDistance;
        SceneConfigPresets[index].DistanceBetweenSteps = CurrentSceneConfig.DistanceBetweenSteps;
        SceneConfigPresets[index].StepScale = CurrentSceneConfig.StepScale;
        SceneConfigPresets[index].DoorScale = CurrentSceneConfig.DoorScale;

        // Update the button text to reflect new config
        SceneConfigButtons[index].SetText(BuildStringFromConfig(index));
        
        // set dirty flag to save the changes
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(SceneConfigPresets[index]);
        #endif
        
        Debug.Log($"Saved current config into Scene Config Preset {index}");
    }

    private void UpdateUIFromConfig()
    {
        if (TotalDistanceSlider != null)
            TotalDistanceSlider.mainSlider.value = CurrentSceneConfig.TotalDistance;
        if (DistanceBetweenStepsSlider != null)
            DistanceBetweenStepsSlider.mainSlider.value = CurrentSceneConfig.DistanceBetweenSteps;
        if (DoorScaleSlider != null)
            DoorScaleSlider.mainSlider.value = CurrentSceneConfig.DoorScale;
        if (DepthOffsetSlider != null)
            DepthOffsetSlider.mainSlider.value = CurrentSceneConfig.DepthOffset;
        if (RotationOffsetSlider != null)
            RotationOffsetSlider.currentValue = CurrentSceneConfig.RotationOffset;

        if (ReferenceStep != null)
            ReferenceStep.transform.localScale = CurrentSceneConfig.StepScale;
        
        // --- NEW --- Update scale input fields when config is loaded
        UpdateScaleInputFields();
            
        UpdateStepDoorParentTransform();
    }

    private void UpdateCurrentFromUI()
    {
        // Update current config from the UI elements before saving
        if (TotalDistanceSlider != null)
            CurrentSceneConfig.TotalDistance = TotalDistanceSlider.mainSlider.value;

        if (DistanceBetweenStepsSlider != null)
            CurrentSceneConfig.DistanceBetweenSteps = DistanceBetweenStepsSlider.mainSlider.value;
        
        if (DoorScaleSlider != null)
            CurrentSceneConfig.DoorScale = DoorScaleSlider.mainSlider.value;
            
        if(DepthOffsetSlider != null)
            CurrentSceneConfig.DepthOffset = DepthOffsetSlider.mainSlider.value;

        if (RotationOffsetSlider != null)
            CurrentSceneConfig.RotationOffset = RotationOffsetSlider.currentValue;

        if (ReferenceStep != null)
            CurrentSceneConfig.StepScale = ReferenceStep.transform.localScale;
    }

    private void OnDoorScaleChanged(float value)
    {
        CurrentSceneConfig.DoorScale = value;
        UpdateDoorScale(value);
    }
    
    private void OnTotalDistanceChanged(float value)
    {
        CurrentSceneConfig.TotalDistance = value;
        UpdateSteps();
    }

    private void OnDistanceBetweenStepsChanged(float value)
    {
        CurrentSceneConfig.DistanceBetweenSteps = value;
        UpdateSteps();
    }
    
    private void OnUserHeightOffsetChanged(float value)
    {
        UpdateUserHeightOffsetClientRpc(value);      
    }
    
    private void OnRotationOffsetChanged(float value)
    {
        CurrentSceneConfig.RotationOffset = value;
        UpdateStepDoorParentTransform();
    }
    
    private void OnDepthOffsetChanged(float value)
    {
        CurrentSceneConfig.DepthOffset = value;
        UpdateStepDoorParentTransform();
    }

    private void UpdateStepDoorParentTransform()
    {
        // Calculate and apply the rotation from the offset
        Quaternion newRotation = Quaternion.Euler(0, CurrentSceneConfig.RotationOffset, 0);
        StepDoorParent.localRotation = newRotation;

        // Calculate the direction vector based on the new rotation
        Vector3 forwardDirection = newRotation * Vector3.forward;

        // Calculate the new position by applying the depth offset along the local forward direction
        Vector3 newPosition = forwardDirection * CurrentSceneConfig.DepthOffset;

        // Preserve the original local Y position, as the offset should only affect the XZ plane
        newPosition.y = StepDoorParent.localPosition.y;

        // Apply the new, correctly calculated local position
        StepDoorParent.localPosition = newPosition;
    }

    private void OnApplicationQuit()
    {
        // Save current config on application quit (optional)
        UpdateCurrentFromUI();
    }
    
    // --- NEW METHODS ---

    // Called when the user finishes editing the X scale input field
    private void OnScaleXInput(string input)
    {
        if (float.TryParse(input, out float newScaleX))
        {
            ReferenceStep.transform.localScale = new Vector3(newScaleX, ReferenceStep.transform.localScale.y, ReferenceStep.transform.localScale.z);
        }
    }

    // Called when the user finishes editing the Y scale input field
    private void OnScaleYInput(string input)
    {
        if (float.TryParse(input, out float newScaleY))
        {
            ReferenceStep.transform.localScale = new Vector3(ReferenceStep.transform.localScale.x, newScaleY, ReferenceStep.transform.localScale.z);
        }
    }

    // Called when the user finishes editing the Z scale input field
    private void OnScaleZInput(string input)
    {
        if (float.TryParse(input, out float newScaleZ))
        {
            ReferenceStep.transform.localScale = new Vector3(ReferenceStep.transform.localScale.x, ReferenceStep.transform.localScale.y, newScaleZ);
        }
    }
    
    // Called when the ReferenceStep's scale changes from any source (e.g., gizmos)
    private void OnReferenceStepScaleChanged(Vector3 newScale)
    {
        UpdateScaleInputFields();
        CurrentSceneConfig.StepScale = newScale; // Keep the config in sync
    }

    // Updates the text of the input fields to reflect the current scale
    private void UpdateScaleInputFields()
    {
        if (ReferenceStep != null && ScaleXInputField != null && ScaleYInputField != null && ScaleZInputField != null)
        {
            Vector3 scale = ReferenceStep.transform.localScale;
            // Use SetTextWithoutNotify to prevent triggering onEndEdit events, which would cause a loop
            ScaleXInputField.inputText.SetTextWithoutNotify(scale.x.ToString("F2"));
            ScaleYInputField.inputText.SetTextWithoutNotify(scale.y.ToString("F2"));
            ScaleZInputField.inputText.SetTextWithoutNotify(scale.z.ToString("F2"));
        }
    }

    // --- END NEW METHODS ---

    [ClientRpc]
    private void UpdateStepActiveStateClientRpc(ulong stepNetworkObjectId, bool isActive)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(stepNetworkObjectId, out var networkObject))
        {
            var step = networkObject.GetComponent<SC_Step>();
            if (step != null)
            {
                step.UpdateActiveState(isActive);
            }
        }
    }
    
    [ClientRpc]
    private void UpdateUserHeightOffsetClientRpc(float value)
    {
        if (ovrManager != null)
            ovrManager.headPoseRelativeOffsetTranslation = new Vector3(0, value, 0);
    }
}