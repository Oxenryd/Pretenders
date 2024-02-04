using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAroundScript : MonoBehaviour
{
    [SerializeField] private PlayerInput _input;
    [SerializeField] private Rigidbody _body;

    private const string STRAFE = "Strafe";
    private const string WALK = "MoveForwardBackward";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 dir = new Vector3(_input.actions[STRAFE].ReadValue<float>(), 0, _input.actions[WALK].ReadValue<float>());
        _body.velocity = dir;
        
    }
}
