using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultScreenScript : MonoBehaviour
{
    [SerializeField] private Transitions _transitions;
    private EasyTimer _fadeTimer;
    private bool _fadeingIn = true;

    // Start is called before the first frame update
    void Start()
    {
        _transitions.TransitionType = TransitionType.FadeToBlack;
        _transitions.Value = 0;
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _fadeTimer.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (_fadeingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
            if (_fadeTimer.Done)
                _fadeingIn = false;
        }
    }
}
