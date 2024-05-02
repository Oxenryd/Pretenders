using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Feet : MonoBehaviour
{
    [SerializeField] private Rigidbody _rBody;
    [SerializeField] private float _stiffness = 650f;
    [SerializeField] private float _restingLength = 1f;
    [SerializeField] private float _damping = 15f;
    [SerializeField] private Transform _feetTransform;
    [SerializeField] private HeroMovement _movement;
    private float _distanceToGround = 0f;
    private Vector3 _groundNormal;

    private HeroMovement _hero;
    private EasyTimer _keepDownThroughPlatformTimer;
    private bool _holdingDown = false;

    public Collider CurrentCollider
    { get; private set; }
    public bool IsGrounded { get; private set; }

    void Start()
    {
        _hero = _rBody.gameObject.GetComponent<HeroMovement>();
        _keepDownThroughPlatformTimer = new EasyTimer(0.3f);
    }
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(_feetTransform.position, Vector3.down, out hit, _restingLength,
            LayerUtil.Include(GlobalValues.GROUND_LAYER, GlobalValues.GROUNDABLE_LAYER, GlobalValues.PLATFORM_LAYER)))
        {
            if (hit.collider.gameObject.layer == GlobalValues.PLATFORM_LAYER)
            {
                if (_rBody.velocity.y < 0f && _hero.StickInputVector.y > -0.5f && !_holdingDown)
                {
                    doGrounded(hit);
                } else if (_hero.StickInputVector.y <= -0.5f)
                {
                    _keepDownThroughPlatformTimer.Reset();
                    _holdingDown = true;
                    notGrounded();
                }
            } else
            {
                doGrounded(hit);
            }
        } else
        {
            notGrounded();
        }
    }
    void Update()
    {
        if (_holdingDown && _keepDownThroughPlatformTimer.Done)
            _holdingDown = false;
    }
    private void doGrounded(RaycastHit hit)
    {
        _groundNormal = hit.normal;
        CurrentCollider = hit.collider;
        _distanceToGround = (_feetTransform.position - hit.point).magnitude;
        IsGrounded = true;
        _movement.SignalGrounded(_groundNormal);
        var dot = Mathf.Max(0, Vector3.Dot(Vector3.up, _groundNormal));
        var k = _distanceToGround - _restingLength;
        var F = -_stiffness * k - _damping * (_rBody.velocity.y * dot);
        var totalForce = F * dot * Vector3.up;
        _rBody.AddForce(totalForce);
    }

    private void notGrounded()
    {
        CurrentCollider = null;
        IsGrounded = false;
        _movement.SignalNotGrounded();
    }

    void OnDrawGizmos()
    {
        var start = _feetTransform.position;
        var end = _feetTransform.position + Vector3.down * _restingLength;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
    }
}