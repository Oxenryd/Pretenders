using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class which determines the behaviour of the camera in the platform brawler minigame.
/// Ensures that all players stay in the frame until they fall of the map and out of bounds.
/// </summary>
public class PlatformCamera : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private float _maxZoomIn = 5f;
    [SerializeField] private float _maxZoomOut = 20f;
    [SerializeField] private Vector3 _startPos = new Vector3(0, 12f, -24f);
    [SerializeField] private Vector3 _zoomPanOffset = new Vector3(0, 0, -0.1f);
    [SerializeField] private float _smoothTimeFactor = 0.08f;
    [SerializeField] public float _bufferFactor = -0.35f;
    [SerializeField] public bool _stationary = false;
    [SerializeField] public bool _zoomDependentOffset = true;
    [SerializeField] public float _deathAltitude;
    [SerializeField] private List<Transform> targets = new List<Transform>();

    private Vector3 _curVecVel;

    void Update()
    {
        // Calculate the rectangle to keep in view
        float left = float.MinValue;
        float right = float.MaxValue;
        float up = float.MaxValue;
        float down = float.MinValue;
        
        for (int i = 0; i < targets.Count; i++)
        {
            //Removes player as a target for the camera if they fall out of bounds
            if (targets[i].transform.position.y <= _deathAltitude)
            {
                targets.RemoveAt(i);
                continue;
            }
            if (targets[i].position.x > left)
                left = targets[i].position.x;
            if (targets[i].position.x < right)
                right = targets[i].position.x;
            if (targets[i].position.z < up)
                up = targets[i].position.z;
            if (targets[i].position.z > down)
                down = targets[i].position.z;            
        }

        var space = new Rect(left, up, right - left, down - up);

        // Calculate the distance to ensure all targets fit in the camera's view
        float diagonalSize = Mathf.Sqrt(space.width * space.width + space.height * space.height);
        float calcDistance = diagonalSize * 0.5f / Mathf.Tan(Mathf.Deg2Rad * (_cam.fieldOfView * 0.5f));

        // Having a screen pad buffer that scales with the amount of zoom if needed to keep things in the frame.
        float requiredDistance = calcDistance + calcDistance * _bufferFactor;

        // Stationary camera or moving with group?
        Vector3 cameraPosition = _stationary ?
            _startPos - _cam.transform.forward * Mathf.Clamp(requiredDistance, _maxZoomIn, _maxZoomOut) :
            new Vector3(space.center.x, space.center.y * 1.1f, 0) - _cam.transform.forward * Mathf.Clamp(requiredDistance, _maxZoomIn, _maxZoomOut);
        var targetPosition = _zoomDependentOffset ?
            cameraPosition + _zoomPanOffset * requiredDistance :
            cameraPosition;

        // Let the smoothing time to target be longer when the camera is far away
        float smoothTime = _smoothTimeFactor * requiredDistance * 0.01f;

        // Set the position of the camera
        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, targetPosition, ref _curVecVel, smoothTime);
    }
}
