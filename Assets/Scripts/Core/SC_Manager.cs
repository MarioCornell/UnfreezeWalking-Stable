using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Michsky.MUIP;
using Unity.Netcode;

public class SC_Manager : NetworkBehaviour
{
    public SO_SceneConfig CurrentSceneConfig;
    public List<SO_SceneConfig> SceneConfigPresets;
    public List<ButtonManager> SceneConfigButtons;
    public List<ButtonManager> SaveSceneConfigButtons;
    
    public SliderManager TotalDistanceSlider;
    public SliderManager DistanceBetweenStepsSlider;
    public SliderManager DoorScaleSlider;
    public RadialSlider RotationOffsetSlider;
    
    public Transform StepDoorParent;
    public GameObject ReferenceStep;
    public GameObject StepParent;
    public SC_Step StepPrefab;
    public GameObject Door;
    public CustomInputField SceneNameInputField;
    
    private bool SetupComplete = false;
    private ObjectPool<SC_Step> stepPool;
    private List<SC_Step> activeSteps = new List<SC_Step>();

    private OVRManager ovrManager;

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

    private void ServerSetup()
    {
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
        RotationOffsetSlider = GameObject.Find("RotationOffset").GetComponentInChildren<RadialSlider>();
        SceneNameInputField = GameObject.Find("ScenarioNameInput").GetComponent<CustomInputField>();
        
        TotalDistanceSlider.mainSlider.onValueChanged.AddListener(OnTotalDistanceChanged);
        DistanceBetweenStepsSlider.mainSlider.onValueChanged.AddListener(OnDistanceBetweenStepsChanged);
        DoorScaleSlider.mainSlider.onValueChanged.AddListener(OnDoorScaleChanged);
        RotationOffsetSlider.onValueChanged.AddListener(OnRotationOffsetChanged);
        
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
        // Load the scene config from the preset
        SceneNameInputField.inputText.text = SceneConfigPresets[index].PresetName;
        CurrentSceneConfig.TotalDistance = SceneConfigPresets[index].TotalDistance;
        CurrentSceneConfig.DistanceBetweenSteps = SceneConfigPresets[index].DistanceBetweenSteps;
        CurrentSceneConfig.StepScale = SceneConfigPresets[index].StepScale;
        CurrentSceneConfig.RotationOffset = SceneConfigPresets[index].RotationOffset;
        CurrentSceneConfig.DoorScale = SceneConfigPresets[index].DoorScale;

        // Update the UI and steps after loading
        UpdateUIFromConfig();
        UpdateSteps();
        StepDoorParent.localEulerAngles = new Vector3(0, CurrentSceneConfig.RotationOffset, 0);
        
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

        // Save the current config into the preset
        SceneConfigPresets[index].PresetName = SceneNameInputField.inputText.text;
        SceneConfigPresets[index].TotalDistance = CurrentSceneConfig.TotalDistance;
        SceneConfigPresets[index].DistanceBetweenSteps = CurrentSceneConfig.DistanceBetweenSteps;
        SceneConfigPresets[index].StepScale = CurrentSceneConfig.StepScale;
        SceneConfigPresets[index].RotationOffset = CurrentSceneConfig.RotationOffset;
        SceneConfigPresets[index].DoorScale = CurrentSceneConfig.DoorScale;

        // Update the button text to reflect new config
        SceneConfigButtons[index].SetText(BuildStringFromConfig(index));
        
        // set dirty flag to save the changes
        # if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(SceneConfigPresets[index]);
        # endif
        
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
        if (RotationOffsetSlider != null)
            RotationOffsetSlider.currentValue = CurrentSceneConfig.RotationOffset;

        if (ReferenceStep != null)
            ReferenceStep.transform.localScale = CurrentSceneConfig.StepScale;

        StepDoorParent.localEulerAngles = new Vector3(0, CurrentSceneConfig.RotationOffset, 0);
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
        StepDoorParent.localEulerAngles = new Vector3(0, value, 0);
    }

    private void OnApplicationQuit()
    {
        // Save current config on application quit (optional)
        UpdateCurrentFromUI();
    }

    [ClientRpc]
    private void UpdateStepActiveStateClientRpc(ulong stepNetworkObjectId, bool isActive)
    {
        var step = NetworkManager.SpawnManager.SpawnedObjects[stepNetworkObjectId].GetComponent<SC_Step>();
        if (step != null)
        {
            step.UpdateActiveState(isActive);
        }
    }
    
    [ClientRpc]
    private void UpdateUserHeightOffsetClientRpc(float value)
    {
        if (ovrManager != null)
            ovrManager.headPoseRelativeOffsetTranslation = new Vector3(0, value, 0);
    }
}
