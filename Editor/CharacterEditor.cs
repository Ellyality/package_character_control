using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using Elly.Runtime;
using System.Diagnostics;

namespace Elly.Editor
{
    /// <summary>
    /// Hierachy menu item for create character
    /// </summary>
    public static class CharacterEditor
    {
        private const string docs = "https://elly2018.github.io/Elly.Character/";

        [MenuItem("Elly/Docs/Character")]
        public static void Docs()
        {
            Process.Start(docs);
        }

        [MenuItem("GameObject/Elly/Character/Default AI Character", false, 1)]
        public static void CreateNoneCameraCharacter()
        {
            Transform t = CreateRootObject("Default AI Character");
            Transform me = ApplyCharacterMesh(t);
            t.gameObject.AddComponent<DefaultAICharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        [MenuItem("GameObject/Elly/Character/First-Person Character", false)]
        public static void CreateFPSCharacterWithCamera()
        {
            Transform t = CreateRootObject("First-Person Character");
            Transform cc = ApplyCameraStructure(t);
            Transform me = ApplyCharacterMesh(t);
            cc.transform.localPosition = new Vector3(0, 0, 1);
            t.gameObject.AddComponent<FPSCharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        [MenuItem("GameObject/Elly/Character/Third-Person Character", false)]
        public static void CreateThirdPersonCharacterWithCamera()
        {
            Transform t = CreateRootObject("Third-Person Character");
            Transform cc = ApplyCameraStructure(t);
            Transform me = ApplyCharacterMesh(t);
            t.gameObject.AddComponent<TPSCharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        [MenuItem("GameObject/Elly/Character/Top-Down Character", false)]
        public static void CreateTopDownCharacter()
        {
            Transform t = CreateRootObject("Top-Down Character");
            Transform cc = ApplyCameraStructure(t);
            t.gameObject.AddComponent<TopdownCharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        [MenuItem("GameObject/Elly/Character/Ghost Character", false)]
        public static void CreateGhostCharacter()
        {
            Transform t = CreateRootObject("Ghost Character");
            Transform cc = ApplyCameraStructure(t);
            cc.transform.localPosition = new Vector3(0, 0, 0);
            t.gameObject.AddComponent<GhostCharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        [MenuItem("GameObject/Elly/Character/Platform Character", false)]
        public static void CreatePlatformCharacter()
        {
            Transform t = CreateRootObject("Platform Character Root");
            GameObject character = new GameObject("Platform Character");
            character.transform.SetParent(t);

            Transform cc = ApplyCameraStructure(t);
            Transform me = ApplyCharacterMesh(character.transform);
            cc.transform.localPosition = new Vector3(0, 0, 0);

            character.transform.gameObject.AddComponent<PlatformCharacter>();
            Undo.RegisterCreatedObjectUndo(t.gameObject, "Create " + t.name);
        }

        /// <summary>
        /// Create root and apply to current selection
        /// </summary>
        /// <param name="name">GameObject name</param>
        /// <returns>Root transform</returns>
        internal static Transform CreateRootObject(string name)
        {
            GameObject go = new GameObject(name);
            Transform t = Selection.activeTransform;
            if (t)
            {
                go.transform.SetParent(t);
                Selection.activeTransform = go.transform;
            }
            return go.transform;
        }
        /// <summary>
        /// Apply transform camera object tree structure <br />
        /// Root - Camera center - Camera <br />
        /// </summary>
        /// <returns>Camera center's transform</returns>
        internal static Transform ApplyCameraStructure(Transform root)
        {
            GameObject go = new GameObject("Camera Rigging");
            go.transform.SetParent(root);
            GameObject cam = new GameObject("Camera");
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.AddComponent<UniversalAdditionalCameraData>();
            cam.transform.SetParent(go.transform);
            return go.transform;
        }
        /// <summary>
        /// Apply transform mesh capsule under root <br />
        /// Root - Mesh
        /// </summary>
        /// <returns>Mesh's transform</returns>
        internal static Transform ApplyCharacterMesh(Transform root)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Mesh";
            go.transform.SetParent(root);
            return go.transform;
        }
    }
}
