using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBattleState : BattleState {
    bool _initOnce = true;

    public override void Enter()
    {
        base.Enter();
        StartCoroutine(Init());
    }

    // This is a Coroutine so it happens all in one frame
    IEnumerator Init()
    {
        if (_initOnce)
        {
            _initOnce = false;
            board.Load(levelData);
            Point p = new Point((int)levelData.tiles[0].x, (int)levelData.tiles[0].z);
            SelectTile(p);
            yield return null;
            owner.ChangeState<MoveTargetState>();
        }
    }
}
