using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneScript : MonoBehaviour
{
    [SerializeField] Transitions _transition;

    private EasyTimer _fadeTimer;
    private bool _fadedIn = false;

    // Start is called before the first frame update
    void Start()
    {
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _transition.Value = 0;
        _fadeTimer.Reset();
    }

    //private void StartProceed(object sender, HeroMovement e)
    //{
    //    var hero = e.GetComponent<Hero>();
    //    if (hero.Index != 0) return;
    //    _activated = true;
    //    _fadeTimer.Reset();
    //}

    void Update()
    {
        if (!_fadedIn)
        {
            _transition.Value = _fadeTimer.Ratio;
        }
        if (_fadeTimer.Done)
            _fadedIn = true;  

    }
}