using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Class that holds information about the game state and other managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Don't forget to alter these in Inspector.
    [SerializeField] private int _numPlayers = 1;
    [SerializeField] private int _maxHeroes = 4;
    [SerializeField] private InputManager _inputMan;

    private static GameManager _instance;
    /// <summary>
    /// The Singleton instance of our GameManager.
    /// <br>Singleton pattern in unity gameobject is special...</br>
    /// </summary>
    public static GameManager Instance
        { get { return _instance; } }
    
    private Hero[] _heroes;

    // Events
    public EventHandler<float> EarlyUpdate;
    public EventHandler<float> EarlyFixedUpdate;
    public EventHandler<int> NumberPlayersChanged;
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
        if (NumberPlayersChanged == null) return;
        NumberPlayersChanged.Invoke(this, NumberHumanPlayers);
    }

    public InputManager InputManager
        { get { return _inputMan; } }
    public float DeltaTime { get; private set; }
    public float FixedDeltaTime { get; private set; }
    public int GroundLayer { get; private set; }
    public int NumberHumanPlayers
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
    public int MaxHeroes { get; private set; }

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
        }

        //Make sure this item survives between scenes.
        // Be aware that this moves this object in the objects list during runtime in editor.
        // To "Don't Destroy On Load" - object.
        DontDestroyOnLoad(this);
        
        // Cache layer so not to compare string literals during updates.
        GroundLayer = LayerMask.NameToLayer(GlobalStrings.LAYER_GROUND);
    }

    private void Start()
    {
        // This must match the number of Heroes found in the scene under "HeroContainer".
        MaxHeroes = _maxHeroes;

        // First, set up for one player.
        NumberHumanPlayers = 1;

        //Makes sure that the GameManger COULD run without an InputManager if that¨s needed.
        if (_inputMan != null)
        {
            // Find Heroes in the container then assign them to and init the inputmanager.
            // Also populate this managers' info about Heroes.
            // (HeroContainer MUST have at least as many Heroes as _maxHeroes!!)
            var hContainer = GameObject.FindGameObjectWithTag(GlobalStrings.CONT_HEROCONTAINER);
            var heroList = hContainer.GetComponentsInChildren<Hero>();
            var iMoveList = new List<ICharacterMovement>();
            _heroes = new Hero[_maxHeroes];
            for (int i = 0; i < _maxHeroes; i++)
            {
                _heroes[i] = heroList[i];
                iMoveList.Add(_heroes[i].GetComponent<ICharacterMovement>());
            }
            _inputMan.Initialize(_numPlayers, iMoveList.ToArray());


            // Start the game by setting up One Player with keyboard Control first.
            _inputMan.SetupDefaultInputs();
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
        NumberHumanPlayers = numOfHumanPlayers;
        OnNumberPlayersChanged();
    }
}
