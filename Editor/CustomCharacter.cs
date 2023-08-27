using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Elly.Editor
{
    public sealed class CustomCharacter : EditorWindow
    {
        public string editorWindowText = "Choose a project name: ";
        public int createCode = 0;
        string debug = "";
        string inputText = "";

        public const string template = @"using Elly.Runtime;
using UnityEngine;

public class %%%% : CharacterBase
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }
}
";
        public const string nonetemplate = @"using Elly.Runtime;
using UnityEngine;

public class %%%% : NoneCameraCharacter
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }
}
";

        void OnGUI()
        {
            inputText = EditorGUILayout.TextField(editorWindowText, inputText);

            Regex rex = new Regex(@"^[a-zA-Z0-9](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])$");
            GUI.enabled = rex.IsMatch(inputText) && inputText.Length != 0;

            if (GUILayout.Button("Create"))
                Create(inputText);

            GUI.enabled = true;

            if (GUILayout.Button("Abort"))
                Close();

            EditorGUILayout.LabelField(debug);
        }

        private void Create(string name)
        {
            string filePath;
            if (Selection.assetGUIDs.Length == 0)
                filePath = "Assets/New Custom Character";
            else
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            string path = Path.Combine(filePath, name + ".cs");
            if(name.Length == 0)
            {
                debug = $"You must enter the file name";
                return;
            }
            if (File.Exists(path))
            {
                debug = $"{path} already exist";
            }
            else
            {
                if (createCode == 0) File.WriteAllText(path, template.Replace("%%%%", name));
                else if (createCode == 1) File.WriteAllText(path, nonetemplate.Replace("%%%%", name));
                AssetDatabase.Refresh();
                Close();
            }
        }

        [MenuItem("Assets/Create/Elly/Custom Character", false, 1)]
        public static void CreateCustomCharacter()
        {
            CustomCharacter window = new CustomCharacter();
            window.createCode = 0;
            window.titleContent = new GUIContent("Create Custom Character");
            window.minSize = new Vector2(400, 70);
            window.maxSize = new Vector2(400, 70);
            window.ShowUtility();
        }

        [MenuItem("Assets/Create/Elly/Custom None Camera Character", false, 1)]
        public static void CreateCustomNoneCameraCharacter()
        {
            CustomCharacter window = new CustomCharacter();
            window.createCode = 1;
            window.titleContent = new GUIContent("Create Custom Character");
            window.minSize = new Vector2(400, 70);
            window.maxSize = new Vector2(400, 70);
            window.ShowUtility();
        }
    }
}
