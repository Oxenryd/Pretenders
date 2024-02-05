using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomFollowGang : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform[] _targets = new Transform[4];
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _smoothTime = 0.3f;

    public float _bufferDistance = 1f;
    private float _bounds = 1000;
    private Vector3 _targetPosition;
    private Vector3 _curVecVel;
    Rect _space;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float left = -_bounds;
        float right = _bounds;
        float up = _bounds;
        float down = -_bounds;
        
        for (int i = 0 ; i < _targets.Length; i++)
        {
            if (_targets[i].position.x > left)
                left = _targets[i].position.x;
            if (_targets[i].position.x < right)
                right = _targets[i].position.x;
            if (_targets[i].position.y < up)
                up = _targets[i].position.y;
            if (_targets[i].position.y > down)
                down = _targets[i].position.y;
        }

        _space = new Rect(left, up, right - left, down - up);
 
        // Calculate the distance to ensure all objects fit in the camera's view
        float requiredDistance = CalculateRequiredDistance(_cam, _space.height, _space.width, _bufferDistance);
        Vector3 baryCenter = (_targets[0].position + _targets[1].position + _targets[2].position + _targets[3].position) / 4;
        Vector3 cameraTarget = baryCenter;// space.center;
        Vector3 cameraPosition = cameraTarget - _cam.transform.forward * requiredDistance;

        _targetPosition = cameraPosition - new Vector3(0, 0, _offset.z);

        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, _targetPosition, ref _curVecVel, _smoothTime);
    }

    float CalculateRequiredDistance(Camera cam, float width, float length, float buffer)
    {
        float diagonalSize = Mathf.Sqrt(width * width + length * length);
        float requiredDistance = diagonalSize * 0.5f / Mathf.Tan(Mathf.Deg2Rad * (cam.fieldOfView * 0.5f));

        requiredDistance += buffer;

        return requiredDistance;
    }


}
