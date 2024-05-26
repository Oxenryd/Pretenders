using UnityEngine;

/// <summary>
/// Class that got demoted to more or less just keep the dragstruggles and rotate them.
/// In earlier development the idea was that this should keep scene-specific information but that got handed out to other classes.
/// </summary>
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
        this.tag = GlobalStrings.NAME_SCENEMANAGER;

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        for (int i = 0; i < _dragStruggles.Length; i++)
        {
            _dragStruggles[i] = Instantiate(_dragStruggles[i], container.transform);
            _dragStruggles[i].Initialize();
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