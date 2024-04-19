using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BombermanManager : MonoBehaviour
{

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
    [SerializeField]
    private GameObject getReady;
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
        getReady.GetComponent<GetReadyScript>().Activate();
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
                fadingOut = false;
                GameManager.Instance.TransitToNextScene("ResultScreen");
            }
        }
    }

    public void AddPlayerDeathToQueue(int playerId)
    {
        deathQueue[playerId] = placementToSet;
        placementToSet--;
        if(placementToSet < 1)
        {
            MatchResult matchResult = new MatchResult(GameType.Bomberman, deathQueue);
            if (!GameManager.Instance.Tournament)
            {
                GameManager.Instance.StartNewTournament();
            }
            GameManager.Instance.AddNewMatchResult(matchResult);
            fadingOut = true;
            timer.Reset();
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
