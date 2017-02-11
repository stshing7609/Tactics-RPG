using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Handle any type of EventArg using Generics
public class InfoEventArgs<T> : EventArgs
{
    public T info;

    // default constructor
    public InfoEventArgs()
    {
        info = default(T);
    }
    
    // constructor with specified data
    public InfoEventArgs(T info)
    {
        this.info = info;
    }
}
