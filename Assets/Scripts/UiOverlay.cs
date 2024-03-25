using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsValueText;
    [SerializeField] private bool _showFps = true;

    void Awake()
    {
        this.tag = GlobalStrings.NAME_UIOVERLAY;
    }

    void Update()
    {
        if (!_showFps)
        {
            _fpsValueText.text = GameManager.Instance.GetCurrentAvgFpsAsString();
        }
    }



    private void ToggleFps()
    {
        _showFps = !_showFps;
        _fpsValueText.transform.parent.gameObject.SetActive(_showFps);
    }
    private void ToggleFps(bool on)
    {
        _showFps = on;
        _fpsValueText.transform.parent.gameObject.SetActive(on);
    }

}
