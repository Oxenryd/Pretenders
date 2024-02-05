using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderData;

//TODO:
// *Fix so that a new controller device is chosen for a new player when a button on that device is pressed instead of forcing all devices as of now.
// *Not polling if a player or AI is controlling a hero every update.
// *Unsubscribe from events for clean disposal.

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _actionsFile;
    [SerializeField] private PlayerInput[] _input = new PlayerInput[4];
    [SerializeField] private HeroMovement[] _heroes = new HeroMovement[4];
    [SerializeField] private bool[] _toggleAiControlled = new bool[4]; //Don´t set index 0 to True in inspector!!!
    [SerializeField] private string _AiToggleWarning = "Leave the first checkbox unchecked above!!";

    private InputActionMap[] _actionsMaps = new InputActionMap[4];
    public InputActionMap[] ActionMaps
        { get { return _actionsMaps; } }
    public InputActionAsset ActionsFile
        { get { return _actionsFile; } }

    // Start is called before the first frame update
    void Start()
    {
        //Setup AiControlled Heroes.
        //First Hero must be human controlled.
        for (int i = 0; i < 4; i++)
        {
            _heroes[i].Index = i;
            _heroes[i].AiControlled = _toggleAiControlled[i];
        }

        //Create InputActionMaps for players and assign devices.
        var gpads = _getGamePadDevices();
        for (int i = 0; i < 4; i++)
        {
            _enableActionMapsforPlayer(i, gpads);
            _subscribeToActions(i);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Assign the action maps to each player and their resp. input device.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="gamepads"></param>
    private void _enableActionMapsforPlayer(int playerIndex, List<Gamepad> gamepads)
    {
        //Setup Controller based on if the hero should be controlled by player or Ai.
        if (!_heroes[playerIndex].AiControlled)
            _actionsMaps[playerIndex] = _actionsFile.FindActionMap("HeroMovement").Clone();
        else
            _actionsMaps[playerIndex] = _actionsFile.FindActionMap("AiHeroMovement").Clone();

        switch (playerIndex)
        {
            case 0: //Player 1
                    //Set the actionmap to this players' controller. First player can not be Ai controlled.                
                _input[0].currentActionMap = _actionsMaps[0];
                if (gamepads.Count > 0)
                    _actionsMaps[0].devices = new InputDevice[] { InputSystem.GetDevice<Keyboard>(), gamepads[0] };
                else
                    _actionsMaps[0].devices = new InputDevice[] { InputSystem.GetDevice<Keyboard>() };
                break;

            case 1: //Player 2
                _input[1].currentActionMap = _actionsMaps[1];
                if (_heroes[playerIndex].AiControlled)
                    break;

                if (gamepads.Count > 1)
                    _actionsMaps[1].devices = new InputDevice[] { gamepads[1] };
                else
                    _actionsMaps[1].devices = new InputDevice[] { };
                break;

            case 2: //Player 3
                _input[2].currentActionMap = _actionsMaps[2];
                if (_heroes[playerIndex].AiControlled)
                    break;

                if (gamepads.Count > 2)
                    _actionsMaps[2].devices = new InputDevice[] { gamepads[2] };
                else
                    _actionsMaps[2].devices = new InputDevice[] { };
                break;

            case 3: //Player 4
                _input[3].currentActionMap = _actionsMaps[3];
                if (_heroes[playerIndex].AiControlled)
                    break;

                if (gamepads.Count > 3)
                    _actionsMaps[3].devices = new InputDevice[] { gamepads[3] };
                else
                    _actionsMaps[3].devices = new InputDevice[] { };
                break;
        }
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
                case "Move":
                    action.started += _heroes[playerIndex].TryMove;
                    action.performed += _heroes[playerIndex].TryMove;
                    action.canceled += _heroes[playerIndex].TryMove;
                    break;
                case "Jump":
                    action.started += _heroes[playerIndex].TryJump;
                    action.canceled += _heroes[playerIndex].TryJump;
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
        foreach (var action in _actionsMaps[playerIndex])
        {
            switch (action.name)
            {
                case "Move":
                    action.started -= _heroes[playerIndex].TryMove;
                    action.performed -= _heroes[playerIndex].TryMove;
                    action.canceled -= _heroes[playerIndex].TryMove;
                    break;
                case "Jump":
                    action.started -= _heroes[playerIndex].TryJump;
                    action.canceled -= _heroes[playerIndex].TryJump;
                    break;
            }
        }
    }

    /// <summary>
    /// Find all connected gamepads.
    /// </summary>
    private List<Gamepad> _getGamePadDevices()
    {
        List<Gamepad> gamepads = new List<Gamepad>();
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                if (!(device as  Gamepad).name.Contains("XInput")) //Debug... Pierre has duplicates of input devices on his computer sometimes.
                    gamepads.Add((Gamepad)device);
            }
        }

        return gamepads;
    }
}
