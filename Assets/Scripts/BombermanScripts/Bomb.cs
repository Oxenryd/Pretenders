using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{

    //Flames

    public GameObject explosion;
    public float explosionForce;
    public float radius;
    public float delayBeforeExplosion = 4;
    void Start()
    {
        ExplodeAfterSomeTime();
    }

    void Update()
    {
        
    }

    private void ExplodeAfterSomeTime()
    {
        StartCoroutine(ExplodeAfterDelay());
    }


    private IEnumerator ExplodeAfterDelay()
    {

        yield return new WaitForSeconds(delayBeforeExplosion);

        Instantiate(explosion, transform.position, transform.rotation);
        KnockBack();
        Destroy(gameObject);
    }

    private void KnockBack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider closeCollider in colliders)
        {
            if (closeCollider.CompareTag(GlobalStrings.NAME_BOMBERCRATE))
            {
                Destroy(closeCollider.gameObject);
            }

        }
    }
}
