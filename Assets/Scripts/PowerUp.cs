using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private GameObject _preFab;
    [SerializeField] private GameType[] CurrentGameTypes;
    private Effect _effect;
    [SerializeField] private float _timeToCollectible = 5f;
    public bool Collected { get; set; } = false;
    public bool IsCollectable { get; set; } = false;
    private EasyTimer _timeToCollectibleTimer;

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public Collider GetCollider()
    {
        return transform.GetComponent<Collider>();
    }
    void Awake()
    {
        _timeToCollectibleTimer = new EasyTimer(_timeToCollectible);
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (_timeToCollectibleTimer.Done)
        {
            OnActivation();
        }
    }
    public void Spawn()
    {
        gameObject.SetActive(true);
        _timeToCollectibleTimer.Reset();
    }
    public void OnActivation()
    {
        IsCollectable = true;
    }
    public void OnPickup()
    {
        Collected = true;
        IsCollectable = false;
        ApplyEffect();
    }
    public void ApplyEffect()
    {
        _effect.Activate();
    }
    public void OnExpire()
    {
        Collected = false;
        gameObject.SetActive(false);
    }
    public void OnTriggerEnter(Collider other)
    {
        var hero = other.gameObject.GetComponent<HeroMovement>();
        if (hero == null)
        {
            return;
        }
        if (IsCollectable)
        {
            OnPickup();
            hero.Effect = _effect;
        } else
        {
            return;
        }
    }
}

