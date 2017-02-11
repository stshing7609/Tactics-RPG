using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Point {
    public int x;   // x value in space
    public int y;   // z value in space

    // constructor
    public Point (int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // overload operators to let us add, subtract, and compare Points
    public static Point operator +(Point a, Point b)
    {
        return new Point(a.x + b.x, a.y + b.y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Point a, Point b)
    {
        return !(a == b);
    }

    // override Equals and GetHashCode because we overloaded the equality operator
    public override bool Equals(object obj)
    {
        if(obj is Point)
        {
            Point p = (Point)obj;
            return x == p.x && y == p.y;
        }
        return false;
    }

    public bool Equals (Point p)
    {
        return x == p.x && y == p.y;
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    // toString to give the Point data as a Point
    public override string ToString()
    {
        return string.Format("({0},{1})", x, y);
    }
}
