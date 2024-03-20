using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateExplosion : MonoBehaviour
{
    [SerializeField]
    private GameObject powerUpObject;

    private Bomb powerUp;
    void Start()
    {
    }

    private void SetInactive()
    {
        gameObject.SetActive(false);
    }

    public void Explode()
    {
        //Instantiate(powerUpObject, transform.position, transform.rotation);
        SetInactive();
    }

    void Update()
    {
        
    }
}
