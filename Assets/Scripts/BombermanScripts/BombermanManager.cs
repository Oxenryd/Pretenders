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
    void Start()
    {
        transitions = GameObject.FindWithTag(GlobalStrings.TRANSITIONS_TAG).GetComponent<Transitions>();
        transitions.Value = 0;
        timer.Reset();

        RandomizeArray(startCorners);
        for (int i = 0; i < startCorners.Length; i++)
        {
            if (startCorners[i] == new Vector3(2, 0, 2) || startCorners[i] == new Vector3(42, 0, 2))
            {
                characterList[i].ForceRotation(Vector3.zero);
                characterList[i].transform.position = GridCellMiddlePoint.Get(_grid, startCorners[i]);
            }
            if (startCorners[i] == new Vector3(2, 0, 38) || startCorners[i] == new Vector3(42, 0, 38))
            {
                characterList[i].ForceRotation(new Vector3(0, -180, 0));
                characterList[i].transform.position = GridCellMiddlePoint.Get(_grid, startCorners[i]);
            }
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

    private void OnKillAll(object sender, System.EventArgs e)
    {
        KillAllElse();
    }

    void Awake()
    {
        timer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
    }
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

    void KillAllElse()
    {
        for(int i = 1; i < 4;  i++)
        {
            var hero = characterList[i].GetComponent<Hero>();
            PlayerDeath(hero);
        }
    }
    public void PlayerDeath(Hero hero)
    {
        var heroMovementScript = hero.gameObject.GetComponent<HeroMovement>();
        if (heroMovementScript.IsAlive)
        {
            heroMovementScript.IsAlive = false;
            heroMovementScript.AcceptInput = false;
            heroMovementScript.RigidBody.velocity = Vector3.zero;
            heroMovementScript.RigidBody.AddForce((-heroMovementScript.FaceDirection + Vector3.up).normalized * 30, ForceMode.Impulse);
            AddPlayerDeathToQueue(hero.Index);
        }
    }
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
