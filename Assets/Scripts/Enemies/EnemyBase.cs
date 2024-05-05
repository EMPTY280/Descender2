using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IResettable, IDamageable
{
    [SerializeField] private float hpMax = 1;
    [SerializeField] private float hpCurr = 1;
    [SerializeField] private bool invulnerable = false;

    private Vector3 spawnPos;

    private float hitBlink = 0.15f;
    private float hitBlinkCurr = 0.0f;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private Color originColor = Color.white;

    private void Awake()
    {
        spawnPos = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (hitBlinkCurr > 0)
        {
            hitBlinkCurr -= Time.deltaTime;
            spriteRenderer.color = Color.Lerp(originColor, hitColor, hitBlinkCurr / hitBlink);
        }
    }

    public void OnHit(Damage dmg)
    {
        if (invulnerable) return;

        hpCurr -= dmg.amount;
        hitBlinkCurr = hitBlink;

        if (hpCurr <= 0f)
            Kill();
    }

    public void Kill()
    {
        gameObject.SetActive(false);
    }

    public void ResetCondition()
    {
        hpCurr = hpMax;
        transform.localPosition = spawnPos;
        gameObject.SetActive(true);
    }
}
