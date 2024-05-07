using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb = null;

    [Header("이동")]
    [SerializeField] private float moveSpeedMax = 3.0f;     // 최대 이동 속도
    [SerializeField] private float moveSpeedAccel = 6.0f;   // 가속력
    [SerializeField] private float moveFriction = 1.0f;     // 감속력
    private float hSpeed = 0f;
    private bool movingRight = false;
    private bool movingLeft = false;

    [Header("점프")]
    [SerializeField] private float jumpPower = 10.0f;       // 점프 파워
    [SerializeField] private int bonusJump = 1;             // 최대 연속 점프 가능 횟수
    private int bonusJumpCurr = 1;
    [SerializeField] private float coyoteTime = 0.3f;       // 점프 판정 완충 시간
    private float coyoteTimeCurr = 0.0f;
    [SerializeField] private float gravityScale = 10f;      // 기본 중력
    [SerializeField] private float jumpGravityMult = 0.5f;  // 점프 지속 중 중력 감소
    private bool isJumping = false;                         // 점프 지속 여부
    private bool canBonusJump = false;                      // 추가 점프가 가능한지 여부

    [Header("플랫폼 하강")]
    [SerializeField] private LayerMask platformLayerMask = default;
    private bool isDescending = false;
    [SerializeField] private int playerLayer = 3;
    [SerializeField] private int platformLayer = 7;

    [Header("바닥 판정")]
    [SerializeField] private LayerMask groundLayer = default;   // 바닥으로 판정할 레이어
    [SerializeField] private Vector2 groundBox = Vector2.zero;  // 바닥 판정 박스
    [SerializeField] private float groundBoxYOffset = 0f;       // 판정 박스 오프셋
    [SerializeField] private float groundDistanceMax = 0.05f;
    [SerializeField] private bool isGrounded = false;           // 바닥인지 여부

    [Header("벽판정")]
    [SerializeField] private LayerMask wallLayer = default;   // 벽으로 판정할 레이어
    [SerializeField] private Vector2 wallBox = Vector2.zero;  // 벽 판정 박스
    [SerializeField] private float wallBoxYOffset = 0f;     // 벽 판정 박스 오프셋

    // 룸 트랜지션
    private bool lockBonusJump = false;             // 추가 점프 일시 정지
    private Vector2 velocitySave = Vector2.zero;    // 속도 일시 저장

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 바닥 판정 상자
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Vector2 center = transform.position;
        center.y += groundBoxYOffset;
        Gizmos.DrawCube(center, groundBox);

        // 벽 판정 상자
        Gizmos.color = new Color(1, 0, 1, 0.5f);
        center = transform.position;
        center.y += wallBoxYOffset;
        Gizmos.DrawCube(center, wallBox);
    }
#endif

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb))
            enabled = false;
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        Vector2 groundBoxCenter = transform.position;
        groundBoxCenter.y += groundBoxYOffset;

        UpdateGrounded(deltaTime, groundBoxCenter);
        UpdateDescendPlatform(groundBoxCenter);
        UpdateHSpeed(deltaTime);
        JumpGravity();
    }

 #region MOVEMENT UPDATE FUNCTIONS

    // 바닥 판정
    private void UpdateGrounded(float deltaTime, Vector2 groundBoxCenter)
    {
        // 바닥에 닿지 않았을 떄
        if (!Physics2D.BoxCast(groundBoxCenter, groundBox, 0f, Vector2.down, groundDistanceMax, groundLayer))
        {
            // 코요테타임
            if (coyoteTimeCurr < coyoteTime)
                coyoteTimeCurr += deltaTime;
            else
                isGrounded = false;
        }
        else if (rb.velocity.y <= 0)
        {
            isGrounded = true;
            if (!isDescending)
                coyoteTimeCurr = 0f;
            bonusJumpCurr = bonusJump;
        }
    }

    // 플랫폼 하강 판정
    private void UpdateDescendPlatform(Vector2 groundBoxCenter)
    {
        if (isDescending && !Physics2D.BoxCast(groundBoxCenter, groundBox, 0f, Vector2.down, groundDistanceMax, platformLayerMask))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
            isDescending = false;
        }
    }

    // 수평 속도 조정
    private void UpdateHSpeed(float deltaTime)
    {
        // 좌우 속도 조정
        float speedDelta = moveSpeedAccel * deltaTime;
        if (movingRight)
            hSpeed = Mathf.Min(moveSpeedMax, hSpeed + speedDelta);
        if (movingLeft)
            hSpeed = Mathf.Max(-moveSpeedMax, hSpeed - speedDelta);

        // 벽에 닿으면 속도 0으로 초기화.
        Vector2 center = transform.position;
        center.y += wallBoxYOffset;
        if (Physics2D.BoxCast(center, wallBox, 0, Vector2.right, hSpeed * deltaTime, wallLayer))
            hSpeed = 0;

        // 계산된 속도 적용
        Vector2 velocity = rb.velocity;
        velocity.x = hSpeed;
        rb.velocity = velocity;

        // 이동 중이 아닐때만 마찰력 적용
        if (!movingLeft && !movingRight)
            hSpeed = Mathf.Max(0, Mathf.Abs(hSpeed) - moveFriction * deltaTime) * Mathf.Sign(hSpeed);
    }

    // 점프 중력 조정
    private void JumpGravity()
    {
        if (isJumping)
        {
            rb.gravityScale = gravityScale * jumpGravityMult;
            if (rb.velocity.y < 0)
                isJumping = false;
        }
        else
            rb.gravityScale = gravityScale;
    }

#endregion

    public void SetMove(bool right, bool b)
    {
        if (right)
            movingRight = b;
        else
            movingLeft = b;
    }

    public void Jump(bool b)
    {
        if (b) // 점프 키를 누르고 있을 떄
        {
            if (!lockBonusJump && !isGrounded)
            {
                if (canBonusJump && bonusJumpCurr > 0)
                    bonusJumpCurr--;
                else
                    return;
            }

            isGrounded = false;
            isJumping = true;
            canBonusJump = false;

            Vector2 velocity = rb.velocity;
            velocity.y = jumpPower;
            rb.velocity = velocity;
        }
        else // 아닐 때
        {
            isJumping = false;
            canBonusJump = true;
        }
    }

    public void DescendPlatform()
    {
        if (!isGrounded) return;
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);
        coyoteTimeCurr = coyoteTime;
        isDescending = true;
    }

    public void SaveVelocity()
    {
        velocitySave = rb.velocity;
        rb.velocity = Vector2.zero;
    }

    public void RestoreVelocity()
    {
        rb.velocity = velocitySave;
    }

    public void ResetVelocity()
    {
        rb.velocity = Vector2.zero;
    }

    public void LockBonusJump(bool b)
    {
        lockBonusJump = b;
    }

}
