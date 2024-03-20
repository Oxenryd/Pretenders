using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tray : Grabbable, IRecievable
{
    [SerializeField] private List<Food> _heldObjects; // List to hold 5 items

    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert
    { get { return _transferAlert; } }


    void Start()
    {
        base.Start();
        _heldObjects = new List<Food>(GlobalValues.BASKET_MAX_SIZE);

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);
    }


    void Update()
    {
        base.Update();
        for (int i = 0; i < _heldObjects.Count; i++)
        {
            _heldObjects[i].transform.position = this.transform.position;
            _heldObjects[i].transform.rotation = this.transform.rotation;
        }

    }

    public int Transfer(object[] recievedObject)
    {
        var foodArray = (recievedObject as Food[]);

        if (foodArray == null)
        {
            return recievedObject.Length;
        }

        // Find an empty slot on the tray
        for (int i = 0; i < recievedObject.Length; i++)
        {
            if (_heldObjects.Count < GlobalValues.BASKET_MAX_SIZE)
            {
                foodArray[i].Attach(this);
                _heldObjects.Add(foodArray[i]);
            }
            else
            {
                return recievedObject.Length - i;
            }
        }
        return 0;
    }


}