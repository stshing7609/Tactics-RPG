using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    [SerializeField] GameObject tilePrefab;
    public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
    public Dictionary<Point, PlaceableObject> content = new Dictionary<Point, PlaceableObject>();

    // load the level
    public void Load(LevelData data)
    {
        // if something is wrong with the data, abort
        if (data.tiles.Count != data.terrainTypes.Count || data.content.Count != data.contentPaths.Count)
            return;

        // load up all the tiles
        for(int i = 0; i < data.tiles.Count; i++)
        {
            GameObject instance = Instantiate(tilePrefab) as GameObject;
            Tile t = instance.GetComponent<Tile>();
            t.Load(data.tiles[i], data.terrainTypes[i]);
            if(!tiles.ContainsKey(t.pos))
                tiles.Add(t.pos, t);
        }

        // load up all of the placeable objects
        for (int i = 0; i < data.content.Count; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("PlaceableObjects/" + data.contentPaths[i]);
            GameObject instance = GameObject.Instantiate(prefab);

            Vector2 v = data.content[i];
            Point tempPos = new Point((int)v.x, (int)v.y);
            PlaceableObject po = instance.GetComponent<PlaceableObject>();
            po.Place(tiles[tempPos]);
            po.Match();
            if (!content.ContainsKey(tempPos))
            {
                content.Add(po.tile.pos, po);
            }
        }
    }
}
