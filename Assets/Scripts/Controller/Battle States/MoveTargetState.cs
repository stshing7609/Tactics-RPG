using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTargetState : BattleState {
    protected override void Awake()
    {
        base.Awake();
        Point p = new Point((int)levelData.tiles[0].x, (int)levelData.tiles[0].z);
        SelectTile(p);
    }

    protected override void OnMove(object sender, InfoEventArgs<Point> e)
    {
        SelectTile(e.info + pos);
    }
}
