using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Grabbable
{
    [SerializeField] protected int _points;

    public int GetPoints()
    {
        return _points;
    }

    public override object[] GetTransferables()
    {
        return new Food[] { this };
    }



}
