using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{

    void Update()
    { 
        Vector3 pos1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 pos2 = transform.position;
        Vector3 pos = pos1 - pos2;
        float tan = Mathf.Atan2(pos.y, pos.x);
        transform.rotation = Quaternion.Euler(0, 0, tan * Mathf.Rad2Deg);
    }
}
