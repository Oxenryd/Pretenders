using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class of our heroes. A holder class for our components/modules.
/// </summary>
public class Hero : MonoBehaviour, ICharacter, IJumpHit
{
    [SerializeField] private int _index;
    [SerializeField] private AiHeroController _aiControl;
    [SerializeField] private HeroType _heroType = HeroType.Basic;
    [SerializeField] private Collider _bodyCollider; 
    [SerializeField] private HeadFeet _headFeet;
    [SerializeField] private float _shovePower = 22f;
    [ColorUsage(false)][SerializeField] private Color _primaryColor;
    [ColorUsage(false)][SerializeField] private Color _secondaryColor;
    [SerializeField] private Material _primMat;
    [SerializeField] private Material _secMat;

    private EasyTimer _shoveOffenderColDisableTimer;
    private bool _colShoveDisabled = false;
    private ICharacterMovement _movement;
    
    public Grabbable CurrentGrab
    { get { return _movement.CurrentGrab; } }
    public float ShovePower
        { get { return _shovePower; } set { _shovePower = value; } }
    public int Index
        { get { return _index; } set { _index = value; } }
    public HeroType HeroType
        { get { return _heroType; } set { _heroType = value; } }
    public ICharacterMovement Movement
        { get { return _movement; } }
    public AiHeroController AiHeroController
        { get { return _aiControl; }  }
    public bool Playable { get { return true; } }
    public HeadFeet HeadFeet
        { get { return _headFeet; } }
    public Collider BodyCollider
        { get { return _bodyCollider; } }
    public Color PrimaryColor
    { get { return _primaryColor; } set { _primaryColor = value; } }
    public Color SecondaryColor
    { get { return _secondaryColor; } set { _secondaryColor = value; } }

    public void OnHeadHit(Hero offender)
    {
    }

    public void OnHitOthersHead(Hero victim)
    {   
        // Send the shove to the victim and temporarily disable this collider
        // to avoid the victim to get "stuck" during first part of shove.
        BodyCollider.enabled = false;
        _colShoveDisabled = true;
        _shoveOffenderColDisableTimer.Reset();
        victim.Movement.TryShove(Movement.FaceDirection, ShovePower);
    }

    private void Awake()
    {
        _movement = GetComponent<ICharacterMovement>();
    }

    void Start()
    {
        // Set color to prefab instance picked color.
        var bodyRend = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        bodyRend.sharedMaterial = new Material(_primMat);
        bodyRend.sharedMaterial.color = _primaryColor;
        var headRend = transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>();
        headRend.sharedMaterial = new Material(_secMat);
        headRend.sharedMaterial.color = _secondaryColor;

        _shoveOffenderColDisableTimer = new EasyTimer(GlobalValues.SHOVE_OFFENDCOL_DIS_DUR, false, true);
    }



    void FixedUpdate()
    {
        // Check if the disabled collider timer has run out and reenable the body collider
        if (_colShoveDisabled && _shoveOffenderColDisableTimer.Done)
        {
            BodyCollider.enabled = true;
            _colShoveDisabled = false;
        }
    }

    // Collisions
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the other is a hero
        var otherHero = collision.collider.gameObject.GetComponent<Hero>();
        if (otherHero != null)
        {
            // Check if my head is in collision with others' feet
            // (Just using 'this' for clarification
            if (otherHero.HeadFeet.FeetBox.bounds.Intersects(this.HeadFeet.HeadBox.bounds))
            {
                // Don't shove if this hero is already being shoved around
                if (!this.Movement.IsShoved && otherHero.Movement.IsFalling)
                {
                    otherHero.OnHitOthersHead(this);
                }
                this.OnHeadHit(otherHero);
            } else
            {
                // Bump each character backwards
                Movement.TryBump(new Vector3(-Movement.FaceDirection.x, 1f, -Movement.FaceDirection.z).normalized, GlobalValues.CHAR_BUMPFORCE);
            }
        }
    }
}