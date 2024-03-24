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
    private Vector3 _itemOffset; 

    void Start()
    {
        base.Start();
        _heldObjects = new List<Food>(GlobalValues.BASKET_MAX_SIZE);

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);
        BoxCollider _boxCollider = GetComponent<BoxCollider>();
        if (_boxCollider != null)
        {
            // Get the bounds of the object
            Bounds bounds = _boxCollider.bounds;

            // Extract length, width, and height from the bounds
            float Length = bounds.size.x/transform.localScale.x/2f;

            // Output the dimensions
            _itemOffset = new Vector3(Length, 0, 0);
            //Debug.LogError(Length);
        }
        else
        {
            Debug.LogError("Renderer component not found!");
        }
    }


    void Update()
    {
        base.Update();
        for(int i =0; i< _heldObjects.Count; i++)
        {
            _heldObjects[i].transform.position = transform.TransformPoint(_itemOffset);
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

    public override void KnockOff()
    {
        base.KnockOff();
        foreach(var item in _heldObjects)
        {
            item.Detach();
        }
        _heldObjects.Clear();
    }
}
