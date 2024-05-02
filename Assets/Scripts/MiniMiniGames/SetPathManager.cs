using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathManager : MonoBehaviour
{
    [SerializeField] private ZoomFollowGang _cam;
    [SerializeField] private GetReadyScript _getReady;
    [SerializeField] private WinnerTextScript _winnerText;
    [SerializeField] private Transitions _transitions;
    [SerializeField] private HeroMovement[] _heroes;
    public GameObject headlightPrefab;
    public GameObject[] spHeroes;

    private float groundLvl;
    private Vector3 startPosition;
    private bool _isFadingIn = true;
    private bool _isFadingOut = false;
    private bool _zoomingToWinner = false;
    private EasyTimer _fadeTimer;

    public void InformWinnerFound(Transform winnerTransform)
    {
        _cam.SetWinner(winnerTransform, false);
        _winnerText.Activate();
        _zoomingToWinner = true;
        _fadeTimer.Time = _fadeTimer.Time * 3;
        _fadeTimer.Reset();
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (var hero in _heroes)
        {
            hero.TryMoveAi(new Vector2(0, 1));
        }

        _transitions.TransitionType = TransitionType.CircleFade;
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _fadeTimer.Reset();
        _getReady.Activate();
        GameManager.Instance.ResetPlayerMultipliers();
        GameManager.Instance.Music.Fadeout(1.5f);

        Vector3 lightOneStartPos = new Vector3 (0, 0, 0);
        Vector3 lightTwoStartPos = new Vector3 (0, 0, 0);
        Vector3 lightThreeStartPos = new Vector3 (0, 0, 0);

        // Spawn in one/multiple light prefabs

        // Initiate default values
        groundLvl = -2F;
        startPosition = new Vector3(20, 3, 3);

        // Add all heroes to the hero list
        spHeroes = GameObject.FindGameObjectsWithTag("Character");
    }

    // Update is called once per frame
    void Update()
    {
        // Randomize the lights next location
        // Move the lights to the next location

        // Check if any hero has fallen of the trail and tp them back if so
        foreach (GameObject heroObj in spHeroes)
        {
            if (heroObj.transform.position.y < groundLvl)
            {
                heroObj.transform.position = startPosition;
            }
        }

        if (_isFadingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                _isFadingIn = false;
            }
        }

        if (_isFadingOut)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if ( _fadeTimer.Done)
            {
                if (GameManager.Instance.Tournament)
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                else
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }

        if (_zoomingToWinner && !_isFadingOut)
        {
            if (_fadeTimer.Done)
            {
                _isFadingOut = true;
                _fadeTimer.Time = _fadeTimer.Time / 3f;
                _fadeTimer.Reset();
            }
        }
    }
}
