using Elly.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Elly.Editor
{
    [CustomEditor(typeof(CharacterBase), true)]
    [CanEditMultipleObjects]
    public class CharacterBaseEditor : UnityEditor.Editor
    {
        private static Dictionary<string, Type> BuiltInCharacterTypeDict
        {
            get
            {
                if(_BuiltInCharacterTypeDict == null)
                {
                    Dictionary<string, Type> result = new Dictionary<string, Type>();
                    foreach (var j in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var i in j.GetTypes())
                        {
                            if (i.IsSubclassOf(typeof(CharacterBase)) && !i.IsAbstract && i.GetCustomAttribute<BuiltInScript>() != null) result[i.Name] = i;
                        }
                    }
                    _BuiltInCharacterTypeDict = result;
                }
                return _BuiltInCharacterTypeDict;
            }
        }
        private static Dictionary<string, Type> _BuiltInCharacterTypeDict = null;

        private static Dictionary<string, Type> CustomCharacterTypeDict
        {
            get
            {
                if (_CustomCharacterTypeDict == null)
                {
                    Dictionary<string, Type> result = new Dictionary<string, Type>();
                    foreach (var j in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var i in j.GetTypes())
                        {
                            if (i.IsSubclassOf(typeof(CharacterBase)) && !i.IsAbstract && i.GetCustomAttribute<BuiltInScript>() == null) result[i.Name] = i;
                        }
                    }
                    _CustomCharacterTypeDict = result;
                }
                return _CustomCharacterTypeDict;
            }
        }
        private static Dictionary<string, Type> _CustomCharacterTypeDict = null;

        private static string[] ScriptType = new string[] { "Built-In", "Customize" };


        public static void DrawCharacterBaseMenu(SerializedObject so)
        {
            CharacterBase cb = so.targetObject as CharacterBase;
            EditorGUILayout.PropertyField(so.FindProperty("Controller"));
            EditorGUILayout.PropertyField(so.FindProperty("Vibration"));
            GUI.enabled = false;
            EditorGUILayout.ObjectField(so.FindProperty("m_Script"));
            EditorGUILayout.ObjectField("Camera Rigging", cb._CameraRigging, typeof(Transform), true);
            EditorGUILayout.ObjectField("Camera", cb._Camera, typeof(Camera), true);
            GUI.enabled = true;
        }

        static float a;

        public static void DrawMenu(CharacterBase targetSource)
        {
            EditorGUILayout.Space();
            DrawSwitch(targetSource);
            Message(targetSource);

        }
        private CharacterBase targetSource;
        private static int type = 0;
        private static int script = 0;

        protected virtual void OnEnable()
        {
            targetSource = target as CharacterBase;
        }

        public override void OnInspectorGUI()
        {
            bool donotneedcamera = targetSource.GetType().GetCustomAttribute<DoNotNeedCamera>() != null;

            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            if (!donotneedcamera)
            {
                bool startdrawing = false;
                SerializedProperty it = serializedObject.GetIterator();
                it.Next(true);

                while (it.Next(false))
                {
                    if (it.name == "Controller") startdrawing = true;

                    if(startdrawing)
                        EditorGUILayout.PropertyField(it);

                    if (it.name == "Vibration")
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("Camera Rigging", targetSource._CameraRigging, typeof(Transform), true);
                        EditorGUILayout.ObjectField("Camera", targetSource._Camera, typeof(Camera), true);
                        GUI.enabled = true;

                        DrawChild();
                    }
                }
            }
            EditorGUILayout.Space();
            DrawSwitch(targetSource);
            Message(targetSource);
            if (serializedObject.targetObject != null)
                serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawChild()
        {

        }

        private static void DrawSwitch(CharacterBase targetSource)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            script = EditorGUILayout.Popup("Type", script, ScriptType);
            switch (script)
            {
                case 0: // Built in
                    {
                        var buffer = BuiltInCharacterTypeDict;
                        var array = buffer.Keys.ToArray();
                        type = EditorGUILayout.Popup("Switch", type, array);
                        if (GUILayout.Button("Switch"))
                        {
                            GameObject g = targetSource.gameObject;
                            targetSource.SwitchType(buffer[array[type]]);
                            EditorUtility.SetDirty(g);
                        }
                        break;
                    }
                case 1: // Customize
                    {
                        var buffer = CustomCharacterTypeDict;
                        var array = buffer.Keys.ToArray();
                        type = EditorGUILayout.Popup("Switch", type, array);
                        if (GUILayout.Button("Switch"))
                        {
                            GameObject g = targetSource.gameObject;
                            targetSource.SwitchType(buffer[array[type]]);
                            EditorUtility.SetDirty(g);
                        }
                        break;
                    }
            }
            EditorGUILayout.EndVertical();
        }
        private static void Message(CharacterBase targetSource)
        {
            bool donotneedcamera = targetSource.GetType().GetCustomAttribute<DoNotNeedCamera>() != null;

            if (!donotneedcamera)
            {
                if (!targetSource._CameraRigging)
                {
                    EditorGUILayout.HelpBox("Missing camera center child\nThis component won't enable to use camera related method if missing the object", MessageType.Warning);
                    if (GUILayout.Button("Fix"))
                    {
                        CharacterEditor.ApplyCameraStructure(targetSource.CameraRiggingRoot()).SetAsFirstSibling();
                        EditorUtility.SetDirty(targetSource.gameObject);
                    }
                }
                else
                {
                    if (!targetSource._Camera)
                    {
                        EditorGUILayout.HelpBox("Missing camera child\nThis component won't enable to use camera related method if missing the camera", MessageType.Warning);
                        if (GUILayout.Button("Fix"))
                        {
                            Transform t = targetSource._CameraRigging;
                            DestroyImmediate(t.gameObject);
                            CharacterEditor.ApplyCameraStructure(targetSource.CameraRiggingRoot()).SetAsFirstSibling();
                            EditorUtility.SetDirty(targetSource.gameObject);
                        }
                    }
                }
            }
        }
    }
}
