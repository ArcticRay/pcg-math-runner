using UnityEngine;
using UnityEditor;
using BKPureNature;

namespace BKPureNature
{
[CustomEditor(typeof(BK_EnvironmentManager))]
public class BK_EnvironmentManagerEditor : Editor
{
    private Editor materialEditor;

    private SerializedProperty overrideSunColorProp;
    private SerializedProperty overrideFogColorProp;
    private SerializedProperty overrideCloudColorProp;

    private bool lightingFoldout = true;
    private bool windFoldout = true;
    private bool grassFoldout = true;
    private bool cloudsFoldout = true;

    private void OnEnable()
    {
        overrideSunColorProp = serializedObject.FindProperty("overrideSunColor");
        overrideFogColorProp = serializedObject.FindProperty("overrideFogColor");
        overrideCloudColorProp = serializedObject.FindProperty("overrideCloudColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.margin = new RectOffset(2, 2, 2, 2);
        boxStyle.padding = new RectOffset(5, 5, 5, 5);

        BK_EnvironmentManager EMscript = (BK_EnvironmentManager)target;

        // Lighting Foldout
EditorGUILayout.Space(10);
EditorGUILayout.BeginVertical(boxStyle);
lightingFoldout = EditorGUILayout.Foldout(lightingFoldout, "Global Lighting", foldoutStyle);
if (lightingFoldout)
{
    // Directional Light property
    EditorGUILayout.PropertyField(serializedObject.FindProperty("directionalLight"), new GUIContent("Directional Light"));

    // Sun Gradient
    EditorGUILayout.BeginHorizontal();
    overrideSunColorProp.boolValue = EditorGUILayout.ToggleLeft("Sun", overrideSunColorProp.boolValue, GUILayout.Width(70));
    using (new EditorGUI.DisabledScope(!overrideSunColorProp.boolValue))
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sunColorGradient"), GUIContent.none);
    }
    EditorGUILayout.EndHorizontal();

    // Fog Gradient
    EditorGUILayout.BeginHorizontal();
    overrideFogColorProp.boolValue = EditorGUILayout.ToggleLeft("Fog", overrideFogColorProp.boolValue, GUILayout.Width(70));
    using (new EditorGUI.DisabledScope(!overrideFogColorProp.boolValue))
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColorGradient"), GUIContent.none);
    }
    EditorGUILayout.EndHorizontal();

    // Clouds Gradient
    EditorGUILayout.BeginHorizontal();
    overrideCloudColorProp.boolValue = EditorGUILayout.ToggleLeft("Clouds", overrideCloudColorProp.boolValue, GUILayout.Width(70));
    using (new EditorGUI.DisabledScope(!overrideCloudColorProp.boolValue))
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scatteringColorGradient"), GUIContent.none);
    }
    Rect gradientRect = GUILayoutUtility.GetLastRect();
    EditorGUILayout.EndHorizontal();

    // Day & Night icons
    EditorGUILayout.BeginHorizontal();
    GUILayout.Space(gradientRect.x + 77);
    GUILayout.Label("☼", GUILayout.Width(20));
    GUILayout.FlexibleSpace();
    GUILayout.Label("☽", GUILayout.Width(20));
    GUILayout.Space(5);
    EditorGUILayout.EndHorizontal();
}
EditorGUILayout.EndVertical();


        // Wind Foldout
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(boxStyle);
        windFoldout = EditorGUILayout.Foldout(windFoldout, "Wind", foldoutStyle);
        if (windFoldout)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseWindPower"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseWindSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsPower"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("burstsScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("microPower"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("microSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("microFrequency"));
        }
        EditorGUILayout.EndVertical();

        // Grass Foldout
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(boxStyle);
        grassFoldout = EditorGUILayout.Foldout(grassFoldout, "Grass", foldoutStyle);
        if (grassFoldout)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("renderDistance"));
        }
        EditorGUILayout.EndVertical();

        // Clouds Foldout
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(boxStyle);
        cloudsFoldout = EditorGUILayout.Foldout(cloudsFoldout, "Clouds", foldoutStyle);
        if (cloudsFoldout)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Altitude"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeSamples"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeSize"));

            // Material
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudsMaterial"));

            // Add a space
            EditorGUILayout.Space(10);

            if (EMscript.cloudsMaterial != null)
            {
                if (materialEditor == null || materialEditor.target != EMscript.cloudsMaterial)
                {
                    materialEditor = Editor.CreateEditor(EMscript.cloudsMaterial);
                }
                materialEditor.DrawHeader();
                materialEditor.OnInspectorGUI();
            }
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
}