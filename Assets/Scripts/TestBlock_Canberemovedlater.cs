using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBlock_Canberemovedlater : Grabbable
{
    protected void Start()
    {
        base.Start();
        ColliderEnabledWhileGrabbed = true;
    }
}
