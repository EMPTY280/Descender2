using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb = null;

    [Header("이동 속성")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float jumpPower = 10.0f;

    [SerializeField] private bool isGrounded = false;

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb))
            enabled = false;
    }

    private void Update()
    {
        // 바닥 판정 함수
    }

    public void Move(bool left)
    {
        Vector2 velocity = rb.velocity;
        velocity.x = moveSpeed;
        rb.velocity = velocity;
    }

    public void Jump()
    {
        if (!isGrounded) return;

        Vector2 velocity = rb.velocity;
        velocity.y = jumpPower;
        rb.velocity = velocity;
    }
}
