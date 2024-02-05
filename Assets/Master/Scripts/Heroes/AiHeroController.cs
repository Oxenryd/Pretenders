using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

//Sample AI script for controlling the heroes

public class AiHeroController : MonoBehaviour
{
    [SerializeField] private HeroMovement _hero;

    //Debug and testing random movement for Ai heroes for now
    private EasyTimer _directionTimer;
    private EasyTimer _jumpTimer;
    private Vector2 _targetDirection = Vector2.zero;
    

    // Start is called before the first frame update
    void Start()
    {
        //Subscribe to GameManager EarlyUpdate
        _directionTimer = new EasyTimer(2f);
        _jumpTimer = new EasyTimer(Random.Range(0.8f, 10f));
        GameManager.Instance.EarlyUpdate += _directionTimer.TickSubscription;
        GameManager.Instance.EarlyUpdate += _jumpTimer.TickSubscription;

    }

    // Update is called once per frame
    void Update()
    {
        //Just stop if this Hero is player controlled
        if (!_hero.AiControlled)
            return;

        //If Timer is done, get a random direction and walk in that direction
        //Do sporadic happy jumps
        if (_directionTimer.Done)
        {
            _targetDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            _hero.TryMoveAi(_targetDirection);
            _directionTimer.Time = Random.Range(0.3f, 3f);
            _directionTimer.Reset();
        }
        if (_jumpTimer.Done)
        {
            _hero.TryJumpAi();
            _jumpTimer.Time = Random.Range(0.8f, 6f);
            _jumpTimer.Reset();
        }
    }  
}
