using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathWinInformer : MonoBehaviour
{
    [SerializeField] private SetPathMaker _pathMaker;

    private bool _foundWinner = false;

    void OnTriggerEnter(Collider other)
    {
        if (_foundWinner) return;
        var hero = other.gameObject.GetComponentInParent<Hero>();
        if (hero != null )
        {
            _foundWinner = true;
            _pathMaker.InformWinner(hero.Index, hero.transform);
        }
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.GetContact(0).normal == Vector3.up)
    //    {
    //        var hero = collision.gameObject.GetComponentInParent<Hero>();
    //        if (hero != null)
    //        {
    //            _pathMaker.InformWinner(hero.Index, hero.transform);
    //        }
    //    }
    //}
}
