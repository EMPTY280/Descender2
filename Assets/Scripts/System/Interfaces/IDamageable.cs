using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Damage
{
    public float amount;
}

interface IDamageable
{
    public void OnHit(Damage dmg);
}
