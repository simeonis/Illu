using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomBuilder))]
public class RandomBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RandomBuilder builder = (RandomBuilder) target;
        
        // GameObject Settings
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        builder.Index = EditorGUILayout.Popup(builder.Index, builder.Options);
        if (GUILayout.Button(builder.IsStatic ? "Static" : "Not Static"))
            builder.IsStatic = !builder.IsStatic;
        GUILayout.EndHorizontal();

        // Custom GUI per option
        switch(builder.Index)
        {
            // Cube
            case 0:
                builder.CustomMaterial = (Material) EditorGUILayout.ObjectField("Material", builder.CustomMaterial, typeof(Material), true);
                builder.Amount = EditorGUILayout.IntSlider("Number Of Points", builder.Amount, 0, 500);
                builder.BoundSize = EditorGUILayout.Vector3Field("Bound Size", builder.BoundSize);
                DrawGUIScaling(builder);
                DrawGUIPivot(builder);
                break;
            // Sphere
            case 1:
                builder.CustomMaterial = (Material) EditorGUILayout.ObjectField("Material", builder.CustomMaterial, typeof(Material), true);
                builder.Amount = EditorGUILayout.IntSlider("Number Of Points", builder.Amount, 0, 500);
                builder.BoundSize = EditorGUILayout.Vector3Field("Bound Size", builder.BoundSize);
                DrawGUIScaling(builder);
                DrawGUIPivot(builder);
                break;
            // Cylinder
            case 2:
                builder.CustomMaterial = (Material) EditorGUILayout.ObjectField("Material", builder.CustomMaterial, typeof(Material), true);
                builder.Amount = EditorGUILayout.IntSlider("Number Of Points", builder.Amount, 0, 500);
                builder.BoundSize = EditorGUILayout.Vector3Field("Bound Size", builder.BoundSize);
                DrawGUIScaling(builder);
                DrawGUIPivot(builder);
                break;
            // Custom
            default:
                builder.CustomObject = (GameObject) EditorGUILayout.ObjectField("GameObject", builder.CustomObject, typeof(GameObject), true);
                builder.Amount = EditorGUILayout.IntSlider("Number Of Points", builder.Amount, 0, 100);
                builder.BoundSize = EditorGUILayout.Vector3Field("Bound Size", builder.BoundSize);
                DrawGUIScaling(builder);
                break;
        }

        // Spacing
        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        // Visualization Settings
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);
        builder.BoundColor = EditorGUILayout.ColorField("Bound Color", builder.BoundColor);
        builder.PointColor = EditorGUILayout.ColorField("Point Color", builder.PointColor);
        if (GUILayout.Button(builder.Visualize ? "Hide Visualization" : "Show Visualization"))
            builder.Visualize = !builder.Visualize;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomize"))
            builder.Randomize();
        if (GUILayout.Button("Build"))
            builder.Build();
        GUILayout.EndHorizontal();
    }

    void DrawGUIScaling(RandomBuilder builder)
    {
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        EditorGUILayout.LabelField("Object Scale", EditorStyles.boldLabel);
        if (builder.UniformScaling)
        {
            GUILayout.BeginHorizontal();
            builder.MinWidth = EditorGUILayout.FloatField("Min Size:", builder.MinWidth);
            builder.MaxWidth = EditorGUILayout.FloatField("Max Size:", builder.MaxWidth);
            GUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref builder.MinWidth, ref builder.MaxWidth, 0.01f, 100f);
        }
        else
        {
            GUILayout.BeginHorizontal();
            builder.MinWidth = EditorGUILayout.FloatField("Min Width:", builder.MinWidth);
            builder.MaxWidth = EditorGUILayout.FloatField("Max Width:", builder.MaxWidth);
            GUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref builder.MinWidth, ref builder.MaxWidth, 0.01f, 100f);
            GUILayout.BeginHorizontal();
            builder.MinHeight = EditorGUILayout.FloatField("Min Height:", builder.MinHeight);
            builder.MaxHeight = EditorGUILayout.FloatField("Max Height:", builder.MaxHeight);
            GUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref builder.MinHeight, ref builder.MaxHeight, 0.01f, 100f);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(builder.UniformScaling ? "Set Scaling: Divergent" : "Set Scaling: Uniform", GUILayout.Width(185)))
            builder.UniformScaling = !builder.UniformScaling;
        EditorGUILayout.LabelField($"(Current Scaling: {(builder.UniformScaling ? "Uniform" : "Divergent")})");
        GUILayout.EndHorizontal();
    }

    void DrawGUIPivot(RandomBuilder builder)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(builder.CenterPivot ? "Set Pivot: Base" : "Set Pivot: Center", GUILayout.Width(185)))
            builder.CenterPivot = !builder.CenterPivot;
        EditorGUILayout.LabelField($"(Current Pivot: {(builder.CenterPivot ? "Center" : "Base")})");
        GUILayout.EndHorizontal();
    }
}
