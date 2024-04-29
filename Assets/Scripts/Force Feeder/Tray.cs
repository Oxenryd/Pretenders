using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tray : Grabbable, IRecievable
{
    [SerializeField] private List<Food> _heldObjects; // List to hold 5 items

    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert
    { get { return _transferAlert; } }

    private Vector3[] _anchorPoints = new Vector3[]{ new Vector3(0.45f, 0f, 0.45f), new Vector3(-0.45f, 0f, -0.45f), new Vector3(-0.45f, 0f, 0.45f), new Vector3(0.45f, 0f, -0.45f), new Vector3(0f, 0f, 0f) };
    [SerializeField] private Vector3[] _rotationPoints = new Vector3[5];

    void Start()
    {
        base.Start();
        _heldObjects = new List<Food>(GlobalValues.TRAY_MAX_SIZE);

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);

        for(int i = 0; i< GlobalValues.TRAY_MAX_SIZE; i++)
        {
            _rotationPoints[i] = new Vector3(0f, Random.Range(30, 60), 0f);
        }
    }


    void Update()
    {
        base.Update();

        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > 1f)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = targetRotation;
        }
        

        for (int i =0; i< _heldObjects.Count; i++)
        {
            _heldObjects[i].transform.position = transform.position + this.transform.rotation * _anchorPoints[i];
            _heldObjects[i].transform.rotation =  this.transform.rotation * Quaternion.Euler(_rotationPoints[i]);
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
            if (_heldObjects.Count < GlobalValues.TRAY_MAX_SIZE)
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

    public override object[] GetTransferables() { return _heldObjects.ToArray(); }

    public override bool ProcessTransferResponse(int response)
    {
        if (response == 0)
        {
            _heldObjects.Clear();

        }
        return false;
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
