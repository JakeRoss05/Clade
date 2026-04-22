using UnityEditor;
using UnityEngine;

public static class RepairMaterialShaders
{
    [MenuItem("Tools/Clade/Repair Material Shaders")]
    public static void RepairAllMaterialShaders()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Shader urpSimpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");

        if (urpLit == null && urpSimpleLit == null)
        {
            Debug.LogError("[RepairMaterialShaders] Could not find URP shaders. Ensure URP is installed and loaded.");
            return;
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int repaired = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                continue;
            }

            string shaderName = mat.shader != null ? mat.shader.name : "<null>";
            bool needsRepair = mat.shader == null
                || shaderName == "Hidden/InternalErrorShader"
                || shaderName == "Standard"
                || shaderName == "Standard (Specular setup)";

            if (!needsRepair)
            {
                continue;
            }

            Shader replacement = urpLit != null ? urpLit : urpSimpleLit;
            mat.shader = replacement;
            EditorUtility.SetDirty(mat);
            repaired++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RepairMaterialShaders] Repaired {repaired} material(s).\nIf any still appear pink, right-click them in Project and choose Reimport.");
    }
}
