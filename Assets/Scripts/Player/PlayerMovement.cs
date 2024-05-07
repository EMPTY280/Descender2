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

    [Header("�̵�")]
    [SerializeField] private float moveSpeedMax = 3.0f;     // �ִ� �̵� �ӵ�
    [SerializeField] private float moveSpeedAccel = 6.0f;   // ���ӷ�
    [SerializeField] private float moveFriction = 1.0f;     // ���ӷ�
    private float hSpeed = 0f;
    private bool movingRight = false;
    private bool movingLeft = false;

    [Header("����")]
    [SerializeField] private float jumpPower = 10.0f;       // ���� �Ŀ�
    [SerializeField] private int bonusJump = 1;             // �ִ� ���� ���� ���� Ƚ��
    private int bonusJumpCurr = 1;
    [SerializeField] private float coyoteTime = 0.3f;       // ���� ���� ���� �ð�
    private float coyoteTimeCurr = 0.0f;
    [SerializeField] private float gravityScale = 10f;      // �⺻ �߷�
    [SerializeField] private float jumpGravityMult = 0.5f;  // ���� ���� �� �߷� ����
    private bool isJumping = false;                         // ���� ���� ����
    private bool canBonusJump = false;                      // �߰� ������ �������� ����

    [Header("�÷��� �ϰ�")]
    [SerializeField] private LayerMask platformLayerMask = default;
    private bool isDescending = false;
    [SerializeField] private int playerLayer = 3;
    [SerializeField] private int platformLayer = 7;

    [Header("�ٴ� ����")]
    [SerializeField] private LayerMask groundLayer = default;   // �ٴ����� ������ ���̾�
    [SerializeField] private Vector2 groundBox = Vector2.zero;  // �ٴ� ���� �ڽ�
    [SerializeField] private float groundBoxYOffset = 0f;       // ���� �ڽ� ������
    [SerializeField] private float groundDistanceMax = 0.05f;
    [SerializeField] private bool isGrounded = false;           // �ٴ����� ����

    [Header("������")]
    [SerializeField] private LayerMask wallLayer = default;   // ������ ������ ���̾�
    [SerializeField] private Vector2 wallBox = Vector2.zero;  // �� ���� �ڽ�
    [SerializeField] private float wallBoxYOffset = 0f;     // �� ���� �ڽ� ������

    // �� Ʈ������
    private bool lockBonusJump = false;             // �߰� ���� �Ͻ� ����
    private Vector2 velocitySave = Vector2.zero;    // �ӵ� �Ͻ� ����

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // �ٴ� ���� ����
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Vector2 center = transform.position;
        center.y += groundBoxYOffset;
        Gizmos.DrawCube(center, groundBox);

        // �� ���� ����
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

    // �ٴ� ����
    private void UpdateGrounded(float deltaTime, Vector2 groundBoxCenter)
    {
        // �ٴڿ� ���� �ʾ��� ��
        if (!Physics2D.BoxCast(groundBoxCenter, groundBox, 0f, Vector2.down, groundDistanceMax, groundLayer))
        {
            // �ڿ���Ÿ��
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

    // �÷��� �ϰ� ����
    private void UpdateDescendPlatform(Vector2 groundBoxCenter)
    {
        if (isDescending && !Physics2D.BoxCast(groundBoxCenter, groundBox, 0f, Vector2.down, groundDistanceMax, platformLayerMask))
        {
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);
            isDescending = false;
        }
    }

    // ���� �ӵ� ����
    private void UpdateHSpeed(float deltaTime)
    {
        // �¿� �ӵ� ����
        float speedDelta = moveSpeedAccel * deltaTime;
        if (movingRight)
            hSpeed = Mathf.Min(moveSpeedMax, hSpeed + speedDelta);
        if (movingLeft)
            hSpeed = Mathf.Max(-moveSpeedMax, hSpeed - speedDelta);

        // ���� ������ �ӵ� 0���� �ʱ�ȭ.
        Vector2 center = transform.position;
        center.y += wallBoxYOffset;
        if (Physics2D.BoxCast(center, wallBox, 0, Vector2.right, hSpeed * deltaTime, wallLayer))
            hSpeed = 0;

        // ���� �ӵ� ����
        Vector2 velocity = rb.velocity;
        velocity.x = hSpeed;
        rb.velocity = velocity;

        // �̵� ���� �ƴҶ��� ������ ����
        if (!movingLeft && !movingRight)
            hSpeed = Mathf.Max(0, Mathf.Abs(hSpeed) - moveFriction * deltaTime) * Mathf.Sign(hSpeed);
    }

    // ���� �߷� ����
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
        if (b) // ���� Ű�� ������ ���� ��
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
        else // �ƴ� ��
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
