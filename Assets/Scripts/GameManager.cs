using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Class that holds information about the game state and other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    private Transform _camTransform;

    // Don't forget to alter these in Inspector.  
    [SerializeField] private int _maxControllables = 4;
    [SerializeField] private InputManager _inputMan;
    [SerializeField] private SceneManager _curSceneman;
    [SerializeField] private Music _music;
    [SerializeField] private Transitions _transitions;

    private float[] _fpsBuffer;
    private int _fpsCounter = 0;
    private string[] _digitStrings;
    private string[] _numberStrings;
    private float[] _tournamentScore;
    private float[] _scoreMultiplier;

    private string[] _tournamentGameList;
    private int _currentTournamentScene = 0;

    private bool _firstStart = true;

    public bool InLoadingScreen { get; set; }

    private List<int> _lastStandings;
    private List<MatchResult> _currentResults;

    private AsyncOperation _unloadingPrevious;

    private ICharacter[] _playableCharacters;
    private int _numPlayers = 1;
    private static GameManager _instance;
    /// <summary>
    /// The Singleton instance of our GameManager.
    /// <br>Singleton pattern in unity gameobject is special...</br>
    /// </summary>
    public static GameManager Instance
    { get { return _instance; } }

    public bool Tournament
    { get; private set; } = false;

    public void SetCurrentSceneManager(SceneManager sceneManager)
    {
        _curSceneman = sceneManager;
    }

    public Music Music
    { get { return _music; } }


    public bool DebuggingResultScreen { get; set; } = false;
    public float UnloadProgress
    { get { return _unloadingPrevious.progress; } }
    public int FpsMaximumSamples { get; set; } = 120;
    public long TotalFrames { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }
    public Transitions Transitions
    { get { return _transitions; } }
    public int LastSceneIndex { get; private set; }
    public string NextScene { get; private set; }
    public bool FromSceneLoaded { get; private set; } = false;
    // Events
    public EventHandler<float> EarlyUpdate;
    public EventHandler<float> EarlyFixedUpdate;
    public EventHandler<int> NumOfPlayersChanged;
    protected void OnEarlyUpdate(float deltaTime)
    {
        if (EarlyUpdate == null) return;
        EarlyUpdate.Invoke(this, deltaTime);
    }
    protected void OnEarlyFixedUpdate(float fixedDeltaTime)
    {
        if (EarlyFixedUpdate == null) return;
        EarlyFixedUpdate.Invoke(this, fixedDeltaTime);
    }
    protected void OnNumberPlayersChanged()
    {
        if (NumOfPlayersChanged == null) return;
        NumOfPlayersChanged.Invoke(this, NumOfPlayers);
    }

    public void StartNewTournament()
    {
        Tournament = true;
        _scoreMultiplier = new float[] { 1f, 1f, 1f,1f };
        _currentTournamentScene = 0;
        _lastStandings.Clear();
        _currentResults.Clear();
        List<int> matchOrder = new();
        List<int> minigameOrder = new();
        System.Random rand = new();

        while (matchOrder.Count < GlobalStrings.MATCHES_NAMES.Length)
        {
            var index = rand.Next(0, 3);
            if (!matchOrder.Contains(index))
                matchOrder.Add(index);
        }
        while (minigameOrder.Count < GlobalStrings.MINIGAMES_NAMES.Length)
        {
            var index = rand.Next(0, 2);
            if (!minigameOrder.Contains(index))
                minigameOrder.Add(index);
        }

        List<string> stringList = new List<string>();
        for (int i = 0; i < matchOrder.Count; i++)
        {
            stringList.Add(GlobalStrings.MATCHES_NAMES[matchOrder[i]]);
            if (i < matchOrder.Count - 1)
                stringList.Add(GlobalStrings.MINIGAMES_NAMES[minigameOrder[i]]);
        }
        _tournamentGameList = stringList.ToArray();
    }

    public float GetPlayerMultiplier(int playerIndex)
    { return _scoreMultiplier[playerIndex]; }
    public void SetPlayerMultiplier(int playerIndex, float multiplier)
    { _scoreMultiplier[playerIndex] = multiplier; }
    public void ResetPlayerMultipliers()
    { _scoreMultiplier = new float[] { 1f, 1f, 1f, 1f }; }
    public string GetTournamentNextScene()
    {
        var sceneString = _tournamentGameList[_currentTournamentScene];
        _currentTournamentScene++;
        return sceneString;
    }

    public string[] TournamentGameList
    { get { return _tournamentGameList; } }

    public void AddNewMatchResult(MatchResult result)
    {
        _currentResults.Add(result);
    }

    public int[] GetLastStandings()
    { return _lastStandings.ToArray(); }
    public MatchResult[] GetMatchResults()
    { return _currentResults.ToArray(); }
    public float GetTournamentScore(int playerIndex)
    { return _tournamentScore[playerIndex]; }
    public void IncreaseTournamentScore(int playerIndex, float score)
    { _tournamentScore[playerIndex] += score; }
    public void ResetTournamentScore()
    {

        for (int i = 0; i < _tournamentScore.Length; i++)
        {
            _tournamentScore[i] = 0f;
        }
    }
    public SceneManager SceneManager
    { get { return _curSceneman; } }
    public InputManager InputManager
    { get { return _inputMan; } }
    public float DeltaTime { get; private set; }
    public float FixedDeltaTime { get; private set; }
    public int NumOfPlayers
    {
        get { return _numPlayers; }
        set
        {
            if (value < 0)
            {
                Debug.LogError(GlobalStrings.ERR_NUMBER_OF_PLAYERS1);
                _numPlayers = 0;
            }
            else if (value > 4)
            {
                Debug.LogError(GlobalStrings.ERR_NUMBER_OF_PLAYERS2);
                _numPlayers = 4;
            }
            else
                _numPlayers = value;

            OnNumberPlayersChanged();
        }
    }
    public int MaxControllableCharacters
    { get { return _maxControllables; } set { _maxControllables = value; } }
    public ICharacter[] PlayableCharacters
    { get { return _playableCharacters; } }

    public void TransitToNextScene(string nextScene)                                            // Not doing Additive sceneloading
    {
        NextScene = nextScene;
        LastSceneIndex = UnitySceneManager.GetActiveScene().buildIndex;
        // UnitySceneManager.LoadScene(GlobalStrings.SCENE_LOADINGSCREEN, LoadSceneMode.Additive);
        // showLoadingScreen();
        //UnitySceneManager.LoadScene(nextScene);
        UnitySceneManager.LoadScene(NextScene);

    }

    public void UnloadLastScene()
    {
        StartCoroutine(unloadLoadScreenScene());
    }

    private IEnumerator unloadLoadScreenScene()
    {
        _unloadingPrevious = UnitySceneManager.UnloadSceneAsync(GlobalStrings.SCENE_LOADINGSCREEN);

        while (!_unloadingPrevious.isDone)
        {
            yield return null;
        }
    }
    
    private void onSceneLoaded(Scene arg0, LoadSceneMode arg1)
    { 
        _camTransform = Camera.main.transform;
        if (!checkThisIsTheOneAndOnly())
        {
            Destroy(this.gameObject);
            return;
        }

        if (InLoadingScreen)
            return;

        if (_firstStart)
            return;

        findAndEnumHeroes(false);

        var transition = GameObject.FindGameObjectWithTag(GlobalStrings.TRANSITIONS_TAG);
        _transitions = transition.GetComponent<Transitions>();

        var sceneMan = GameObject.FindGameObjectsWithTag(GlobalStrings.NAME_SCENEMANAGER);//.GetComponent<SceneManager>();
        _curSceneman = sceneMan[^1].GetComponent<SceneManager>();
        List<HeroMovement> movements = new List<HeroMovement>();
        foreach (var character in _playableCharacters)
        {
            character.Movement.AcceptInput = _curSceneman.CharactersTakeInput;
            movements.Add(character.Movement);
        }
        _inputMan.ResetHeroes(movements.ToArray());

        FromSceneLoaded = true;
    }

    private bool checkThisIsTheOneAndOnly()
    {
        if (Instance != null && this != Instance)
        {
            return false;
        }
        else
        {
            _instance = this;
        }
        return true;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _currentResults = new List<MatchResult>();
        _lastStandings = new List<int>();
        if (!checkThisIsTheOneAndOnly())
        {
            Destroy(this.gameObject);
            return;
        }

        _tournamentScore = new float[] { 0f, 0f, 0f, 0f };

        this.tag = GlobalStrings.NAME_GAMEMANAGER;
        UnitySceneManager.sceneLoaded -= onSceneLoaded;
        UnitySceneManager.sceneLoaded += onSceneLoaded;

        // Pre caching strings
        _digitStrings = new string[10];
        for (int i = 0; i < 10; i++)
        {
            _digitStrings[i] = i.ToString();
        }
        _numberStrings = CreateNumberStrings(GlobalValues.STRINGS_MAX_PRECACHED_NUMBERSTRINGS, 0);


        // Setup FPS counter stuffs
        _fpsBuffer = new float[FpsMaximumSamples];
    }

    private void OnDestroy()
    {
        UnitySceneManager.sceneLoaded -= onSceneLoaded;
    }

    void Start()
    {
        Cursor.visible = false;

        if (!checkThisIsTheOneAndOnly())
        {
            Destroy(this.gameObject);
            return;
        }

        // First, set up for none players.
        NumOfPlayers = 0;

        // Makes sure that the GameManger COULD run without an InputManager if that¨s needed.
        if (_inputMan != null)
        {
            findAndEnumHeroes(true);

            // Setting up first Player with simple keyboard Control.
            //_inputMan.SetupKeyboardPlayerOne();
            _inputMan.SetupDefaultEmptyInputs();

            _firstStart = false;
        }

        // Makeing sure this item survives between scenes.
        // Be aware that this moves this object in the objects list during runtime in editor.
        // To "Don't Destroy On Load" - object.
        DontDestroyOnLoad(this);
    }



    public void ApplyControlScheme()
    { ApplyControlScheme(_curSceneman.ControlScheme); }
    public void ApplyControlScheme(ControlSchemeType controlScheme)
    {
        foreach (var character in _playableCharacters)
        {
            character.Movement.CurrentControlScheme = controlScheme;
        }
    }


    // The GameManager Update is being executed before all other MonoBehaviors Update().
    // Project settings -> Script Execution Order
    void Update()
    {
        // Cache external call to deltaTime.
        // Small benefit to use the gameManager's cached version instead of calling
        // the external Time.deltaTime in every objects' Update().
        DeltaTime = Time.deltaTime;

        // Invoke Early Update for subscribers.
        OnEarlyUpdate(DeltaTime);

        transform.position = _camTransform.position;
    }
    private void FixedUpdate()
    {
        FixedDeltaTime = Time.fixedDeltaTime;
        OnEarlyFixedUpdate(FixedDeltaTime);
    }

    void LateUpdate()
    {
        // Update FPS
        CurrentFramesPerSecond = (1f / DeltaTime);
        _fpsBuffer[_fpsCounter] = CurrentFramesPerSecond;
        _fpsCounter++;
        if (_fpsCounter >= FpsMaximumSamples)
        {
            float total = _fpsBuffer.Sum();
            AverageFramesPerSecond = total / _fpsCounter;
            _fpsCounter = 0;
        }
        TotalFrames++;
    }

    /// <summary>
    /// Set the number of human players currently in the game.
    /// </summary>
    /// <param name="numOfHumanPlayers"></param>
    public void SetNumberPlayers(int numOfHumanPlayers)
    {
        NumOfPlayers = numOfHumanPlayers;
        OnNumberPlayersChanged();
    }

    public string DigitToString(int digit)
    {
        int d = digit;
        if (digit < 0)
            d = 0;
        else if (digit > 9)
            d = 9;
        return _digitStrings![d];
    }

    public string IntegerToString(int value)
    {
        if (value < 0)
            return _numberStrings[0];
        if (value >= _numberStrings.Length)
            return _numberStrings[_numberStrings.Length - 1];

        return _numberStrings[value];
    }

    public string GetCurrentAvgFpsAsString()
    {
        return IntegerToString((int)AverageFramesPerSecond);
    }

    public static string[] CreateNumberStrings(int maxValue, int totalDigits)
    {
        string[] stringArray = new string[maxValue];
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < maxValue; i++)
        {
            int wholeTens = 0;
            int current = i;
            while (current > 9)
            {
                current = current / 10;
                wholeTens++;
            }
            int numberOfZeroesToWrite = totalDigits - wholeTens - 1;
            sb.Clear();
            for (int j = 0; j < numberOfZeroesToWrite; j++)
            {
                sb.Append("0");
            }
            sb.Append(i);
            stringArray[i] = sb.ToString();
        }
        return stringArray;
    }

    private void findAndEnumHeroes(bool init)
    {
        // Find characters in the scene then assign them to and init the inputmanager.
        // Also populate this managers' info about characters
        var sceneChars = GameObject.FindGameObjectsWithTag(GlobalStrings.CHARACTER_TAG);
        var charList = new List<ICharacter>();

        for (int i = 0; i < sceneChars.Length; i++)
        {
            var character = sceneChars[i].gameObject.GetComponent<ICharacter>();
            if (character != null && character.Playable == true)
            {
                charList.Add(character);
            }
            if (charList.Count == _maxControllables)
                break;
        }

        // Naive sorting of the characters
        var sortedCharList = new List<ICharacter>();
        var iMoveList = new List<HeroMovement>();
        for (int i = 0; i < charList.Count; i++)
        {
            for (int j = 0; j < charList.Count; j++)
            {
                if (charList[j].Index == i)
                {
                    sortedCharList.Add(charList[j]);
                    iMoveList.Add(charList[j].Movement);
                    break;
                }
            }
        }

        _playableCharacters = sortedCharList.ToArray();

        if (init)
            _inputMan.Initialize(iMoveList.ToArray());
    }
}
