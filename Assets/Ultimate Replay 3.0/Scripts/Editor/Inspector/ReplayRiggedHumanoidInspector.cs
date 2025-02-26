using UnityEngine;
using UnityEditor;

namespace UltimateReplay
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReplayRiggedHumanoid))]
    public class ReplayRiggedHumanoidInspector : ReplayRecordableBehaviourInspector
    {
        // Private
        private SerializedProperty observedAnimator = null;
        private SerializedProperty observedRoot = null;

        private SerializedProperty bodyPosition = null;
        private SerializedProperty bodyPositionPrecision = null;
        private SerializedProperty bodyRotation = null;
        private SerializedProperty bodyRotationPrecision = null;
        private SerializedProperty interpolateMuscleValues = null;
        private SerializedProperty muscleValuePrecision = null;

        // Methods
        public override void OnEnable()
        {
            base.OnEnable();

            // Find properties
            observedAnimator = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.observedAnimator));
            observedRoot = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.observedRoot));

            bodyPosition = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.replayBodyPosition));
            bodyPositionPrecision = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.bodyPositionPrecision));
            bodyRotation = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.replayBodyRotation));
            bodyRotationPrecision = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.bodyRotationPrecision));
            interpolateMuscleValues = serializedObject.FindProperty(nameof(ReplayRiggedHumanoid.interpolateMuscleValues));
            muscleValuePrecision = serializedObject.FindProperty(nameof(muscleValuePrecision));
        }

        public override void OnInspectorGUI()
        {
            float precisionUIWidth = 44;

            // Display main properties
            DisplayDefaultInspectorProperties();

            // Check for changed
            EditorGUI.BeginChangeCheck();


            // Draw body position
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(bodyPosition);
                EditorGUILayout.PropertyField(bodyPositionPrecision, GUIContent.none, GUILayout.Width(precisionUIWidth));
            }
            GUILayout.EndHorizontal();

            // Draw body rotation
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(bodyRotation);
                EditorGUILayout.PropertyField(bodyRotationPrecision, GUIContent.none, GUILayout.Width(precisionUIWidth));
            }
            GUILayout.EndHorizontal();

            // Muscle precision
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(interpolateMuscleValues, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                if(Screen.width > 330)
                    GUILayout.Label("Muscle Precision");
                EditorGUILayout.PropertyField(muscleValuePrecision, GUIContent.none, GUILayout.Width(precisionUIWidth));
            }
            GUILayout.EndHorizontal();


            // Update changes
            if (EditorGUI.EndChangeCheck() == true)
                serializedObject.ApplyModifiedProperties();

            // Draw data statistics
            DisplayReplayStorageStatistics();
        }
    }
}
