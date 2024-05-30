using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Class that enable toggling of fps counter.
/// </summary>
public class UiOverlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsValueText;
    [SerializeField] private bool _showFps = true;

    void Start()
    {
        this.tag = GlobalStrings.NAME_UIOVERLAY;
        ToggleFps(_showFps);
    }

    void LateUpdate()
    {
        if (_showFps)
        {
            _fpsValueText.text = GameManager.Instance.GetCurrentAvgFpsAsString();
        }
    }

    public void ToggleFps(bool on)
    {
        _showFps = on;
        _fpsValueText.transform.parent.gameObject.SetActive(on);
    }

}
