using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Base class of our heroes. A holder class for our components/modules.
/// </summary>
public class Hero : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    public HeroMovement Movement { get; private set; }
    public AiHeroController AiHeroController { get; private set; }
    

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        Movement = GetComponent<HeroMovement>();
        AiHeroController = GetComponent<AiHeroController>();
    }
}
