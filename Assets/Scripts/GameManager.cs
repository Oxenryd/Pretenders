using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Class that holds information about the game state and other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Don't forget to alter these in Inspector.  
    [SerializeField] private int _maxControllables = 4;
    [SerializeField] private InputManager _inputMan;
    [SerializeField] private SceneManager _curSceneman;
    [SerializeField] private Transitions _screenTransitions;

    private float[] _fpsBuffer;
    private int _fpsCounter = 0;
    private string[] _digitStrings;
    private string[] _numberStrings;

    private bool _loadingScene = false;

    private List<MatchResult> _currentResults;

    AsyncOperation _loadingNext;
    AsyncOperation _unloadingPrevious;

    private ICharacter[] _playableCharacters;
    private int _numPlayers = 1;
    private static GameManager _instance;
    /// <summary>
    /// The Singleton instance of our GameManager.
    /// <br>Singleton pattern in unity gameobject is special...</br>
    /// </summary>
    public static GameManager Instance
        { get { return _instance; } }

    public int FpsMaximumSamples { get; set; } = 120;
    public long TotalFrames { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public int LastSceneIndex { get; private set; }
    public string NextScene { get; private set; }

    public Transitions ScreenTransitions
    { get { return _screenTransitions; } }

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
        _currentResults = new List<MatchResult>();
    }

    public void AddNewMatchResult(MatchResult result)
    {
        _currentResults.Add(result);
    }

    public MatchResult[] GetMatchResults()
    { return _currentResults.ToArray(); }

    public SceneManager SceneManager
    { get { return _curSceneman; } }
    public InputManager InputManager
    { get { return _inputMan; } }
    public float DeltaTime { get; private set; }
    public float FixedDeltaTime { get; private set; }
    public int GroundLayer { get; private set; }
    public int NumOfPlayers
    { 
        get { return _numPlayers; }
        set
        {
            if (value < 0)
            {
                Debug.LogError(GlobalStrings.ERR_NUMBER_OF_PLAYERS1);
                _numPlayers = 0;
            } else if (value > 4)
            {
                Debug.LogError(GlobalStrings.ERR_NUMBER_OF_PLAYERS2);
                _numPlayers = 4;
            } else
                _numPlayers = value;

            OnNumberPlayersChanged();
        }
    }
    public int MaxControllableCharacters
    { get { return _maxControllables; } set { _maxControllables = value; } }
    public ICharacter[] PlayableCharacters
    { get { return _playableCharacters; } }

    public void SetupTransition(string nextScene)
    {
        NextScene = nextScene;
        LastSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(GlobalStrings.SCENE_LOADINGSCREEN);
        _loadingScene = true;
        StartCoroutine(loadNext());
    }

    private IEnumerator unloadPrevious()
    {
        _unloadingPrevious = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(LastSceneIndex);
        while (!_unloadingPrevious.isDone)
        {
            yield return null;
        }
    }
    private IEnumerator loadNext()
    {
        _loadingNext = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GlobalStrings.SCENE_LOADINGSCREEN);

        // Wait until the asynchronous scene fully loads
        while (!_loadingNext.isDone)
        {
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        this.tag = GlobalStrings.NAME_GAMEMANAGER;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += onSceneLoaded;

        // Pre caching strings
        _digitStrings = new string[10];
        for (int i = 0; i < 10; i++)
        {
            _digitStrings[i] = i.ToString();
        }
        _numberStrings = CreateNumberStrings(GlobalValues.STRINGS_MAX_PRECACHED_NUMBERSTRINGS, 0);


        // Setup FPS counter stuffs
        _fpsBuffer = new float[FpsMaximumSamples];

        if (this != Instance && Instance != null)
        {
            Destroy(this);
            return;
        } else
        {
            _instance = this;

            // Cache layer so not to compare string literals during updates.
            GroundLayer = LayerMask.NameToLayer(GlobalStrings.LAYER_GROUND);
        }

        // Makeing sure this item survives between scenes.
        // Be aware that this moves this object in the objects list during runtime in editor.
        // To "Don't Destroy On Load" - object.
        DontDestroyOnLoad(this);
    }

    private void onSceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        var objects = GameObject.FindGameObjectsWithTag(GlobalStrings.CHARACTER_TAG);
        List<ICharacter> characters = new List<ICharacter>();
        foreach (var character in objects)
        {
            characters.Add(character.GetComponent<ICharacter>());
        }
        _playableCharacters = characters.ToArray();

        var sceneMan = GameObject.FindGameObjectWithTag(GlobalStrings.NAME_SCENEMANAGER).GetComponent<SceneManager>();
        _curSceneman = sceneMan;
        foreach (var character in _playableCharacters)
        {
            character.Movement.AcceptInput = _curSceneman.CharactersTakeInput;
        }

        if (_loadingScene)
        {
            _loadingScene = false;
            StartCoroutine(unloadPrevious());
        }
    }

    void Start()
    {
        // First, set up for none players.
        NumOfPlayers = 0;

        // Makes sure that the GameManger COULD run without an InputManager if that¨s needed.
        if (_inputMan != null)
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
            _inputMan.Initialize(iMoveList.ToArray());

            // Setting up first Player with simple keyboard Control.
            //_inputMan.SetupKeyboardPlayerOne();
            _inputMan.SetupDefaultEmptyInputs();
        }
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
}
