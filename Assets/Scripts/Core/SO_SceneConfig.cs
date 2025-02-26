using UnityEngine;

[SSSoftware.Attributes.Mutable]
[CreateAssetMenu(fileName = "SceneConfig", menuName = "SceneConfig", order = 1)]
public class SO_SceneConfig : SSSoftware.SOPro.ScriptableObject
{
    public string PresetName = "unnamed";
    
    public float TotalDistance = 10f;
    public float DistanceBetweenSteps = 1f;
    
    // from -180 to 180
    public float RotationOffset = 0f;
    
    public Vector3 StepScale = new Vector3(2,0.3f,0.3f);
}
