using UnityEngine;
using System.Collections;
using UnityEditor;

// add a custom editor for board creation
[CustomEditor(typeof(BoardCreator))]
public class BoardCreatorInspector : Editor {
    string[] _tileTypeOptions = { "Random", "Sand", "Grass", "Water" };
    int _tileTypeIndex = 1;
    
    // make sure out target is of the righ type (BoardCreator)
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
        }

        _tileTypeIndex = EditorGUILayout.Popup(_tileTypeIndex, _tileTypeOptions);
        if (GUILayout.Button("Set Tile Type"))
            current.SetTileType(_tileTypeOptions[_tileTypeIndex]);

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
