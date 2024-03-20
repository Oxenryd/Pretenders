using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Grabbable
{

    public override object[] GetTransferables()
    {
        return new Food[] { this };
    }

}
