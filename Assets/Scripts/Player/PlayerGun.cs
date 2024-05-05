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
    [SerializeField] private float damage = 1.0f;       // 피해량
    [SerializeField] private int clipSize = 6;          // 탄창 크기
      [SerializeField] private int loadedRounds = 6;    // 현재 장전된 탄환 수
    [SerializeField] private int shots = 1;             // 한 번에 발사하는 탄환 수
    [SerializeField] private float fireDelay = 0.2f;    // 발사 간 딜레이 (초)
      private float fireDelayCurr = 0f;
    [SerializeField] private float reloadTime = 1.0f;   // 장전 시간
      private float reloadTimeCurr = 0f;
    [SerializeField] private float spread = 0f;         // 산탄도

    // DEBUG
    [Space][Header("DEBUG")]
    [SerializeField] private BulletPool bulletLinePool = null;
    [SerializeField] private ParticlePool bulletHitPool = null;
    // /DEBUG

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // 발사 딜레이
        if (fireDelayCurr > 0f)
            fireDelayCurr -= deltaTime;

        // 재장전
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
        if (IsFireDelay()) return;      // 발사 딜레이일 때
        if (IsReloading()) return;      // 장전 중일 때
        if (loadedRounds < 1) return;   // 잔탄이 없을 떄

        // 총구 위치
        Vector2 gunPos = firePos.position;

        for (int i = 0; i < shots; i++)
        {
            // 산탄도 적용, 첫 탄환은 제외.
            float accuracyDev = Random.Range(-spread * 0.5f, spread * 0.5f);
            Vector2 trueDir = Quaternion.AngleAxis(accuracyDev, Vector3.forward) * dir;

            // 레이 발사 및 판정
            RaycastHit2D hit = Physics2D.Raycast(gunPos, trueDir, rayRange, hitLayers);
            if (hit && hit.collider.TryGetComponent(out IDamageable target))
            {
                Damage d;
                d.amount = damage;
                target.OnHit(d);
            }

            // 총알 오브젝트 활성화
            Vector2 bulletHitPos;
            if (hit)
                bulletHitPos = hit.point;
            else
                bulletHitPos = gunPos + trueDir * rayRange;
            bulletLinePool.Get().Active(transform.position, bulletHitPos);

            // 총알 피격 파티클 활성화
            if (hit)
            {
                ParticleSystem ps = bulletHitPool.Get();
                ps.transform.position = hit.point;
                ps.transform.rotation = Quaternion.FromToRotation(Vector2.right, -trueDir);
                ps.Play();
            }
        }

        // 잔탄 -1
        loadedRounds--;

        // 발사 딜레이 적용 or 전탄 소모 시 자동 장전
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
