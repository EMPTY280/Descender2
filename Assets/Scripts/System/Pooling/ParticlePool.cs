using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : ObjectPool<ParticleSystem>
{
    public override bool IsAvailable(ParticleSystem comp)
    {
        return !comp.isPlaying;
    }
}
