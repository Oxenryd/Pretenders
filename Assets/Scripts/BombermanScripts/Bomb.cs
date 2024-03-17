using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{

    //Flames

    public GameObject explosion;
    public float explosionForce;
    public float radius;
    public float delayBeforeExplosion = 2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        StartCoroutine(ExplodeAfterDelay(other));
    }


    private IEnumerator ExplodeAfterDelay(Collision other)
    {

        yield return new WaitForSeconds(delayBeforeExplosion);

        GameObject instantiateExplosion = Instantiate(explosion, transform.position, transform.rotation);
        KnockBack();
        Destroy(instantiateExplosion, 3);
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
