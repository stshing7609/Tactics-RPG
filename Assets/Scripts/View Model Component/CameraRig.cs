using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {
    public float speed = 3f; // how fast does the camera move
    public Transform follow; // what are we following
    Transform _transform; // store the camera's transform

    public void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        // if we have a transform we're following, move from current pos to the follow pos at speed
        if (follow)
            _transform.position = Vector3.Lerp(_transform.position, follow.position, speed * Time.deltaTime);
    }
}
