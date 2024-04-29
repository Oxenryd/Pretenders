using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Camera Component for following a group of transforms and keep them in the view.
/// </summary>
public class ZoomFollowGang : MonoBehaviour
{
    [SerializeField] private GameType _mode;
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform[] _targets = new Transform[4];
    [SerializeField] private float _maxZoomIn = 30f;
    [SerializeField] private Vector3 _startPos = new Vector3(0, 12f, -24f);
    [SerializeField] private Vector3 _posOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _zoomPanOffset = new Vector3(0, 0, -0.1f);
    [SerializeField] private float _smoothTimeFactor = 0.08f;
    [SerializeField] public float _bufferFactor = -0.35f;
    [SerializeField] public bool _stationary = false;
    [SerializeField] public bool _zoomDependentOffset = true;
    [SerializeField] private float _winnerZoomTime = 0.7f;
    [SerializeField] private float _winnerZoom = 2.2f;
    [SerializeField] private float _distance = 10;
    [SerializeField] private float _orthoZoomOffset = 0f;
    [SerializeField] private float _orthoZoomFactor = 1f;
    [SerializeField] public float _deathAltitude;
    [SerializeField] public float _orthoMaxZoom = 6f;

    private float _curWinZoom = 21.3f;


    public bool OrthoMode { get; set; } = false;
    public Transform[] Targets { get { return _targets; } set { _targets = value; }}
    public bool FollowingWinner { get; set; } = false;
    public Transform WinnerTransform { get; set; }
    private Vector3 _curVecVel;
    private float _t = 0;
    private Vector3 _startPosWinner;

    public void RemoveActor(int playerIndex)
    {
        var objs = new List<GameObject>();
        foreach (var obj in _targets)
        {
            objs.Add(obj.gameObject);
        }
        var heroes = new List<Hero>();
        foreach (var hero in objs)
        {
            heroes.Add(hero.GetComponent<Hero>());
        }
        var tempList = heroes.Where(hero => hero.Index != playerIndex).ToList();
        var transforms = new List<Transform>();
        foreach (var hero in tempList)
        {
            transforms.Add(hero.transform);
        }
        _targets = transforms.ToArray();
    }

    public void SetWinner(Transform winner, bool orthoMode)
    {
        OrthoMode = orthoMode;
        _startPosWinner = transform.position;
        _curWinZoom = _cam.orthographicSize;
        FollowingWinner = true;
        WinnerTransform = winner;
        _t = 0;
    }
    void Update()
    {
        if (_stationary && !FollowingWinner) return;

        if (FollowingWinner)
        {
            var target = WinnerTransform.position + -transform.forward * _winnerZoom;
            _t += GameManager.Instance.DeltaTime;
            if (OrthoMode)
                _cam.orthographicSize = Mathf.Lerp(_curWinZoom, _winnerZoom, _t);
            transform.position = Vector3.Lerp(_startPosWinner, target, Mathf.Clamp01(_t / _winnerZoomTime));
            return;
        }

        if (!_cam.orthographic)
        {
            // Calculate the rectangle to keep in view
            float left = float.MinValue;
            float right = float.MaxValue;
            float up = float.MaxValue;
            float down = float.MinValue;
            for (int i = 0; i < _targets.Length; i++)
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
            var targetPosition = _zoomDependentOffset ?
                cameraPosition + _zoomPanOffset * requiredDistance :
                cameraPosition;

            // Let the smoothing time to target be longer when the camera is far away
            float smoothTime = _smoothTimeFactor * requiredDistance * 0.01f;

            // Set the position of the camera
            _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, targetPosition, ref _curVecVel, smoothTime) + _posOffset;
        } else
        {
            // Calculate the rectangle to keep in view
            float left = float.MaxValue;
            float right = float.MinValue;
            float top = float.MinValue;
            float bottom = float.MaxValue;

            for (int i = 0; i < _targets.Length; i++)
            {
                left = Mathf.Min(left, _targets[i].position.x);
                right = Mathf.Max(right, _targets[i].position.x);
                bottom = Mathf.Min(bottom, _targets[i].position.z);
                top = Mathf.Max(top, _targets[i].position.z);

                if (_targets[i].position.y <= _deathAltitude)
                {
                    var hero = _targets[i].GetComponent<Hero>();
                    RemoveActor(hero.Index);
                    continue;
                }
            }

            var width = right - left;
            var height = top - bottom;
            var center = new Vector2(left + width * 0.5f, bottom + height * 0.5f);

            // Set camera's orthographic size to encompass the largest dimension
            float aspectRatio = _cam.aspect;
            float targetOrthoSize = Mathf.Max((height > width / aspectRatio) ? height * 0.5f : (width / aspectRatio) * 0.5f * _orthoZoomFactor + _orthoZoomOffset, _orthoMaxZoom);
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetOrthoSize, GameManager.Instance.DeltaTime);

            // Calculate the camera position
            Vector3 cameraPosition = new Vector3(center.x, 0, center.y) - _cam.transform.forward * (_distance);

            // Set the position of the camera smoothly
            _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, cameraPosition + _posOffset, ref _curVecVel, _smoothTimeFactor * _distance * 0.01f);
        }     
    }
}
