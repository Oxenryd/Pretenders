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
    [SerializeField] private HeroType _heroType = HeroType.Basic;


    public int Index
        { get { return _index; } set { _index = value; } }
    public HeroType HeroType
        { get { return _heroType; } set { _heroType = value; } }
    public ICharacterMovement Movement { get; private set; }
    public AiHeroController AiHeroController { get; private set; }
    public bool Playable { get { return true; } }
    

    private void Awake()
    {
        Movement = GetComponent<HeroMovement>();
        AiHeroController = GetComponent<AiHeroController>();
    }
}
