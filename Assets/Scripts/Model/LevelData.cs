using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Is a scriptableobject because it is an asset that only stores data
// kinda like unity's version of json and XML, but since it's built in, it's faster
// don't need to load an external file. just access the asset created by this script
public class LevelData : ScriptableObject {
    // holds the positions and heights of every tile
    // remember, its (x, height, y)
    public List<Vector3> tiles;
    public List<string> terrainTypes;
    public List<Vector2> content;
    public List<string> contentPaths;
}
