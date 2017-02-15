using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : PlaceableObject {
    public Directions dir;

    public override void Match()
    {
        transform.localPosition = tile.center;
        transform.localEulerAngles = dir.ToEuler(); // set facing
    }
}
