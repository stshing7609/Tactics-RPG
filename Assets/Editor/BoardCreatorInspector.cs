using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

// add a custom editor for board creation
[CustomEditor(typeof(BoardCreator))]
[CanEditMultipleObjects]
public class BoardCreatorInspector : Editor {
    // options for generating new tiles
    string[] _tileTypeOptions = { "Random", "Sand", "Grass", "Water" };
    int _tileTypeIndex = 1;
    // options for changing existing tiles
    string[] _changeTileTypeOptions = { "Sand", "Grass", "Water" };
    int _changeSingleTileIndex = 0;
    
    // make sure out target is of the right type (BoardCreator)
    public BoardCreator current
    {
        get
        {
            return (BoardCreator)target;
        }
    }

    // override the inspector to our custom inspector
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Add a button for every public class in BoardCreator, which triggers the method
        if (GUILayout.Button("Clear"))
        {
            current.Clear();
            _tileTypeIndex = 1;
            _changeSingleTileIndex = 0;
        }

        // generate new tiles
        _tileTypeIndex = EditorGUILayout.Popup(_tileTypeIndex, _tileTypeOptions);
        if (GUILayout.Button("Set Tile Type"))
            current.SetTileType(_tileTypeOptions[_tileTypeIndex]);

        // change existing tiles
        _changeSingleTileIndex = EditorGUILayout.Popup(_changeSingleTileIndex, _changeTileTypeOptions);
        if (GUILayout.Button("Change Single Tile Type"))
            current.ChangeSingleTileType(_changeTileTypeOptions[_changeSingleTileIndex]);

        if (GUILayout.Button("Grow"))
            current.Grow();
        if (GUILayout.Button("Shrink"))
            current.Shrink();
        if (GUILayout.Button("Grow Area"))
            current.GrowArea();
        if (GUILayout.Button("Shrink Area"))
            current.ShrinkArea();
        if (GUILayout.Button("Add Obstacle"))
            current.AddObstacle();
        if (GUILayout.Button("Remove Obstacle"))
            current.RemoveObstacle();
        if (GUILayout.Button("Save"))
            current.Save();
        if (GUILayout.Button("Load"))
            current.Load();

        // when a value changes, update our Marker to the right place
        if (GUI.changed)
            current.UpdateMarker();
    }
}
