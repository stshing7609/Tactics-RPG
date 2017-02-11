using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEventsDemo : MonoBehaviour {

	void onEnable()
    {
        Debug.Log("oh hi");
        InputController.moveEvent += OnMoveEvent;
        InputController.fireEvent += OnFireEvent;
    }

    void onDisable()
    {
        InputController.moveEvent -= OnMoveEvent;
        InputController.fireEvent -= OnFireEvent;
    }

    void OnMoveEvent(object sender, InfoEventArgs<Point> e)
    {
        Debug.Log("Move " + e.info.ToString());
    }

    void OnFireEvent(object sender, InfoEventArgs<int> e)
    {
        Debug.Log("Fire " + e.info.ToString());
    }
}
