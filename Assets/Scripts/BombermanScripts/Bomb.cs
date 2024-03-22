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
    private LayerMask levelMask;

    [SerializeField]
    private float delayBeforeExplosion = 1;

    public bool IsActive
    { get; set; } = false;

    private Collider[] colliders;


    //Cacha instantiate och destroy här
    void Start()
    {


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
        Instantiate(explosion, transform.position, explosion.transform.rotation);
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = charPosition;
        StartCoroutine(CreateExplosions());
    }
    //kolla easytimer
    private IEnumerator CreateExplosions()
    {
        yield return new WaitForSeconds(delayBeforeExplosion);


        Vector3[] directions = { Vector3.back, Vector3.forward, Vector3.left, Vector3.right };

        for (int i = 0; i < directions.Length; i++)
        {
            StartCoroutine(ExplosionCheckNearby(directions[i]));
        }
    }

    private IEnumerator ExplosionCheckNearby(Vector3 direction)
    {
        for (int i = 1; i < 10; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(gameObject.transform.position, direction, out hit, levelMask,i))
            {

                if (hit.collider.CompareTag(GlobalStrings.NAME_BOMBERTREE))
                {
                    break;
                }
                else if(hit.collider.CompareTag(GlobalStrings.NAME_BOMBERMANWALL))
                {
                    break;
                }

                else if (hit.collider != null)
                {
                    if (hit.collider.gameObject.TryGetComponent<CrateExplosion>(out var crate))
                    {
                        crate.Explode();
                        break;
                    }


                }

            }
            else
            {

                Instantiate(explosion, transform.position + (i * direction), explosion.transform.rotation);

            }
            yield return new WaitForSeconds(0.15f);


        }


    }

}
