using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Camera Component for following a group of transforms and keep them in the view.
/// </summary>
public class ZoomFollowGang : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform[] _targets = new Transform[4];
    [SerializeField] private float _maxZoomIn = 30f;
    [SerializeField] private Vector3 _startPos = new Vector3(0, 12f, -24f);
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _smoothTimeFactor = 0.65f;
    [SerializeField] public float _bufferFactor = 0f;
    [SerializeField] public bool _stationary = false;
    
    private Vector3 _curVecVel;

    void Update()
    {
        // Calculate the rectangle to keep in view
        float left = float.MinValue;
        float right = float.MaxValue;
        float up = float.MaxValue;
        float down = float.MinValue;
        for (int i = 0 ; i < _targets.Length; i++)
        {
            if (_targets[i].position.x > left)
                left = _targets[i].position.x;
            if (_targets[i].position.x < right)
                right = _targets[i].position.x;
            if (_targets[i].position.z < up)
                up = _targets[i].position.z;
            if (_targets[i].position.z > down)
                down = _targets[i].position.z;
        }
        var space = new Rect(left, up, right - left, down - up);

        // Calculate the distance to ensure all targets fit in the camera's view
        float diagonalSize = Mathf.Sqrt(space.width * space.width + space.height * space.height);
        float calcDistance = diagonalSize * 0.5f / Mathf.Tan(Mathf.Deg2Rad * (_cam.fieldOfView * 0.5f));

        // Having a screen pad buffer that scales with the amount of zoom if needed to keep things in the frame.
        float requiredDistance = calcDistance + calcDistance * _bufferFactor;

        // Stationary camera or moving with group?
        Vector3 cameraPosition = _stationary ?
            _startPos - _cam.transform.forward * Mathf.Clamp(requiredDistance, _maxZoomIn, float.MaxValue) :
            new Vector3(space.center.x, 0, space.center.y) - _cam.transform.forward * Mathf.Clamp(requiredDistance, _maxZoomIn, float.MaxValue);
        var targetPosition = cameraPosition + new Vector3(_offset.x, _offset.y, _offset.z);

        // Let the smoothing time to target be longer when the camera is far away
        float smoothTime = _smoothTimeFactor * requiredDistance * 0.01f;

        // Set the position of the camera
        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, targetPosition, ref _curVecVel, smoothTime);
    }
}
