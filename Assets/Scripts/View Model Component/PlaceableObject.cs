using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableObject : MonoBehaviour {
    public Tile tile { get; set; } // every placeable object has a tile it's on
    public string name { get; set; } // name of the placeable object

    public void Place(Tile target)
    {
        // make sure the old location is no longer pointing to the object
        if (tile != null && tile.content == gameObject)
            tile.content = null;

        tile = target;

        // if the target tile isn't null, put the object on it
        if(target != null)
        {
            target.content = gameObject;
        }
    }

    // correctly position the object relative to the tile
    // can be overriden, but will almost always have the object be directly over the center of the tile
    // REMEMBER TO CALL BASE
    public abstract void Match();
}
