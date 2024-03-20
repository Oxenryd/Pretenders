using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateExplosion : MonoBehaviour
{
    public bool isDestroyed = false;
    GameObject powerUpObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePowerUpAfterExplosion()
    {
        if(isDestroyed)
        {
            GameObject instantiatePowerUp = Instantiate(powerUpObject, transform.position, transform.rotation);

        }
    }
}
