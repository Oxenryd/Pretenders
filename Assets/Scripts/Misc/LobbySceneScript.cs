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
        if (GameManager.Instance.FromSceneLoaded)
            GameManager.Instance.Transitions.Value = 0;
        else
            _transition.Value = 1;
        _fadeTimer.Reset();
        GameManager.Instance.Music.Fadeout(3f);
    }

    void Update()
    {
        if (!_fadedIn)
        {
            if (GameManager.Instance.FromSceneLoaded)
                GameManager.Instance.Transitions.Value = _fadeTimer.Ratio;
        }
        if (_fadeTimer.Done)
            _fadedIn = true;  

    }
}