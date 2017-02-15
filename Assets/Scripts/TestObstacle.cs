using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObstacle : PlaceableObject {
    public override void Match()
    {
        transform.localPosition = new Vector3(tile.pos.x, tile.center.y + transform.localScale.y, tile.pos.y);
    }
}
