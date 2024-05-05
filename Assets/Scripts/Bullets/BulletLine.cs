using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletLine : MonoBehaviour
{
    private LineRenderer lr = null;

    [SerializeField] private float activeTime = 0.1f; // 활성화되어 있을 시간
    private float activeTimeCurr = 0f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.endColor = Color.clear;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        activeTimeCurr -= deltaTime;
        if (activeTimeCurr <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        lr.endColor = new Color(1, 1, 1, Mathf.Lerp(0, 1, activeTimeCurr / activeTime));
    }

    public void Active(Vector2 from, Vector2 to)
    {
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);

        lr.endColor = Color.white;
        activeTimeCurr = activeTime;
        gameObject.SetActive(true);
    }
}
