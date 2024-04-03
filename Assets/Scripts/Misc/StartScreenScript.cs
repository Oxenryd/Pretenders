using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    [SerializeField] Transitions _transition;

    private EasyTimer _fadeTimer;
    private bool _activated = false;

    // Start is called before the first frame update
    void Start()
    {
        _fadeTimer = new EasyTimer(0.5f);
        GameManager.Instance.InputManager.HeroPressedButton += StartProceed;
    }

    private void StartProceed(object sender, HeroMovement e)
    {
        var hero = e.GetComponent<Hero>();
        if (hero.Index != 0) return;
        _activated = true;
        _fadeTimer.Reset();
    }

    void Update()
    {
        if (!_activated) return;

        if (_fadeTimer.Done)
            doProceed();

        _transition.Value = 1 - _fadeTimer.Ratio;

    }

    private void doProceed()
    {
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
