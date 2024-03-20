using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Bomb : MonoBehaviour
{

    //Flames

    [SerializeField]
    private GameObject explosion;

    [SerializeField]
    private float explosionForce;

    [SerializeField]
    private float radius;

    [SerializeField]
    private float delayBeforeExplosion = 4;

    private Grid grid;
    public bool IsActive
    { get; set; } = false;

    private Collider[] colliders;

    //Cacha instantiate och destroy här
    void Start()
    {
        GameObject gridObject = GameObject.FindWithTag(GlobalStrings.NAME_BOMBERGRID);
        grid = gridObject.GetComponent<Grid>();

    }
    void Update()
    {

    }

    public void SetInactive()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }

    public void SpawnBomb(Vector3 charPosition)
    {
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = charPosition;
        StartCoroutine(StartExplosion());
    }

    //kolla easytimer
    private IEnumerator StartExplosion()
    {
        yield return new WaitForSeconds(delayBeforeExplosion);

        ExplosionCheckNearby();
        SetInactive();
    }

    private void ExplosionCheckNearby()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);

        colliders = Physics.OverlapSphere(transform.position, radius);

        for (int j = 0; j < colliders.Length; j++)
        {
            if (colliders[j].CompareTag(GlobalStrings.NAME_BOMBERCRATE))
            {
                var crate = colliders[j].gameObject.GetComponent<CrateExplosion>();
                crate.Explode();
            }
            if (colliders[j].CompareTag(GlobalStrings.NAME_BOMBERMANWALL))
            {
            }

        }
    }
}
