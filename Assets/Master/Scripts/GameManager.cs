using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    //Unity specific Singleton-shenanigans----------------------------------------------------------
    private static GameManager _instance;
    /// <summary>
    /// The Singleton instance of our GameManager
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(GameManager).Name);
                    _instance = singletonObject.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }
    //-----------------------------------------------------------------------------------------------

    //Events
    public EventHandler<float> EarlyUpdate;
    public EventHandler<float> EarlyFixedUpdate;
    protected void OnEarlyUpdate(float deltaTime)
    {
        if (EarlyUpdate == null) return;
        EarlyUpdate.Invoke(this, deltaTime);
    }
    protected void OnEarlyFixedUpdate(float fixedDeltaTime)
    {
        if (EarlyFixedUpdate == null) return;
        EarlyFixedUpdate.Invoke(this, fixedDeltaTime);
    }

    public float DeltaTime { get; private set; }
    public float FixedDeltaTime { get; private set; }
    public int GroundLayer { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        //Cache layer so not to compare string literals during updates.
        GroundLayer = LayerMask.NameToLayer("Ground");
    }

    //The GameManager Update is being executed before all other MonoBehaviors Update().
    //Project settings -> Script Execution Order
    void Update()
    {
        //Cache external call to deltaTime.
        //Small benefit to use the gameManager's cached version instead of calling
        //the external Time.deltaTime in every objects' Update().
        DeltaTime = Time.deltaTime;

        //Invoke Early Update for subscribers.
        OnEarlyUpdate(DeltaTime);
    }
    private void FixedUpdate()
    {
        FixedDeltaTime = Time.fixedDeltaTime;
        OnEarlyFixedUpdate(FixedDeltaTime);
    }
}
