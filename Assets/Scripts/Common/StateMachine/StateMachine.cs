using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores current state, handles state transitions
public class StateMachine : State {

    protected State _currentState;
    protected bool _inTransition;

	public virtual State CurrentState
    {
        get { return _currentState; }
        // when we set a state, transist to it
        set { Transition(value); }
    }

    // get any generic state.
    public virtual T GetState<T>() where T : State
    {
        T target = GetComponent<T>();
        // if there is no state, add it
        if (target == null)
            target = gameObject.AddComponent<T>();
        return target;
    }

    // when you're changing a state, call GetState to try to get it, or make it if it doesn't exist
    public virtual void ChangeState<T>() where T : State
    {
        CurrentState = GetState<T>();
    }

    public virtual void Transition(State value)
    {
        // if we're trying to go to the same state, or we're in transition, stop Transiting
        if (_currentState == value || _inTransition)
            return;

        // we're in Transition
        _inTransition = true;

        // if the state exists, remove listeners
        if(_currentState != null)
            _currentState.Exit();

        // set our state to the new one
        _currentState = value;

        // if the new state exists, add listeners
        if (_currentState != null)
            _currentState.Enter();

        // we are done transisting
        _inTransition = false;
    }
}
