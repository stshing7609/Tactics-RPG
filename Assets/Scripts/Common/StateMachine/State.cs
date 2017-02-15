using System.Collections;
using UnityEngine;

public abstract class State : MonoBehaviour {
    // when you enter a state, add all listeners
    public virtual void Enter()
    {
        AddListeners();
    }

    // when you leave a state, properly remove all listeners
    public virtual void Exit()
    {
        RemoveListeners();
    }

    protected virtual void AddListeners()
    {

    }

    protected virtual void RemoveListeners()
    {

    }
}
