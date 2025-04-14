using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace BKPureNature
{
    public class MaterialReplacer : EditorWindow
{
    private Material originalMaterial;
    private Material newMaterial;

    [MenuItem("Tools/BK/Material Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MaterialReplacer>("Material Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Material in Selected LODs", EditorStyles.boldLabel);

        originalMaterial = (Material)EditorGUILayout.ObjectField("Original Material", originalMaterial, typeof(Material), false);
        newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), false);

        if (GUILayout.Button("Replace Materials"))
        {
            ReplaceMaterialsInSelected();
        }
    }

    void ReplaceMaterialsInSelected()
    {
        if (originalMaterial == null || newMaterial == null)
        {
            Debug.LogWarning("Both Original and New materials must be set.");
            return;
        }

        Undo.SetCurrentGroupName("Replace Materials");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject obj in Selection.gameObjects)
        {
            ReplaceMaterialsInLODs(obj);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    void ReplaceMaterialsInLODs(GameObject gameObject)
    {
        LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            bool hasChanged = false; // Flag to check if any material was replaced

            foreach (LOD lod in lodGroup.GetLODs())
            {
                foreach (Renderer renderer in lod.renderers)
                {
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] == originalMaterial)
                        {
                            materials[i] = newMaterial;
                            hasChanged = true; // Material was replaced, set the flag
                        }
                    }
                    Undo.RecordObject(renderer, "Material Replacement");
                    renderer.sharedMaterials = materials;
                }
            }

            // Check if the GameObject is part of a prefab and if any material was replaced
            if (hasChanged)
            {
                // Check if the GameObject is a prefab instance and apply changes
                if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);

                    // Alternatively, if you want to apply all modifications to the prefab asset, you can use:
                    // PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
                }
            }
        }
    }
}
}
