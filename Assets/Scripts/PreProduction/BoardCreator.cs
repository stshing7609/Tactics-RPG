using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

// This class is in preproduction because it's used to make the game, but not
// actually used in the game itself
// This class is a visual editor that let's a user create a board
public class BoardCreator : MonoBehaviour {
    // Reference the tile and tile selection indicator prefabs so we can manipulate them
    [SerializeField] GameObject tileViewPrefab;
    [SerializeField] GameObject obstacleViewPrefab;
    [SerializeField] GameObject tileSelectionIndicatorPrefab;

    // create the Selection Indicator whenever we need to use it through Lazy Loading
    // it's a transform to track position in world space
    private Transform _marker = null;
    public Transform marker
    {
        get
        {
            // if we have a tile selection indicator, use that
            if (GameObject.Find(tileSelectionIndicatorPrefab.name) != null)
            {
                GameObject instance = GameObject.Find(tileSelectionIndicatorPrefab.name);
                _marker = instance.transform;
            }
            // else follow the normal patter of lazy loading
            else if (_marker == null)
            {
                GameObject instance = Instantiate(tileSelectionIndicatorPrefab) as GameObject;
                _marker = instance.transform;
            }
            return _marker;
        }
    }

    // max ranges of the board
    public int width = 10; // number of units in X direction in world space
    public int depth = 10; // number of units in Z direction in world space
    public int height = 8; // number of step units as defined by Tile script - affects things like jump heights on units
    public Point pos; // used to find specific point in board in case we wish to make modifications to the board
    public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();  // dictionary of tiles and their locations
    public Dictionary<Point, PlaceableObject> contentDict = new Dictionary<Point, PlaceableObject>(); // dictionary of placeable objects and their locations
    public Dictionary<Point, string> contentPaths = new Dictionary<Point, string>(); // dictionary of placeobject locations and their types

    public LevelData levelData; // load in any saved level data

    string _terrainType = "Sand"; // set the default tile type to Sand
    string[] _possibleTileTypes = { "Sand", "Grass", "Water" };

    // clear our board
    public void Clear()
    {
        // destroys every child of this gameObject aka every tile on the board
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        tiles.Clear(); // clear the tile dictionary
        contentDict.Clear(); // clear the placeobject dictionary
        contentPaths.Clear(); // clear the paths to placeable objects dictionary
        _terrainType = "Sand"; // reset terrainType to Sand
    }

    // set what tile type to use for populating
    public void SetTileType(string type)
    {
        _terrainType = type;
    }

    // change the terrain type of the selected tile
    public void ChangeSingleTileType(string type)
    {
        Tile t = tiles.ContainsKey(pos) ? tiles[pos] : null;
        t.Load(t.pos, t.height, type);
    }

    // modify individual tiles at a time
    public void Grow()
    {
        GrowSingle(pos);
    }

    public void Shrink()
    {
        ShrinkSingle(pos);
    }

    // generate a random board based off either growing tiles or shrinking them
    public void GrowArea()
    {
        Rect r = RandomRect();
        GrowRect(r);
    }

    public void ShrinkArea()
    {
        Rect r = RandomRect();
        ShrinkRect(r);
    }

    // update the position of the Tile Selection Indicator so we can see what tile we might be modifying
    public void UpdateMarker()
    {
        // does the tile exist
        Tile t = tiles.ContainsKey(pos) ? tiles[pos] : null;
        // if we find a tile, put the cursor on it. if not, put it where we wanted to try to find a tile
        marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0, pos.y);
    }

    // add content on a tile
    public void AddContent(string content)
    {
        if (!tiles.ContainsKey(pos))  // make sure a tile exists first
            return;
        if (contentDict.ContainsKey(pos)) // check if there's something on the tile already
            return;

        Tile t = tiles[pos]; // get the tile

        // make the placeable object
        PlaceableObject po = CreateContent(content);

        // place the placeable object
        if (po != null)
            po.Place(t);

        // reposition everything
        t.Match();

        // add to dictionaries
        contentDict.Add(pos, po);
        contentPaths.Add(pos, content);
    }

    // remove content from a tile
    public void RemoveContent()
    {
        // make sure a tile exists first
        if (!tiles.ContainsKey(pos))
            return;
        // then make sure the content exists
        if (!contentDict.ContainsKey(pos))
            return;

        Tile t = tiles[pos];
        // remove everything and destroy the content
        contentDict.Remove(pos);
        contentPaths.Remove(pos);
        DestroyImmediate(t.content.gameObject);
        t.content = null;
    }

    // lets a user save a level using the LevelData class since it's a ScriptableObject
    public void Save(string name)
    {
        if(name.Equals(""))
        {
            Debug.Log("Invalid file name");
            return;
        }
        
        // try to access Resources/Levels in our App
        string filePath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(filePath))
            CreateSaveDirectory();

        // create the data for the board
        LevelData board = ScriptableObject.CreateInstance<LevelData>();
        // instantiate the lists of levelData
        board.tiles = new List<Vector3>(tiles.Count);
        board.terrainTypes = new List<string>(tiles.Count);
        board.content = new List<Vector2>(contentDict.Count);
        board.contentPaths = new List<string>(contentPaths.Count);
        // save tile parameters in those lists
        foreach (Tile t in tiles.Values)
        {
            board.tiles.Add(new Vector3(t.pos.x, t.height, t.pos.y));
            board.terrainTypes.Add(t.terrainTypeName);
        }

        // save placeable object positions
        foreach(PlaceableObject po in contentDict.Values)
        {
            board.content.Add(new Vector2(po.tile.pos.x, po.tile.pos.y));
        }

        // save placeable object types
        foreach(string s in contentPaths.Values)
        {
            board.contentPaths.Add(s);
        }
      
        // name and create the asset
        string fileName = string.Format("Assets/Resources/Levels/{1}.asset", filePath, name);
        AssetDatabase.CreateAsset(board, fileName);
    }

    // load a level from levelData
    public void Load()
    {
        Clear();
        if (levelData == null)
        {
            Debug.Log("No level selected");
            return;
        }

        // if this isn't true, something is wrong, so exit the function
        if (levelData.tiles.Count != levelData.terrainTypes.Count || levelData.content.Count != levelData.contentPaths.Count)
            return;

        // populate our tile dictionary with data from levelData
        for (int i = 0; i < levelData.tiles.Count; i++)
        {
            Vector3 v = levelData.tiles[i];
            string s = levelData.terrainTypes[i];
            Tile t = CreateTile();
            t.Load(v, s);
            tiles.Add(t.pos, t);
        }

        // populate out placeable object dictionaries with data from levelData
        for (int i = 0; i < levelData.content.Count; i++)
        {
            PlaceableObject po = CreateContent(levelData.contentPaths[i]);
            Vector2 v = levelData.content[i];
            Point tempPos = new Point((int)v.x,(int)v.y);
            Tile t = tiles[tempPos];
            po.Place(t);
            po.Match();
            contentDict.Add(po.tile.pos, po);
            contentPaths.Add(po.tile.pos, levelData.contentPaths[i]);
        }
    }

    // create a rect somewhere in the region specified by our max values width, depth, and height
    Rect RandomRect()
    {
        int x = UnityEngine.Random.Range(0, width);
        int y = UnityEngine.Random.Range(0, depth);
        int w = UnityEngine.Random.Range(1, width - x + 1);
        int h = UnityEngine.Random.Range(1, depth - y + 1);
        return new Rect(x, y, w, h);
    }

    // loop through all positions made by our random rect, and grow or shrink 1 tile at a time
    void GrowRect(Rect rect)
    {
        for(int y = (int)rect.yMin; y < (int)rect.yMax; ++y)
        {
            for (int x = (int)rect.xMin; x < (int)rect.xMax; ++x)
            {
                Point p = new Point(x, y);
                GrowSingle(p);
            }
        }
    }

    void ShrinkRect(Rect rect)
    {
        for (int y = (int)rect.yMin; y < (int)rect.yMax; ++y)
        {
            for (int x = (int)rect.xMin; x < (int)rect.xMax; ++x)
            {
                Point p = new Point(x, y);
                ShrinkSingle(p);
            }
        }
    }

    // to grow or shrink a single tile, we must first reference it from the Dictionary
    // if the tile doesn't exist, create it
    // Create a tile
    Tile CreateTile()
    {
        GameObject instance = Instantiate(tileViewPrefab) as GameObject;
        instance.transform.parent = transform;
        return instance.GetComponent<Tile>();
    }

    // create placeable object from the name of object we want
    PlaceableObject CreateContent(string name)
    {
        // look for the prefab from Resources/PlaceableObjects
        GameObject prefab = Resources.Load<GameObject>("PlaceableObjects/" + name);
        GameObject instance = GameObject.Instantiate(prefab);
        instance.transform.parent = transform;
        return instance.GetComponent<PlaceableObject>();
    }

    // check if the Tile is in the dictionary and if not create it. then return
    // whatever Tile we find or make
    Tile GetOrCreate(Point p)
    {
        if (tiles.ContainsKey(p))
            return tiles[p];

        string tempTerrainType = _terrainType;

        if (_terrainType.Equals("Random"))
        {
            int rnd = Random.Range(0, _possibleTileTypes.Length);
            tempTerrainType = _possibleTileTypes[rnd];
        }

        Tile t = CreateTile();
        t.Load(p, 0, tempTerrainType);
        tiles.Add(p, t);

        return t;
    }

    // grow a single tile using the Tile's growth method
    void GrowSingle(Point p)
    {
        Tile t = GetOrCreate(p);
        // make sure we don't go above our max elevation
        if (t.height < height)
        {
            t.Grow();
            // if there's an obstacle, correctly reposition it
            if(t.obs != null)
                t.obs.Match();
        }
    }

    // shrink a single tile after finding out if it exists
    // don't create a tile if there isn't one, because we will destroy any tile that
    // drops below 0 height
    void ShrinkSingle(Point p)
    {
        if (!tiles.ContainsKey(p))
            return;

        Tile t = tiles[p];
        t.Shrink();
        if(t.obs != null)
            t.obs.Match();

        // destroy the tile if is equal or below 0 height (this is how we make gaps in the board)
        if (t.height <= 0)
        {
            tiles.Remove(p);
            // destroy any content on it too
            if (contentDict.ContainsKey(p) && contentPaths.ContainsKey(p))
            {
                contentDict.Remove(p);
                contentPaths.Remove(p);
                DestroyImmediate(t.content.gameObject);
                t.content = null;
            }
            DestroyImmediate(t.gameObject);
        }
    }

    // makes a save directory if we do not have one
    void CreateSaveDirectory()
    {
        string filePath = Application.dataPath + "/Resources";
        // if Resources doesn't exist, make the folder
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets", "Resources");
        filePath += "/Levels";
        // if Levels doesnt exist in Resources, make the folder
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets/Resources", "Levels");
        // update our changes
        AssetDatabase.Refresh();
    }
}
