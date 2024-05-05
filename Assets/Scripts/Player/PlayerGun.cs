using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private Transform firePos = null;
    [SerializeField] private LayerMask hitLayers = default;
    [SerializeField] private float rayRange = 55f;

    [Header("Status")]
    [SerializeField] private float damage = 1.0f;       // ���ط�
    [SerializeField] private int clipSize = 6;          // źâ ũ��
      [SerializeField] private int loadedRounds = 6;    // ���� ������ źȯ ��
    [SerializeField] private int shots = 1;             // �� ���� �߻��ϴ� źȯ ��
    [SerializeField] private float fireDelay = 0.2f;    // �߻� �� ������ (��)
      private float fireDelayCurr = 0f;
    [SerializeField] private float reloadTime = 1.0f;   // ���� �ð�
      private float reloadTimeCurr = 0f;
    [SerializeField] private float spread = 0f;         // ��ź��

    // DEBUG
    [Space][Header("DEBUG")]
    [SerializeField] private BulletPool bulletLinePool = null;
    [SerializeField] private ParticlePool bulletHitPool = null;
    // /DEBUG

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // �߻� ������
        if (fireDelayCurr > 0f)
            fireDelayCurr -= deltaTime;

        // ������
        if (reloadTimeCurr > 0f)
        {
            reloadTimeCurr -= deltaTime;
            if (reloadTimeCurr <= 0f)
                loadedRounds = clipSize;
        }
    }

    public void Fire()
    {
        Fire((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
    }

    public void Fire(Vector2 dir)
    {
        if (IsFireDelay()) return;      // �߻� �������� ��
        if (IsReloading()) return;      // ���� ���� ��
        if (loadedRounds < 1) return;   // ��ź�� ���� ��

        // �ѱ� ��ġ
        Vector2 gunPos = firePos.position;

        for (int i = 0; i < shots; i++)
        {
            // ��ź�� ����, ù źȯ�� ����.
            float accuracyDev = Random.Range(-spread * 0.5f, spread * 0.5f);
            Vector2 trueDir = Quaternion.AngleAxis(accuracyDev, Vector3.forward) * dir;

            // ���� �߻� �� ����
            RaycastHit2D hit = Physics2D.Raycast(gunPos, trueDir, rayRange, hitLayers);
            if (hit && hit.collider.TryGetComponent(out IDamageable target))
            {
                Damage d;
                d.amount = damage;
                target.OnHit(d);
            }

            // �Ѿ� ������Ʈ Ȱ��ȭ
            Vector2 bulletHitPos;
            if (hit)
                bulletHitPos = hit.point;
            else
                bulletHitPos = gunPos + trueDir * rayRange;
            bulletLinePool.Get().Active(transform.position, bulletHitPos);

            // �Ѿ� �ǰ� ��ƼŬ Ȱ��ȭ
            if (hit)
            {
                ParticleSystem ps = bulletHitPool.Get();
                ps.transform.position = hit.point;
                ps.transform.rotation = Quaternion.FromToRotation(Vector2.right, -trueDir);
                ps.Play();
            }
        }

        // ��ź -1
        loadedRounds--;

        // �߻� ������ ���� or ��ź �Ҹ� �� �ڵ� ����
        if (loadedRounds != 0)
            fireDelayCurr = fireDelay;
        else
            Reload();
    }

    public void Reload()
    {
        if (IsReloading()) return;
        reloadTimeCurr = reloadTime;
        if (reloadTime <= 0f)
            loadedRounds = clipSize;
    }

    public bool IsReloading()
    {
        return (reloadTimeCurr > 0.0f);
    }

    public bool IsFireDelay()
    {
        return (fireDelayCurr > 0.0f);
    }
}
