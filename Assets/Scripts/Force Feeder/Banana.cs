using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : Food
{
    void Start()
    {
        base.Start();
        _points = 3;
    }
}
