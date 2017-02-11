using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    public Point pos; // obstacle position
    Tile _groundBelow; // tile the obstacle is on

    // any time the height or position of the tile below it is changed, visually reflect its new values
    public void Match()
    {
        transform.localPosition = new Vector3(pos.x, _groundBelow.center.y + transform.localScale.y, pos.y);
    }

    // pass the Tile by reference so that whenever the tile moves, we can just call Obstacle's Match function
    public void Load(ref Tile t, Point p)
    {
        _groundBelow = t;
        pos = p;
        Match();
    }
}
