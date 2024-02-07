using System;
using UnityEngine;

public interface IJumpHit
{    
    public void OnHeadHit(Hero offender);  
    public void OnHitOthersHead(Hero victim);
}
