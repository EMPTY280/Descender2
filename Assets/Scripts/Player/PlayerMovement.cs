using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    [Header("바닥 판정")]
    [SerializeField] private LayerMask groundLayer = default;   // 바닥으로 판정할 레이어
    [SerializeField] private Vector2 groundBox = Vector2.zero;  // 바닥 판정 박스
    [SerializeField] private float groundBoxYOffset = 0f;       // 판정 박스 오프셋
    [SerializeField] private bool isGrounded = false;           // 바닥인지 여부

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 바닥 판정 상자
        Gizmos.color = Color.cyan;
        Vector2 center = transform.position;
        center.y += groundBoxYOffset;
        Gizmos.DrawCube(center, groundBox);
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
        // 바닥 판정
        UpdateGrounded();

        // 좌우 속도 조정
        float speedDelta = moveSpeedAccel * Time.deltaTime;
        if (movingRight)
            hSpeed = Mathf.Min(moveSpeedMax, hSpeed + speedDelta);
        if (movingLeft)
            hSpeed = Mathf.Max(-moveSpeedMax, hSpeed - speedDelta);

        Vector2 velocity = rb.velocity;
        velocity.x = hSpeed;
        rb.velocity = velocity;

        // 이동 중이 아닐때만 마찰력 적용
        if (!movingLeft && !movingRight)
            hSpeed = Mathf.Max(0, Mathf.Abs(hSpeed) - moveFriction * Time.deltaTime) * Mathf.Sign(hSpeed);

        // 점프 중력 조정
        if (isJumping)
        {
            rb.gravityScale = gravityScale * jumpGravityMult;
            if (rb.velocity.y < 0)
                isJumping = false;
        }
        else
            rb.gravityScale = gravityScale;
    }

    private void UpdateGrounded()
    {
        Vector2 center = transform.position;
        center.y += groundBoxYOffset;

        // 바닥에 닿지 않았을 떄
        if (!Physics2D.OverlapBox(center, groundBox, 0, groundLayer))
        {
            // 코요테타임
            if (coyoteTimeCurr < coyoteTime)
                coyoteTimeCurr += Time.deltaTime;
            else
                isGrounded = false;
        }
        else
        {
            isGrounded = true;
            coyoteTimeCurr = 0f;
            bonusJumpCurr = bonusJump;
        }
    }

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
            if (!isGrounded)
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

    public void ResetVSpeed()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
    }
}
