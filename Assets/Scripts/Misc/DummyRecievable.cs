using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRecievable : MonoBehaviour, IRecievable
{
    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert
    { get { return _transferAlert; } }

    public int Transfer(object[] recievedObject)
    {
        var gObjects = recievedObject as GameObject[];
        if (gObjects == null)
        {
            var food = recievedObject as Food[];
            if (food == null)
                throw new System.Exception("COULD CAST THIS ARRAY");
            else
                foreach (var foodThing in food)
                {
                    Destroy(foodThing);
                    Debug.Log("JOINK!!!");
                }
        } else
            foreach (var gobject in gObjects)
            {
                Destroy(gobject);
                Debug.Log("JOINK!!!");
            }
        return 0;
    }

    void Start()
    {
        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);
    }
}

