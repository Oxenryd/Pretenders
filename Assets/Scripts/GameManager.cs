using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

/// <summary>
/// Class that holds information about the game state and other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const string CHARACTER_TAG = "Character";

    // Don't forget to alter these in Inspector.  
    [SerializeField] private int _maxControllables = 4;
    [SerializeField] private InputManager _inputMan;

    private ICharacter[] _playableCharacters;

    private int _numPlayers = 1;

    private static GameManager _instance;
    /// <summary>
    /// The Singleton instance of our GameManager.
    /// <br>Singleton pattern in unity gameobject is special...</br>
    /// </summary>
    public static GameManager Instance
        { get { return _instance; } }
    
    

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

    // Start is called before the first frame update
    void Awake()
    {
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

    void Start()
    {
        // First, set up for one player.
        NumOfPlayers = 1;

        // Makes sure that the GameManger COULD run without an InputManager if that¨s needed.
        if (_inputMan != null)
        {
            // Find characters in the scene then assign them to and init the inputmanager.
            // Also populate this managers' info about characters
            var sceneChars = GameObject.FindGameObjectsWithTag(CHARACTER_TAG);
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
            var iMoveList = new List<ICharacterMovement>();
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
            _inputMan.SetupDefaultInput();
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

    /// <summary>
    /// Set the number of human players currently in the game.
    /// </summary>
    /// <param name="numOfHumanPlayers"></param>
    public void SetNumberPlayers(int numOfHumanPlayers)
    {
        NumOfPlayers = numOfHumanPlayers;
        OnNumberPlayersChanged();
    }
}
