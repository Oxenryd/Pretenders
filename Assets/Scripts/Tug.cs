using System;
using TMPro;
using UnityEngine;

public class Tug : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private TextMeshPro _text;
    private Hero _heroOne;
    private Hero _heroTwo;
    private float _counter;
    
    private float _value = 50;

    public int Winner { get; private set; } = 0;
    public bool Active = true;
    public void StartTug(Hero participantOne, Hero participantTwo)
    {
        gameObject.SetActive(true);
        _counter = 0;
        Winner = 0;
        Active = true;
        _heroOne = participantOne;
        _heroTwo = participantTwo;
    }

    public event EventHandler<Hero> GotWinner;
    protected void OnGotWinner(Hero winner)
    { GotWinner?.Invoke(this, winner); }

    public void DecreaseValue(float amount)
    { _value -= amount; }
    public void IncreaseValue(float amount)
    { _value += amount; }

    void Update()
    {
       // if (!Active) return;

        _counter += GameManager.Instance.DeltaTime;
        _text.rectTransform.localScale = Vector3.one * (0.15f * MathF.Cos(2 * MathF.PI + _counter * 20) + 1f);

        transform.rotation = _camera.transform.rotation;
        if (_value <= 0)
            Winner = 1;
        else if (_value >= 1)
            Winner = 2;

        //if (Winner != 0)
        //{
        //    if (Winner == 1)
        //        OnGotWinner(_heroOne);
        //    else
        //        OnGotWinner(_heroTwo);
        //    Active = false;
        //    gameObject.SetActive(false);
        //}
    }
}
