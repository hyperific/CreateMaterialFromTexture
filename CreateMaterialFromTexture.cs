//Code is an adaptation of a script from brombs73 on the unity forum
//Select a group of textures that should all belong to the same material. 
//Click the new "Create Complex Material" button in the Assets tab and presto! A new material with the textures you selected
//*NOTE* Not currently compatible with URP or HDRP

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
public class CreateMaterialFromTexture : EditorWindow
{
    ShaderType shaderType;

    [MenuItem("Window / Create Material from Textures")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreateMaterialFromTexture));
    }
    private void OnGUI()
    {
        //OnGUI Method Contents
        shaderType = (ShaderType)EditorGUILayout.EnumPopup("Shader Type", shaderType);
        GUILayout.Label("1. Select image textures from the project window", EditorStyles.boldLabel);
        GUILayout.Label("2. Set shader type", EditorStyles.boldLabel);
        GUILayout.Label("3. Press execute", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Execute", GUILayout.Width(100), GUILayout.Height(30)))
        {
            switch (shaderType)
            {
                case ShaderType.Standard:
                    CreateComplexDiffuseMaterial("Standard");
                    break;
                case ShaderType.URPLit:
                    CreateComplexDiffuseMaterial("Universal Render Pipeline/Lit");
                    break;
                case ShaderType.URPUnlit:
                    CreateComplexDiffuseMaterial("Universal Render Pipeline/Unlit");
                    break;
                default:
                    CreateComplexDiffuseMaterial("Standard");
                    break;
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public static void CreateComplexDiffuseMaterial(string type)
    {
        var selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        var cnt = selectedAsset.Length * 1.0f;
        var idx = 0f;
        List<Texture2D> tx2D = new List<Texture2D>();
        foreach (Object obj in selectedAsset)
        {
            idx++;
            EditorUtility.DisplayProgressBar("Create material", "Create material for: " + obj.name, idx / cnt);

            if (obj is Texture2D)
            {
                tx2D.Add(obj as Texture2D);
            }
        }
        CreateComplexMatFromTx(tx2D, Shader.Find(type));
        EditorUtility.ClearProgressBar();
    }
    static void CreateComplexMatFromTx(List<Texture2D> tx2D, Shader shader)
    {
        Texture2D albedo = null;
        Texture2D metallic = null;
        Texture2D normal = null;
        Texture2D occlusion = null;
        Texture2D specular = null;
        Texture2D emission = null;
        Texture2D height = null;
        foreach (Texture2D tex in tx2D)
        {
            string n = tex.name.ToLower();
            n = n.Substring(n.IndexOf("_"));
            if (n.Contains("albedo") || n.Contains("diffuse") || n.Contains("color"))
            {
                albedo = tex;
            }
            else if (n.Contains("metallic") || n.Contains("met"))
            {
                metallic = tex;
            }
            else if (n.Contains("specular") || n.Contains("spec"))
            {
                specular = tex;
            }
            else if (n.Contains("normal") || n.Contains("nrm"))
            {
                normal = tex;
            }
            else if (n.Contains("occlusion") || n.Contains("occ"))
            {
                occlusion = tex;
            }
            else if (n.Contains("emission"))
            {
                emission = tex;
            }
            else if (n.Contains("height") || n.Contains("parallax"))
            {
                height = tex;
            }
        }
        var path = AssetDatabase.GetAssetPath(tx2D[0]);
        if (File.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }
        var mat = new Material(shader) { mainTexture = albedo };
        if (emission != null)
        {
            mat.EnableKeyword("_EmissionMap");
            Debug.Log("Using emission");
        }
        if (height != null)
        {
            mat.EnableKeyword("_ParallaxMap");
            Debug.Log("Using heigh map");
        }
        mat.SetTexture("_BumpMap", normal);
        mat.SetTexture("_SpecGlossMap", specular);
        mat.SetTexture("_MetallicGlossMap", metallic);
        mat.SetTexture("_OcclusionMap", occlusion);
        mat.SetTexture("_EmissionMap", emission);
        mat.SetTexture("_ParallaxMap", height);
        string matTitle = albedo.name.Split('_')[0];
        AssetDatabase.CreateAsset(mat, Path.Combine(path, string.Format("{0}.mat", matTitle)));
    }
}
public enum ShaderType
{
    Standard,
    URPLit,
    URPUnlit
}



