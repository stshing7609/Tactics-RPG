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
    public Transform marker
    {
        get
        {
            if (_marker == null)
            {
                GameObject instance = Instantiate(tileSelectionIndicatorPrefab) as GameObject;
                _marker = instance.transform;
            }
            return _marker;
        }
    }
    private Transform _marker;

    // max ranges of the board
    public int width = 10; // number of units in X direction in world space
    public int depth = 10; // number of units in Z direction in world space
    public int height = 8; // number of step units as defined by Tile script - affects things like jump heights on units
    public Point pos; // used to find specific point in board in case we wish to make modifications to the board
    public Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();  // dictionary of tiles and their locations
    public Dictionary<Point, Obstacle> obstacles = new Dictionary<Point, Obstacle>();

    public LevelData levelData; // load in any saved level data

    string _terrainType = "Sand"; // set the default tile type to Sand
    string[] _possibleTileTypes = { "Sand", "Grass", "Water" };

    // clear our board
    public void Clear()
    {
        // destroys every child of this gameObject aka every tile on the board
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        tiles.Clear(); // clear the dictionary
        _terrainType = "Sand"; // reset terrainType to Sand
    }

    // set what tile type to use for populating
    public void SetTileType(string type)
    {
        _terrainType = type;
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

    // add an obstacle
    public void AddObstacle()
    {
        Tile t = tiles.ContainsKey(pos) ? tiles[pos] : null; // make sure a tile exists first
        // check to see if there's already an obstacle
        if(!obstacles.ContainsKey(pos))
        {
            Obstacle o = CreateObstacle();
            o.Load(ref t, pos); // the obstacle is on the tile we checked
            t.AddObstacle(o); // make sure the tile knows it has an obstacle
            obstacles.Add(pos, o); // add the obstacle to the dictionary
        }
    }

    // remove an obstacle
    public void RemoveObstacle()
    {
        Obstacle o = obstacles.ContainsKey(pos) ? obstacles[pos] : null; // check if the obstacle exists
        if(o)
        {
            Tile t = tiles[pos]; // find the tile its on (this cannot be null since obstacles can only be made on tiles)
            t.RemoveObstacle(); // remove the reference to this obstacle
            obstacles.Remove(pos); // remove the obstacle from the dictionary
            DestroyImmediate(o.gameObject); // destroy the obstacle
        }
    }

    // lets a user save a level using the LevelData class since it's a ScriptableObject
    public void Save()
    {
        // try to access Resources/Levels in our App
        string filePath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(filePath))
            CreateSaveDirectory();

        // create the data for the board
        LevelData board = ScriptableObject.CreateInstance<LevelData>();
        // instantiate the lists of levelData
        board.tiles = new List<Vector3>(tiles.Count);
        board.terrainTypes = new List<string>(tiles.Count);
        board.obstacles = new List<Vector2>(obstacles.Count);
        // save tile parameters in those lists
        foreach (Tile t in tiles.Values)
        {
            board.tiles.Add(new Vector3(t.pos.x, t.height, t.pos.y));
            board.terrainTypes.Add(t.terrainTypeName);
        }
        // save obstacle position data
        foreach (Obstacle o in obstacles.Values)
        {
            board.obstacles.Add(new Vector2(o.pos.x, o.pos.y));
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
            return;

        // if this isn't true, something is wrong, so exit the function
        if (levelData.tiles.Count != levelData.terrainTypes.Count)
            return;
        
        // populate our dictionary with data from levelData
        for(int i = 0; i < levelData.tiles.Count; i++)
        {
            Vector3 v = levelData.tiles[i];
            string s = levelData.terrainTypes[i];
            Tile t = CreateTile();
            t.Load(v, s);
            tiles.Add(t.pos, t);
        }

        // populate obstacles
        foreach(Vector2 v in levelData.obstacles)
        {
            Obstacle o = CreateObstacle();
            Point tempP;
            tempP.x = (int)v.x;
            tempP.y = (int)v.y;
            Tile tempT = tiles[tempP];
            o.Load(ref tempT, tempP); // due to Match() being called, this should realign positioning
            obstacles.Add(o.pos, o);
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

    Obstacle CreateObstacle()
    {
        GameObject instance = Instantiate(obstacleViewPrefab) as GameObject;
        instance.transform.parent = transform;
        return instance.GetComponent<Obstacle>();
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
            DestroyImmediate(t.gameObject);
            // if the tile had an obstacle, properly destroy the obstacle too
            if(t.obs != null)
            {
                Obstacle tempO = obstacles[p];
                obstacles.Remove(p);
                DestroyImmediate(tempO.gameObject);
            }
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
