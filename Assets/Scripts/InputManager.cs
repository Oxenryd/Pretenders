using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Class that control characters' movement behaviors.
/// </summary>
public class InputManager : MonoBehaviour
{
    public const string IGNOREINPUT_MSG = "Ignored ";
    public const string PLAYERJOIN = "Player Joined! ID: ";
    public const string DEVICECONNECTED = " Device Assigned: ";

    [SerializeField] private InputActionAsset _actionsFile;
    [SerializeField] private PlayerInput[] _input = new PlayerInput[4];
    [SerializeField] private ICharacterMovement[] _characters = new ICharacterMovement[4];
    
    private InputActionMap[] _actionsMaps = new InputActionMap[4];
    private List<InputDevice> _inputDevices = new List<InputDevice>();
    private Dictionary<InputDevice, int> _deviceCharCouple = new Dictionary<InputDevice, int>();

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
    /// List of currently detected devices (since last UpdateDevices() ).
    /// </summary>
    public List<InputDevice> InputDevices
        { get { return _inputDevices; } }
    public int MaxControllableCharacters
        { get { return GameManager.Instance.MaxControllableCharacters; } } 


    /// <summary>
    /// Needs to be run before anything else on the manager to assign controllable characters and such.
    /// </summary>
    public void Initialize(ICharacterMovement[] moveableCharacters)
    {
        _deviceCharCouple.Clear();
        _characters = moveableCharacters;
        UpdateDevices();
    }


    /// <summary>
    /// <br>Can be used to set up a default inputs. Keyboard only for player one, and attached gamepads to following players.</br>
    /// </summary>
    /// <param name="numberOfHumanPlayers"></param>
    public void SetupDefaultInput()
    {
        _deviceCharCouple.Clear();
        var players = GameManager.Instance.NumOfPlayers;
        for (int i = 0; i < MaxControllableCharacters; i++)
        {        
            if (players > 0 && i < players)
            {
                if (i == 0)
                {
                    var keyboard = InputSystem.GetDevice<Keyboard>();
                    SetHeroControl(0, false, new InputDevice[] { keyboard });
                    _deviceCharCouple.Add(keyboard, 0);
                }

                if (i < players)
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
    /// Find all connected gamepads and others and store references to them in InputManager.InputDevices
    /// </summary>
    public void UpdateDevices()
    {
        _inputDevices.Clear();
        var ignoreList = GlobalStrings.MISC_INPUT_IGNORE.Split(';');
        foreach (var device in InputSystem.devices)
        {
            if (device is Mouse || device is Keyboard) // Skip mouse and keyboard
                continue;

            // Check ignores
            bool ignored = false;
            foreach (var ignore in ignoreList)
                if (device.name == ignore)
                {
                    Debug.Log(IGNOREINPUT_MSG + ignore);
                    ignored = true;
                }
            if (ignored)
                continue;

            _inputDevices.Add(device);
        }
    }

    void Update()
    {
        // Probe devices and if a new device is being activated,
        // assign that device to a character as a new player.
        for (int i = 0; i < _inputDevices.Count; i++)
        {
            if (GameManager.Instance.NumOfPlayers >= GameManager.Instance.MaxControllableCharacters)
                break;

            if (!_inputDevices[i].enabled || _deviceCharCouple.ContainsKey(_inputDevices[i]))
                continue;

            var controls = _inputDevices[i].allControls;
            for (int j = 0; j < controls.Count; j++)
            {
                if (controls[j] is ButtonControl && (controls[j] as ButtonControl).wasPressedThisFrame)
                {
                    int newIndex = GameManager.Instance.NumOfPlayers++;
                    _deviceCharCouple.Add(_inputDevices[i], newIndex);
                    SetHeroControl(newIndex, false, new InputDevice[] { _inputDevices[i] });
                    Debug.Log(PLAYERJOIN + newIndex + DEVICECONNECTED + _inputDevices[i].name);
                    _characters[newIndex].Halt();
                }
            }    
        }
    }
}
