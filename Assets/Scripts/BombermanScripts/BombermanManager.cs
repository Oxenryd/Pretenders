using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BombermanManager : MonoBehaviour
{
    [SerializeField]
    private ZoomFollowGang _cam;
    [SerializeField]
    private WinnerTextScript _winnerText;
    [SerializeField]
    private HeroMovement[] characterList = new HeroMovement[4];
    [SerializeField]
    private Vector3[] startCorners = new Vector3[] { new Vector3(2, 0, 2), new Vector3(42, 0, 2), new Vector3(42, 0, 38), new Vector3(2, 0, 38)};
    [SerializeField]
    private Grid _grid;
    private int placementToSet = 3;
    public int[] deathQueue = new int[4];
    private EasyTimer timer;
    private Transitions transitions;
    private bool fadingIn = true;
    private bool fadingOut = false;
    private bool zoomingToWinner = false;
    [SerializeField]
    private GameObject getReady;
    [SerializeField]
    private bool killAll = true;

    /// <summary>
    /// This class handles the overall behavior of the bomber man gameplay
    /// It determines the positions at which the characters start at in bomber man
    /// It handles the player death in bomber man
    /// It handles the transitions for this scene
    /// </summary>
    void Start()
    {
        transitions = GameObject.FindWithTag(GlobalStrings.TRANSITIONS_TAG).GetComponent<Transitions>();
        transitions.Value = 0;
        timer.Reset();

        RandomizeArray(startCorners);
        var gridOccupation = _grid.GetComponent<GridOccupation>();
        //This loop determines at which corner each character will stand in and depending on that corner if will change
        //The direction at which the characters face
        for (int i = 0; i < startCorners.Length; i++)
        {
            if (startCorners[i] == new Vector3(1, 0, 3) || startCorners[i] == new Vector3(41, 0, 3))
            {
                characterList[i].ForceRotation(Vector3.zero);
            }
            if (startCorners[i] == new Vector3(1, 0, 39) || startCorners[i] == new Vector3(41, 0, 39))
            {
                characterList[i].ForceRotation(new Vector3(0, -180, 0));
            }
            characterList[i].transform.position = startCorners[i];
            gridOccupation.SetOccupiedForced(i, characterList[i].transform.position);
        }
        var getReadyScript = getReady.GetComponent<GetReadyScript>();
        getReadyScript.Activate();

        if(killAll)
        {
            getReadyScript.CountdownComplete += OnKillAll;
        }

        if (GameManager.Instance.Music != null)
            GameManager.Instance.Music.Fadeout(1.5f);
    }

    /// <summary>
    /// This method is a debugging method that kills all the characters in the bomberman scene
    /// </summary>

    private void OnKillAll(object sender, System.EventArgs e)
    {
        KillAllElse();
    }

    void Awake()
    {
        timer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
    }

    /// <summary>
    /// The update method handles the timers which determines when the intro and outro transitions will start.
    /// </summary>
    void Update()
    {
        if (fadingIn)
        {
            transitions.Value = timer.Ratio;
            if (timer.Done)
            {
                fadingIn = false;
            }

        }
        if (fadingOut)
        {
            transitions.Value = 1 - timer.Ratio;
            if (timer.Done)
            {
                if (GameManager.Instance.Tournament)
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                else
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);

            }
        }
        if (zoomingToWinner && !fadingOut)
        {
            if (timer.Done)
            {
                fadingOut = true;
                timer.Time = timer.Time / 3f;
                timer.Reset();
            }
        }

    }

    /// <summary>
    /// This method adds all the players in an array which will determine the match result
    /// The array is ordered based on which character got killed first.
    /// It also handles the animations for the winning scene when there is only one player left in the scene.
    /// </summary>
    public void AddPlayerDeathToQueue(int playerId)
    {
        deathQueue[playerId] = placementToSet;
        placementToSet--;
        if(placementToSet < 1)
        {
            if (GameManager.Instance.Tournament)
            {
                MatchResult matchResult = new MatchResult(GameType.Bomberman, deathQueue);
                GameManager.Instance.AddNewMatchResult(matchResult);
            }
            var winnerIndex = -1;
            for(int i =0; i < 4; i++)
            {
                if (deathQueue[i] == 0)
                {
                    winnerIndex = i;
                    break;
                }
            }
            zoomingToWinner = true;
            timer.Time = timer.Time * 3;
            timer.Reset();
            characterList[winnerIndex].SetWinner(true);
            _cam.SetWinner(characterList[winnerIndex].transform, true);
            _winnerText.Activate();
        }
    }

    /// <summary>
    /// This method is a debugging method that kills all the characters in the bomberman scene
    /// </summary>
    void KillAllElse()
    {
        for(int i = 1; i < 4;  i++)
        {
            var hero = characterList[i].GetComponent<Hero>();
            PlayerDeath(hero);
        }
    }

    /// <summary>
    /// The playerDeath method handles the individual players death
    /// When a player dies it will fly off the scene and no longer be in the camera
    /// </summary>
    public void PlayerDeath(Hero hero)
    {
        var heroMovementScript = hero.gameObject.GetComponent<HeroMovement>();
        if (heroMovementScript.IsAlive)
        {
            heroMovementScript.IsAlive = false;
            heroMovementScript.AcceptInput = false;
            heroMovementScript.RigidBody.velocity = Vector3.zero;
            heroMovementScript.RigidBody.useGravity = false;
            //heroMovementScript.RigidBody.AddForce((-heroMovementScript.FaceDirection + Vector3.up).normalized * 30, ForceMode.Impulse);
            heroMovementScript.RigidBody.AddForce((-heroMovementScript.FaceDirection + Vector3.up).normalized * 40, ForceMode.Impulse);
            var colliders = heroMovementScript.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            AddPlayerDeathToQueue(hero.Index);
        }
    }

    /// <summary>
    /// This is a randomizing array which is used to randomize where the characters start in order to achieve more fairness in the game
    /// </summary>
    private void RandomizeArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
