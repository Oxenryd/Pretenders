using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class of our heroes. A holder class for our components/modules.
/// </summary>
public class Hero : MonoBehaviour, ICharacter
{
    [SerializeField] private int _index;
    [SerializeField] private AiHeroController _aiControl;
    [SerializeField] private HeroType _heroType = HeroType.Basic;
    [SerializeField] private Collider _bodyCollider; 
    [SerializeField] private HeadBody _headFeet;
    [SerializeField] private float _shovePower = 22f;
    [ColorUsage(false)][SerializeField] private Color _primaryColor;
    [ColorUsage(false)][SerializeField] private Color _secondaryColor;
    [SerializeField] private Material _primMat;
    [SerializeField] private Material _secMat;

    private HeroMovement _movement;
    
    public Grabbable CurrentGrab
    { get { return _movement.CurrentGrab; } }
    public float ShovePower
        { get { return _shovePower; } set { _shovePower = value; } }
    public int Index
        { get { return _index; } set { _index = value; } }
    public HeroType HeroType
        { get { return _heroType; } set { _heroType = value; } }
    public HeroMovement Movement
        { get { return _movement; } }
    public AiHeroController AiHeroController
        { get { return _aiControl; }  }
    public bool Playable { get { return true; } }
    public HeadBody HeadFeet
        { get { return _headFeet; } }
    public Collider BodyCollider
        { get { return _bodyCollider; } }
    public Color PrimaryColor
    { get { return _primaryColor; } set { _primaryColor = value; } }
    public Color SecondaryColor
    { get { return _secondaryColor; } set { _secondaryColor = value; } }


    private void Awake()
    {
        _movement = GetComponent<HeroMovement>();
    }
}