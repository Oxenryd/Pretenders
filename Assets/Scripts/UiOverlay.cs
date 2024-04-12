using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsValueText;
    [SerializeField] private bool _showFps = true;

    void Start()
    {
        this.tag = GlobalStrings.NAME_UIOVERLAY;
        toggleFps(_showFps);
    }

    void LateUpdate()
    {
        if (_showFps)
        {
            _fpsValueText.text = GameManager.Instance.GetCurrentAvgFpsAsString();
        }
    }



    private void toggleFps()
    {
        _showFps = !_showFps;
        _fpsValueText.transform.parent.gameObject.SetActive(_showFps);
    }
    private void toggleFps(bool on)
    {
        _showFps = on;
        _fpsValueText.transform.parent.gameObject.SetActive(on);
    }

}
