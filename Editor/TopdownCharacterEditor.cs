using Elly.Runtime;
using UnityEditor;
using UnityEngine;

namespace Elly.Editor
{
    [CustomEditor(typeof(TopdownCharacter), true)]
    [CanEditMultipleObjects]
    public class TopdownCharacterEditor : CharacterBaseEditor
    {
        private TopdownCharacter TDtargetSource;

        protected override void OnEnable()
        {
            base.OnEnable();
            TDtargetSource = target as TopdownCharacter;
        }

        protected override void DrawChild()
        {
            float start = serializedObject.FindProperty("StartDistance").floatValue;
            float min = serializedObject.FindProperty("MinDistance").floatValue;
            float max = serializedObject.FindProperty("MaxDistance").floatValue;
            EditorGUILayout.MinMaxSlider($"Distance range ({min.ToString(".0")}, {max.ToString(".0")})", ref min, ref max, TDtargetSource.MinimumDistanceBounds, TDtargetSource.MaximumDistanceBounds);
            start = Mathf.Clamp(start, min, max);
            start = EditorGUILayout.Slider("Start distance", start, min, max);
            serializedObject.FindProperty("StartDistance").floatValue = start;
            serializedObject.FindProperty("MinDistance").floatValue = min;
            serializedObject.FindProperty("MaxDistance").floatValue = max;
            
            if(serializedObject.targetObject != null)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
