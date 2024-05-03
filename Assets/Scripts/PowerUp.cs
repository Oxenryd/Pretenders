using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private Effect _effect;
    [SerializeField] private float _timeToCollectible = 5f;
    [SerializeField] private ParticleSystem _particlePickup;
    [SerializeField] private ParticleSystem _particleCollectible;

    private Material[] _materials;
    private GameObject _graphics;
    private float _counter = 0;
    private Collider _trigger;

    private float _orgSpeedMulti = 0;
    private float _orgSizeMulti = 0;
    public bool AutoApplyEffect { get; set; } = false;
    public bool Enabled { get; private set; } = false;
    public bool IsCollectable { get; set; } = false;
    private EasyTimer _timeToCollectibleTimer;
    private EasyTimer _despawnTimer;
    private EasyTimer _expireTimer;
    private bool _despawning = false;
    private int _colorNameId;
    public bool Expires { get; set; } = true;

    public void Spawn(Vector3 position)
    {
        _despawning = false;
        _expireTimer.Reset();
        var main = _particleCollectible.main;
        main.startSpeedMultiplier = _orgSpeedMulti;
        main.startSizeMultiplier = _orgSizeMulti;
        _trigger.enabled = true;
        Enabled = true;
        transform.position = position;
        gameObject.SetActive(true);
        _graphics.SetActive(true);
        _timeToCollectibleTimer.Reset();
        _counter = 0;
    }

    public void SetEffect(Effect effect)
    { _effect = effect; }

    public Effect Effect
    { get { return _effect; } }

    void Awake()
    {
        var main = _particleCollectible.main;
        _orgSizeMulti = main.startSizeMultiplier;
        _orgSpeedMulti = main.startSpeedMultiplier;
        _expireTimer = new EasyTimer(GlobalValues.POWERUP_DEFAULT_EXPIRETIME);
        _trigger = GetComponent<Collider>();
        _despawnTimer = new EasyTimer(2f);
        Enabled = false;
        _timeToCollectibleTimer = new EasyTimer(_timeToCollectible);
        gameObject.SetActive(false);
        List<Material> matList = new();
        _graphics = transform.GetChild(0).gameObject;
        foreach (Material mat in _graphics.GetComponent<MeshRenderer>().materials)
        { 
            matList.Add(new Material(mat));
        }
        _materials = matList.ToArray();
        var renderer = _graphics.GetComponent<MeshRenderer>();
        renderer.materials = _materials;
        _colorNameId = Shader.PropertyToID("_BaseColor");
        foreach (Material mat in _materials)
        {
            mat.SetColor(_colorNameId, new Color(mat.color.r / 4f, mat.color.g / 4f, mat.color.b / 4f, 0.5f));
        }
        
    }

    void Update()
    {
        if (!Enabled) return;

        _counter += GameManager.Instance.DeltaTime;
        if (_timeToCollectibleTimer.Done && !IsCollectable)
        {
            onActivation();
        } else if (_timeToCollectibleTimer.Ratio > 0.75f && !IsCollectable)
        {
            var multi = 0.3f * Mathf.Cos(_counter * 35f) + 1f;
            transform.localScale = Vector3.one * multi;
        }

        _graphics.transform.Rotate(0, GameManager.Instance.DeltaTime * 35f, 0);

        if (_despawning && _despawnTimer.Done)
        {
            gameObject.SetActive(false);
            enabled = false;
            _despawning = false;
        }

        if (Expires && _expireTimer.Done && _trigger.enabled)
        {
            var main = _particleCollectible.main;
            main.startSpeedMultiplier = -_orgSpeedMulti * 0.66f;
            main.startSizeMultiplier = 0.5f * _orgSizeMulti;
            _particleCollectible.Play();
            _despawning = true;
            _despawnTimer.Reset();
            _trigger.enabled = false;
            _graphics.SetActive(false);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!IsCollectable) return;

        var hero = other.gameObject.GetComponentInParent<HeroMovement>();
        if (hero == null) return;

        IsCollectable = false;
        _trigger.enabled = false;
        hero.Effect = _effect;
        if (AutoApplyEffect)
            _effect.Activate();

        _particlePickup.Play();
        _despawning = true;
        _despawnTimer.Reset();
        _graphics.SetActive(false);
    }

    private void onActivation()
    {
        transform.localScale = Vector3.one;
        foreach (Material mat in _materials)
        {
            mat.SetColor(_colorNameId, new Color(mat.color.r * 4f, mat.color.g * 4f, mat.color.b * 4f, 1f));
        }
        IsCollectable = true;
        _particleCollectible.Play();
        _expireTimer.Reset();
    }
}

