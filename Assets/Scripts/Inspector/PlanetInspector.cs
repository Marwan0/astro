﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetInspector : Editor {

    private Planet planet;
    private string file = "Assets/ScalarData/Planets/MyAwesomeFile.dat";

    public override void OnInspectorGUI()
    {
        planet = (Planet)target;

        DrawDefaultInspector();
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Generate New"))
        {
            planet.Generate();
        }
        if(GUILayout.Button("Build"))
        {
            planet.Build();
        }
        if (GUILayout.Button("Build Async"))
        {
            planet.BuildAsync();
        }
        if (GUILayout.Button("Clear"))
        {
            planet.Clear();
        }
        if (GUILayout.Button("Save Fields"))
        {
            PlanetUtils.Save(planet.num, file, planet.chunks);
        }

        if(GUILayout.Button("Load Fields"))
        {
            PlanetUtils.Load(planet.num, file, planet.chunks);
        }
        GUILayout.EndHorizontal();

        file = GUILayout.TextField(file);
    }

}
