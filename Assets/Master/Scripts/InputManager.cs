using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This class get the inputs from controllers, but it is not Polling in update but rather gets event from Unity´s InputSystem.

//TODO:
// *Fix so that a new controller device is chosen for a new player when a button on that device is pressed instead of forcing all devices as of now.
// *Not polling if a player or AI is controlling a hero every update.
// *Unsubscribe from events for clean disposal.

/// <summary>
/// Class that control characters' movement behaviors.
/// </summary>
public class InputManager : MonoBehaviour
{
    
    [SerializeField] private InputActionAsset _actionsFile;
    [SerializeField] private PlayerInput[] _input = new PlayerInput[4];
    [SerializeField] private ICharacterMovement[] _characters = new ICharacterMovement[4];
    
    private int _numOfPlayers;
    private InputActionMap[] _actionsMaps = new InputActionMap[4];
    private List<Gamepad> _gamePads = new List<Gamepad>();
    private int _maxControllables;

    /// <summary>
    /// ActionMaps holds the different actions that a character can perform.
    /// </summary>
    public InputActionMap[] ActionMaps
        { get { return _actionsMaps; } }
    /// <summary>
    /// ActionsFile is the Input file that holds the definition of maps and their resp. actions.
    /// </summary>
    public InputActionAsset ActionsFile
        { get { return _actionsFile; } }
    /// <summary>
    /// List of currently detected gamepads (since last poll).
    /// </summary>
    public List<Gamepad> GamePads
        { get { return _gamePads; } }
    public int MaxControllableCharacter
        { get { return _maxControllables; } private set { _maxControllables = value; } } 
    /// <summary>
    /// Number of human players.
    /// </summary>
    public int NumberPlayers
        { get { return _numOfPlayers; } set { _numOfPlayers = value; } }


    /// <summary>
    /// Needs to be run before anything else on the manager to assign controllable characters and such.
    /// </summary>
    public void Initialize(int numberOfPlayers, ICharacterMovement[] moveableCharacters)
    {
        _numOfPlayers = numberOfPlayers;
        _characters = moveableCharacters;
        MaxControllableCharacter = _characters.Length;
        UpdateGamePads();
    }


    /// <summary>
    /// <br>Can be used to set up a default inputs. Keyboard only for player one, and attached gamepads to following players.</br>
    /// </summary>
    /// <param name="numberOfHumanPlayers"></param>
    public void SetupDefaultInputs()
    {
        for (int i = 0; i < _maxControllables; i++)
        {        
            if (_numOfPlayers > 0 && i < _numOfPlayers)
            {
                if (i == 0)
                {
                    SetHeroControl(0, false, new InputDevice[] { InputSystem.GetDevice<Keyboard>() });
                } else
                {
                    if (_gamePads.Count >= i)
                    {
                        SetHeroControl(i, false, new InputDevice[] { _gamePads[i - 1] });
                    } else
                        SetHeroControl(i, false, new InputDevice[] { } );
                }

                if (i < _numOfPlayers)
                    _characters[i].AiControlled = false;

            } else
            {
                SetHeroControl(i, true, new InputDevice[] { } );
            }
        }
    }

   
    /// <summary>
    /// <para>Assign the action maps to each player and their resp. input device.
    /// Provide an array of InputDevice[] for this player to control his/her character.</para>
    /// <para>Provide empty array in no controller or Ai controlled.</para>
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="devices"></param>
    public void SetHeroControl(int playerIndex, bool isAi, InputDevice[] devices)
    {
        _unSubscribeToActions(playerIndex);

        _characters[playerIndex].AiControlled = isAi;

        //Setup Controller based on if the hero should be controlled by player or Ai.
        if (!_characters[playerIndex].AiControlled)
        {
            _actionsMaps[playerIndex] = _actionsFile.FindActionMap(GlobalStrings.INPUT_HEROMOVEMENT).Clone();
        } else
            _actionsMaps[playerIndex] = _actionsFile.FindActionMap(GlobalStrings.INPUT_AI_HEROMOVEMENT).Clone();

        _input[playerIndex].currentActionMap = _actionsMaps[playerIndex];

        _actionsMaps[playerIndex].devices = devices;

        _subscribeToActions(playerIndex);
    }

    /// <summary>
    /// What it says it does.
    /// </summary>
    /// <param name="playerIndex"></param>
    private void _subscribeToActions(int playerIndex)
    {
        foreach (var action in _actionsMaps[playerIndex])
        {
            switch (action.name)
            {
                case GlobalStrings.INPUT_MOVE:
                    action.started += _characters[playerIndex].TryMove;
                    action.performed += _characters[playerIndex].TryMove;
                    action.canceled += _characters[playerIndex].TryMove;
                    break;
                case GlobalStrings.INPUT_MOVE_JUMP:
                    action.started += _characters[playerIndex].TryJump;
                    action.canceled += _characters[playerIndex].TryJump;
                    break;
            }
        }
    }
    /// <summary>
    /// Also does what it says on the label.
    /// </summary>
    /// <param name="playerIndex"></param>
    private void _unSubscribeToActions(int playerIndex)
    {
        if (_actionsMaps[playerIndex] == null) return;

        foreach (var action in _actionsMaps[playerIndex])
        {
            switch (action.name)
            {
                case GlobalStrings.INPUT_MOVE:
                    action.started -= _characters[playerIndex].TryMove;
                    action.performed -= _characters[playerIndex].TryMove;
                    action.canceled -= _characters[playerIndex].TryMove;
                    break;
                case GlobalStrings.INPUT_MOVE_JUMP:
                    action.started -= _characters[playerIndex].TryJump;
                    action.canceled -= _characters[playerIndex].TryJump;
                    break;
            }
        }
    }

    /// <summary>
    /// Find all connected gamepads and store references to them in InputManager.GamePads
    /// </summary>
    public void UpdateGamePads()
    {
        _gamePads.Clear();
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                if (!(device as  Gamepad).name.Contains(GlobalStrings.MISC_XINPUT_IGNORE)) // Debug... Pierre has duplicates of input devices on his computer sometimes.
                    _gamePads.Add((Gamepad)device);
            }
        }
    }
}
