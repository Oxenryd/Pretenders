using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimation : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Hero player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        
    }
}
