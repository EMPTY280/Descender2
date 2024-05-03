using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    [Header("�ٴ� ����")]
    [SerializeField] private LayerMask groundLayer = default;   // �ٴ����� ������ ���̾�
    [SerializeField] private Vector2 groundBox = Vector2.zero;  // �ٴ� ���� �ڽ�
    [SerializeField] private float groundBoxYOffset = 0f;       // ���� �ڽ� ������
    [SerializeField] private bool isGrounded = false;           // �ٴ����� ����

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // �ٴ� ���� ����
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
        // �ٴ� ����
        UpdateGrounded();

        // �¿� �ӵ� ����
        float speedDelta = moveSpeedAccel * Time.deltaTime;
        if (movingRight)
            hSpeed = Mathf.Min(moveSpeedMax, hSpeed + speedDelta);
        if (movingLeft)
            hSpeed = Mathf.Max(-moveSpeedMax, hSpeed - speedDelta);

        Vector2 velocity = rb.velocity;
        velocity.x = hSpeed;
        rb.velocity = velocity;

        // �̵� ���� �ƴҶ��� ������ ����
        if (!movingLeft && !movingRight)
            hSpeed = Mathf.Max(0, Mathf.Abs(hSpeed) - moveFriction * Time.deltaTime) * Mathf.Sign(hSpeed);

        // ���� �߷� ����
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

        // �ٴڿ� ���� �ʾ��� ��
        if (!Physics2D.OverlapBox(center, groundBox, 0, groundLayer))
        {
            // �ڿ���Ÿ��
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
        if (b) // ���� Ű�� ������ ���� ��
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
        else // �ƴ� ��
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
