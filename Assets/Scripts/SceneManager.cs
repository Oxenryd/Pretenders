using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour, IScene
{
    [SerializeField] private ControlSchemeType _controlScheme;
    [SerializeField] private DragStruggle[] _dragStruggles;

    private int _curDragStrug = 0;
    public ControlSchemeType ControlScheme
    { get { return _controlScheme; } }

    void Awake()
    {
        this.tag = GlobalStrings.NAME_SCENEMANAGER;

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        for (int i = 0; i < 2; i++)
        {
            _dragStruggles[i] = Instantiate(_dragStruggles[i], container.transform);
            _dragStruggles[i].gameObject.SetActive(false);
        }
    }

    public DragStruggle NextDragStruggle()
    {
        _curDragStrug++;
        if (_curDragStrug > 1)
            _curDragStrug = 0;
        return _dragStruggles[_curDragStrug];

    }

}
