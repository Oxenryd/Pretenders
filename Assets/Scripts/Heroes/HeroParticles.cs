using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroParticles : MonoBehaviour
{
    private bool wasFalling = false;
    private bool wasJumping = false;
    private float timeMoving = 0;
    private float timePerStep = 1F;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 GetStepDustPosition(HeroMovement characterMovement)
    {
        Vector3 stepPosition = gameObject.transform.position + characterMovement.CurrentDirection * -1;
        return stepPosition;
    }

    void EmitDustParticle(HeroMovement characterMovement)
    {
        ParticleSystem dustParticles = GameObject.Find("Dust_Particles").GetComponent<ParticleSystem>();

        Vector3 stepPos = GetStepDustPosition(characterMovement);

        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = Color.white;
        emitParams.startSize = 0.12f;
        //emitParams.position = gameObject.transform.position + new Vector3(0, 0, -0.2f);
        emitParams.position = stepPos;
        emitParams.applyShapeToPosition = true;

        dustParticles.Emit(emitParams, 5);
    }

    void EmitLandParticle()
    {
        ParticleSystem landParticles = GameObject.Find("Land_Particles").GetComponent<ParticleSystem>();

        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = Color.green;
        //emitParams.startSize = 0.25f;
        emitParams.position = gameObject.transform.position;
        emitParams.applyShapeToPosition = true;

        landParticles.Emit(emitParams, 10);
    }

    void EmitJumpParticle()
    {
        ParticleSystem jumpParticles = GameObject.Find("Jump_Particles").GetComponent<ParticleSystem>();

        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = Color.red;
        //emitParams.startSize = 0.25f;
        emitParams.position = gameObject.transform.position;
        emitParams.applyShapeToPosition = true;

        jumpParticles.Emit(emitParams, 10);
    }

    // Update is called once per frame
    void Update()
    {
        HeroMovement characterMovement = gameObject.GetComponent<HeroMovement>();

        if (characterMovement.IsFalling) { wasFalling = true; }

        if (characterMovement.IsMoving && !wasFalling)
        {
            EmitDustParticle(characterMovement);
        }

        if (wasFalling && characterMovement.IsGrounded)
        {
            EmitLandParticle();
            wasFalling = false;
        }

        if (!wasJumping && characterMovement.IsJumping)
        {
            EmitJumpParticle();
            wasJumping = true;
        }

        if (characterMovement.IsGrounded)
        {
            wasJumping = false;
        }
    }
}
