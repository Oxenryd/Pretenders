using System;
using TMPro;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Scene = UnityEngine.SceneManagement.Scene;

public class TransitionZoneScript : MonoBehaviour
{
    [SerializeField] private LobbySceneScript _script;
    [SerializeField] private Transitions _transitions;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private TextMeshPro _alert;
    [SerializeField] private string _nextScene;

    private bool _playerInCollider = false;
    private HeroMovement _interactivePlayer = null;
    private EasyTimer _alertTimer;
    private EasyTimer _fadeTimer;
    private bool _fadingIn = false;
    private bool _transitioning = false;

    public event EventHandler TriggeredTransition;
    protected void OnTriggeredTransition()
    { TriggeredTransition?.Invoke(this, EventArgs.Empty); }

    // Start is called before the first frame update
    void Start()
    {
        _alertTimer = new EasyTimer(1f);
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _alertTimer.SetOff();
    }
    // Update is called once per frame
    void Update()
    {
        if (_fadingIn)
        {
            _alert.color = new Color(_alert.color.r, _alert.color.g, _alert.color.b, _alertTimer.Ratio);
        } else
        {
            _alert.color = new Color(_alert.color.r, _alert.color.g, _alert.color.b, 1 - _alertTimer.Ratio);
        }

        if (_transitioning)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                OnTriggeredTransition();
                if (_nextScene == GlobalStrings.TOUR_IDENTIFIER)
                {
                    GameManager.Instance.StartNewTournament();
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                } else
                    GameManager.Instance.TransitToNextScene(_nextScene);
            }
        }

    }
    private void OnDestroy()
    {
        if (_playerInCollider)
            _interactivePlayer.PressedTriggerButton -= onHeroPressedTrigger;
    }


    void OnTriggerStay(Collider other)
    {
        var hero = other.transform.parent.GetComponent<HeroMovement>();
        if (hero == null) return;

        if (!_playerInCollider)
        {
            _playerInCollider = true;
            _interactivePlayer = hero;
            _alertTimer.Reset();
            _alertTimer.Counter = _alert.color.a;
            _fadingIn = true;
            _interactivePlayer.PressedTriggerButton += onHeroPressedTrigger;
        }
    }



    private void OnTriggerExit(Collider other)
    {
        var hero = other.transform.parent.GetComponent<HeroMovement>();
        if (hero == null) return;

        if (hero == _playerInCollider)
        {
            _playerInCollider = false;
            _interactivePlayer.PressedTriggerButton -= onHeroPressedTrigger;
            _interactivePlayer = null;
            _alertTimer.Reset();
            _alertTimer.Counter =  1 -_alert.color.a;
            _fadingIn = false;
        }
    }

    private void onHeroPressedTrigger(object sender, EventArgs e)
    {
        _transitions.TransitionType = TransitionType.CircleFade;
        _fadeTimer.Reset();
        _transitioning = true;
    }
}
