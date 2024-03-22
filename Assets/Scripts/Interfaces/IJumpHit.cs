using System;
using UnityEngine;

public interface IJumpHit
{    
    public void OnHeadHit(HeroMovement offender);  
    public void OnHitOthersHead(HeroMovement victim);
}
