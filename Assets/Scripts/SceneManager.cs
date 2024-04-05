using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Scene = UnityEngine.SceneManagement.Scene;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private bool _charactersTakeInput = true;
    [SerializeField] private GameType gameType = GameType.Lobby;
    [SerializeField] private ControlSchemeType _controlScheme;
    [SerializeField] private DragStruggle[] _dragStruggles;

    private int _curDragStrug = 0;
    public ControlSchemeType ControlScheme
    { get { return _controlScheme; } }
    public bool CharactersTakeInput
    { get { return _charactersTakeInput; } set {  _charactersTakeInput = value; } }

    void Awake()
    {
        UnitySceneManager.sceneLoaded += onSceneLoaded;
    }
    private void OnDestroy()
    {
        UnitySceneManager.sceneLoaded -= onSceneLoaded;
    }
    private void onSceneLoaded(Scene argo, LoadSceneMode arg1)
    {
        if (GameManager.Instance.InLoadingScreen) return;
        this.tag = GlobalStrings.NAME_SCENEMANAGER;

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        for (int i = 0; i < _dragStruggles.Length; i++)
        {
            var gObject = (GameObject)Resources.Load("Prefabs/Common/DragStruggle",  typeof(GameObject));
            _dragStruggles[i] = Instantiate(gObject.GetComponent<DragStruggle>(), container.transform);
            _dragStruggles[i].Initialize();
            _dragStruggles[i].gameObject.SetActive(false);
        }
        //GameManager.Instance.SetCurrentSceneManager(this);
    }

    public DragStruggle NextDragStruggle()
    {
        _curDragStrug++;
        if (_curDragStrug > 1)
            _curDragStrug = 0;
        return _dragStruggles[_curDragStrug];
    }
}
