using UnityEngine;

public class HeroDust : MonoBehaviour
{
    [SerializeField] private HeroMovement _hero;
    [SerializeField] private Transform _dustsChild;
    [SerializeField] private ParticleSystem _particleJump;
    [SerializeField] private ParticleSystem _particleLand;
    [SerializeField] private ParticleSystem _particleDJump;
    [SerializeField] private ParticleSystem _particleHurt;
    [SerializeField] private ParticleSystem _particleFootDownRun;
    [SerializeField] private Vector3 _leftFootPosOffset;
    [SerializeField] private Vector3 _rightFootPosOffset;
    [SerializeField] private Vector3 _slidePosOffset;
    [SerializeField] private float _landSizeFactor = 100f;
    [SerializeField] private float _runSizeFactor = 1f;
    [SerializeField] private float _runSpeedFactor = 1f;
    [SerializeField] private float _pushingSpeedFactor = 1f;
    [SerializeField] private float _pushingSizeFactor = 1f;

    private bool _didJump = false;
    private bool _didDJump = false;
    private bool _wasFalling = false;

    private EasyTimer _pushTimer;

    private float _lastFallSpeed = 0f;
    private float _lastGroundSpeedSqr = 0f;

    public void PlayRightFootStep()
    {
        var main = _particleFootDownRun.main;
        main.startSize = _lastGroundSpeedSqr * _runSizeFactor;
        main.startSpeed = _lastGroundSpeedSqr * _runSpeedFactor;
        _particleFootDownRun.transform.localPosition = _rightFootPosOffset;
        _particleFootDownRun.Play();
    }

    public void PlayLeftFootStep()
    {
        var main = _particleFootDownRun.main;
        main.startSize = _lastGroundSpeedSqr * _runSizeFactor;
        main.startSpeed = _lastGroundSpeedSqr * _runSpeedFactor;
        _particleFootDownRun.transform.localPosition = _leftFootPosOffset;
        _particleFootDownRun.Play();
    }

    public void PlayHurtLand()
    {
        if (_hero.IsGrounded)
            _particleHurt.Play();
    }

    void Start()
    {
        _particleFootDownRun = Instantiate(_particleFootDownRun, _dustsChild);
        _particleHurt = Instantiate(_particleHurt, _dustsChild);
        _particleDJump = Instantiate(_particleDJump, _dustsChild);
        _particleJump = Instantiate(_particleJump, _dustsChild);
        _particleLand = Instantiate(_particleLand, _dustsChild);
        _pushTimer = new EasyTimer(0.02f);
    }

    void Update()
    {
        if (_hero.IsPushing && _pushTimer.Done)
        {
            _pushTimer.Reset();
            var main = _particleFootDownRun.main;
            main.startSize = _pushingSizeFactor;
            main.startSpeed = _pushingSpeedFactor;
            _particleFootDownRun.transform.localPosition = _slidePosOffset;
            _particleFootDownRun.Play();
        }

        // Play Jump
        if (_hero.IsJumping && !_didJump)
        {
            _didJump = true;
            _particleJump.Play();
        }

        // Landed
        if (_hero.IsGrounded)
        {
            _lastGroundSpeedSqr = _hero.Velocity.x * _hero.Velocity.x + _hero.Velocity.z * _hero.Velocity.z;

            if (_wasFalling)
            {
                var landMain = _particleLand.main;
                landMain.startSizeMultiplier = _lastFallSpeed * _landSizeFactor;
                _particleLand.Play();              
            }
            _wasFalling = false;
            _didJump = false;
            _didDJump = false;
        }

        if (_hero.IsDoubleJumping && !_didDJump)
        {
            _didDJump = true;
            _particleDJump.Play();
        }

        if (_hero.IsFalling)
        {
            _wasFalling = true;
        }

        if (_hero.IsFalling)
            _lastFallSpeed = -_hero.Velocity.y;

    }
}
