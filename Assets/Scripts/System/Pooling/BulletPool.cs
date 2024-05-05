using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ObjectPool<BulletLine>
{
    public override bool IsAvailable(BulletLine comp)
    {
        return !comp.gameObject.activeSelf;
    }
}
