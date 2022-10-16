using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// Code sourced from https://gist.github.com/bendux/5fab0c176855d4e37bf6a38bb071b4a4
// Video at https://www.youtube.com/watch?v=K1xZ-rycYY8&ab_channel=bendux

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float momentum = 0f;

    private float jumpingPower = 16f;
    private float dashingPower = 16f;

    private bool isFacingRight = true;

    private int jumps =2;
    private int dash_fuel = 150;


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    void Start()
    {
    }

    void Update()
    {

        if (IsGrounded())
        {
            jumps = 2;
            dash_fuel = 150;
        }

        if (momentum > 0) momentum -= 1;
        else if (momentum < 0) momentum += 1;

        horizontal = Input.GetAxisRaw("Horizontal");



        //Input jump, and jumps remain
        if (Input.GetButtonDown("Jump") && jumps > 0)
        {
            --jumps;
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }



        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }


        //screw dashing while on the ground- I'm bad at c#
        if (Input.GetKey(KeyCode.LeftShift) && dash_fuel > 0 && !IsGrounded())
        {
            --dash_fuel;

            if (isFacingRight)
            {
                momentum = dashingPower;
            }
            else
            {
                momentum = -dashingPower;
            }
        }


        //I'll be honest, I don't actually know what this function does meaninfully
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y);
        }

        Flip();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed + momentum, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
