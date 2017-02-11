using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputController : MonoBehaviour {
    #region Events
    public static event EventHandler<InfoEventArgs<int>> fireEvent;
    public static event EventHandler<InfoEventArgs<Point>> moveEvent;
    #endregion

    #region Properties
    const float _repeatThreshold = 0.5f; // how long to wait before checking repeats
    const float _repeatRate = 0.25f; // how often to update a repeat when actually repeating
    const float _tapRate = 0.1f; // not used atm
    float _horNext, _verNext; // target time before we start next repeat cycle
    bool _horHold, _verHold; // are we holding down a button
    string[] _buttons = new string[] { "Fire1", "Fire2", "Fire3" }; // handles fire button presses
    #endregion

    #region MonoBehaviour
    void Update()
    {
        // get input for vertical and horizontal, but since it's a tactics based game with snap moving,
        // we only need to know direction. GetAxisRaw returns -1, 0, 1 for left/down, no input, right/up
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // our default input is nothing
        int x = 0, y = 0;

        // are we moving horizontally
        if (!Mathf.Approximately(h, 0))
        {
            if (Time.time > _horNext)
            {
                // set horizontal input values
                x = (h < 0f) ? -1 : 1;
                // have we been holding the button or is this the first press
                _horNext = Time.time + (_horHold ? _repeatRate : _repeatThreshold);
                _horHold = true;
            }
        }
        // reset if no horizontal input
        else
        {
            _horHold = false;
            _horNext = 0;
        }

        // are we moving vertically
        if (!Mathf.Approximately(v, 0))
        {
            if (Time.time > _verNext)
            {
                // set vertical input values
                y = (v < 0f) ? -1 : 1;
                // have we been holding the button or is this the first press
                _verNext = Time.time + (_verHold ? _repeatRate : _repeatThreshold);
                _verHold = true;
            }
        }
        // reset if not vertical input
        else
        {
            _verHold = false;
            _verNext = 0;
        }

        // set Move
        if (x != 0 || y != 0)
            Move(new Point(x, y));

        for (int i = 0; i < 3; ++i)
        {
            if (Input.GetButtonUp(_buttons[i]))
                Fire(i);
        }
    }
    #endregion

    #region Private
    void Fire(int i)
    {
        if (fireEvent != null)
            fireEvent(this, new InfoEventArgs<int>(i));
    }

    void Move(Point p)
    {
        if (moveEvent != null)
            moveEvent(this, new InfoEventArgs<Point>(p));
    }
    #endregion
}