using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Unicorn : MonoBehaviour
{
    private float _speed = 2f;
    private Vector3 _direction;
    private bool _stoppedMoving;
    private enum UnicornState {standing, walking, eating };

    private UnicornState _state = UnicornState.standing;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (_state == UnicornState.standing)
        {
            _direction = new Vector3(UnityEngine.Random.Range(-1f,1f), 0f, UnityEngine.Random.Range(-1f, 1f));


            _state = UnicornState.walking;
        }
        else if (_state == UnicornState.walking)
        {
            MoveUnicorn();
            if (_direction != Vector3.zero)
            {
                transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, _direction);
            }
        }
    }

    void MoveUnicorn()
    {
        transform.position = transform.position + _direction.normalized * _speed * Time.deltaTime;

    }
}
