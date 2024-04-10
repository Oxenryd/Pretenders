using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageScaler : MonoBehaviour
{
    [SerializeField] private Image _image;
    //[SerializeField] private Canvas _canvas;
    [SerializeField] private Camera _cam;

    // Update is called once per frame
    void LateUpdate()
    {
        //_image.rectTransform.sizeDelta = _cam.pixelRect.size;
    }
}
