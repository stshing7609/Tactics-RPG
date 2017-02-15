using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleController : StateMachine {
    public CameraRig cameraRig;
    public Board board;
    public LevelData levelData;
    public Transform tileSelectionIndicator;
    public Point pos;

    void Start()
    {
        board = GetComponentInChildren<Board>();
        levelData = Resources.Load<LevelData>(string.Format("Levels/Level_{0}", Board.level));
        ChangeState<MoveTargetState>();
    }
}
