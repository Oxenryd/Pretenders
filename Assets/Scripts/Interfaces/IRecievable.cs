using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRecievable
{
    public Transform transform { get; }
    public TransferAlert TransferAlert { get; }
    public int Transfer(object[] recievedObject);
}