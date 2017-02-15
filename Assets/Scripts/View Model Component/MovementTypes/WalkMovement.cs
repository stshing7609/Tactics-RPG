using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkMovement : Movement {
    // check for obstacles
    protected override bool ExpandSearch(Tile tile)
    {
        // check for jump heights
        if (Mathf.Abs(tile.prev.height - tile.height) > jumgHeight)
            return false;

        // cannot move through other objects
        if(tile.content != null)
            return false;

        return base.ExpandSearch(tile);
    }

    public override IEnumerator Traverse (Tile tile)
    {
        yield return StartCoroutine(base.Traverse(tile));

        // Build a list of way points from the unit's starting tile to the destination tile
        List<Tile> targets = new List<Tile>();
        while(tile != null)
        {
            targets.Insert(0, tile);
            tile = tile.prev;
        }

        // Move to each way point in succession
        for(int i = 1; i < targets.Count; ++i)
        {
            Tile from = targets[i - 1];
            Tile to = targets[i];

            // get the direction we need to go
            Directions dir = from.GetDirection(to);
            // if it's not the same direciton, turn
            if (unit.dir != dir)
                yield return StartCoroutine(Turn(dir));

            if (from.height == to.height)
                yield return StartCoroutine(Walk(to));
            else
                yield return StartCoroutine(Jump(to));
        }

        yield return null;
    }

    // animate walk to target tile
    IEnumerator Walk(Tile target)
    {
        Tweener tweener = transform.MoveTo(target.center, 0.5f, EasingEquations.Linear);
        while (tweener != null)
            yield return null;
    }

    // animate jump to target tile
    IEnumerator Jump(Tile target)
    {
        // we still move forward
        Tweener tweener = transform.MoveTo(target.center, 0.5f, EasingEquations.Linear);

        // jump to match the target tile's height
        // multiply stepHeight by 2 to shop a hop
        Tweener t2 = jumper.MoveToLocal(new Vector3(0, Tile.stepHeight * 2f, 0), tweener.easingControl.duration / 2f, EasingEquations.EaseOutQuad);
        t2.easingControl.loopCount = 1;
        t2.easingControl.loopType = EasingControl.LoopType.PingPong;

        while (tweener != null)
            yield return null;
    }
}
