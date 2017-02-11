using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
    public const float stepHeight = 0.25f; // height of a full tile is equal to four steps

    public Point pos; // tile position
    public int height; // tile's height
    // gives the center of the tile (so that we may place objects at the center of the top of the tile)
    public Vector3 center { get { return new Vector3(pos.x, height * stepHeight, pos.y); } }

    public string terrainTypeName = "Sand";
    public Obstacle obs;
    // TO BE USED LATER FOR ANY TILES THAT WILL HAVE EFFECTS
    // EFFECT TILES WILL INHERIT FROM TILE.CS
    public bool hasEffect = false;
    // TO BE USED LATER FOR CHECKING MOVEMENT
    public bool passable = true;

    // any time a tile's height or position is changed, visually reflect its new values
    void Match ()
    {
        transform.localPosition = new Vector3(pos.x, height * stepHeight / 2f, pos.y);
        transform.localScale = new Vector3(1, height * stepHeight, 1);
    }

    // the board will be created by randomly growing or shrinking tiles
    public void Grow()
    {
        height++;
        Match();
    }

    public void Shrink()
    {
        height--;
        Match();
    }

    public void AddObstacle(Obstacle o)
    {
        obs = o;
    }

    public void RemoveObstacle()
    {
        if (obs)
            obs = null;
    }

    //overload Load so that we can persist Tile data as Vector3
    public void Load(Point p, int h, string mat)
    {
        pos = p;
        height = h;
        Match();

        terrainTypeName = mat;
        Material newMat = Resources.Load("Materials/" + terrainTypeName, typeof(Material)) as Material;
        gameObject.GetComponent<MeshRenderer>().material = newMat;
    }

    public void Load(Vector3 v, string mat)
    {
        Load(new Point((int)v.x, (int)v.z), (int)v.y, mat);
    }
}
