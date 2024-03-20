using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tray : Grabbable, IRecievable
{
    [SerializeField] private List<Food> _heldObjects; // List to hold 5 items

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        _heldObjects = new List<Food>(GlobalValues.BASKET_MAX_SIZE);        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        for(int i =0; i< _heldObjects.Count; i++)
        {
            _heldObjects[i].transform.position = this.transform.position;
        }
    }

    public int Transfer(object[] recievedObject)
    {
        // Find an empty slot in the basket

        var foodArray = recievedObject as Food[];

        if (foodArray == null)
        {
            return recievedObject.Length;
        }

        for (int i = 0; i < recievedObject.Length; i++)
        {
            if (_heldObjects.Count < GlobalValues.BASKET_MAX_SIZE)
            {
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
