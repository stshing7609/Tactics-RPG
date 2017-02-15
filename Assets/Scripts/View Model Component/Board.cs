using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Board : MonoBehaviour {
    #region Properties
    public static int level = 2;

    [SerializeField] GameObject tilePrefab;
    Color selectedTileColor = new Color(0, 1, 1, 1);
    Color defaultTileColor = new Color(1, 1, 1, 1);

    public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
    public Dictionary<Point, PlaceableObject> content = new Dictionary<Point, PlaceableObject>();
    Point[] dirs = new Point[4]
    {
        new Point(0,1),
        new Point(0,-1),
        new Point(1,0),
        new Point(-1,0)
    };
    #endregion

    #region MonoBehaviour
    void Awake()
    {
        Debug.Log("fired");
        CreateBoard();
    }
    #endregion

    #region Public
    public Tile GetTile(Point p)
    {
        return tiles.ContainsKey(p) ? tiles[p] : null;
    }

    public HashSet<Tile> Search(Tile start, Func<Tile, bool> addTile)
    {
        ClearSearch();
        start.distance = 0;

        HashSet<Tile> retVal = new HashSet<Tile>();
        retVal.Add(start);

        Queue<Tile> checkNext = new Queue<Tile>();
        Queue<Tile> checkNow = new Queue<Tile>();
        checkNow.Enqueue(start);

        while(checkNow.Count > 0)
        {
            Tile t = checkNow.Dequeue();
            for(int i = 0; i < 4; ++i)
            {
                // attempt to get the tile one position over in the direction we're looking for
                Tile next = GetTile(t.pos + dirs[i]);

                // check if the tile doesn't exist, move on
                if (next == null)
                    continue;

                // If the tile does exist in retVal, but also its distance value is int.MaxValue, move on
                if (retVal.Contains(next) && t.distance + 1 >= next.distance)
                    continue;

                // set up the distance and prev of next
                next.distance = t.distance + 1;
                next.prev = t;

                // if the what we have is indeed a Tile, add it to the queue of things to check next
                // but also add it to our return value
                if (addTile(next))
                {
                    checkNext.Enqueue(next);
                    retVal.Add(next);
                }
            }

            if (checkNow.Count == 0)
                SwapReference(ref checkNow, ref checkNext);
        }

        return retVal;
    }

    public void SelectTiles(HashSet<Tile> tiles)
    {
        foreach (Tile t in tiles)
            t.GetComponent<Renderer>().material.color = selectedTileColor;
    }

    public void DeSelectTiles(HashSet<Tile> tiles)
    {
        foreach (Tile t in tiles)
            t.GetComponent<Renderer>().material.color = defaultTileColor;
    }
    #endregion

    #region Private
    // load the level
    void CreateBoard()
    {
        LevelData data = Resources.Load<LevelData>(string.Format("Levels/Level_{0}", level));

        int tileCount = data.tiles.Count;
        int contentCount = data.content.Count;

        // if something is wrong with the data, abort
        if (tileCount != data.terrainTypes.Count || contentCount != data.contentPaths.Count)
            return;

        // load up all the tiles
        for (int i = 0; i < tileCount; i++)
        {
            GameObject instance = Instantiate(tilePrefab) as GameObject;
            Tile t = instance.GetComponent<Tile>();
            t.Load(data.tiles[i], data.terrainTypes[i]);
            if (!tiles.ContainsKey(t.pos))
                tiles.Add(t.pos, t);
        }

        // load up all of the placeable objects
        for (int i = 0; i < contentCount; i++)
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

    void ClearSearch()
    {
        foreach(Tile t in tiles.Values)
        {
            t.prev = null;
            t.distance = int.MaxValue;
        }
    }

    void SwapReference(ref Queue<Tile> a, ref Queue<Tile> b)
    {
        Queue<Tile> temp = a;
        a = b;
        b = temp;
    }
    #endregion
}
