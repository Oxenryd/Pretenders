using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrayNecessary : Grabbable, IRecievable
{
    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert
    { get { return _transferAlert; } }

    public int Transfer(object[] recievedObject)
    {
        // implentation i den här såklart!
        return 0;
    }
    protected void Start()
    {
        base.Start();
        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);
    }
}

