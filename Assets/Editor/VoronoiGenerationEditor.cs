using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiMapGenerator))]
public class VoronoiGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VoronoiMapGenerator voronoiGen = (VoronoiMapGenerator)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            voronoiGen.GenerateMap();
        }
    }
}
