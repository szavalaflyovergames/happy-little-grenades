using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SAExamplePlayerController : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer sprite;
    public BoxCollider2D boxCol;
    public float runSpeed;
    public float jumpForce;

    public Rigidbody2D Body { get; private set; }
    public bool IsJumping   { get; private set; }
    public bool IsGrounded  { get; private set; }

    private void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        foreach(ContactPoint2D contact in col.contacts)
        {
            if(contact.normal.y == 1.0f)
            {
                IsGrounded = true;
                animator.SetBool("IsGrounded", IsGrounded);
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            if (contact.normal.y == 1.0f)
            {
                IsGrounded = false;
                animator.SetBool("IsGrounded", IsGrounded);
                break;
            }
        }
    }

    private void Update()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        
        if (horizontal > 0.0f)
        {
            Body.velocity = new Vector2(runSpeed, Body.velocity.y);
            sprite.flipX = false;
            animator.SetBool("Running", true);
        }
        else if (horizontal < 0.0f)
        {
            Body.velocity = new Vector2(-runSpeed, Body.velocity.y);
            sprite.flipX = true;
            animator.SetBool("Running", true);
        }
        else
        {
            Body.velocity = new Vector2(0.0f, Body.velocity.y);
            animator.SetBool("Running", false);
        }

        if(Input.GetButtonDown("Jump"))
            Jump();

        animator.SetFloat("VelocityY", Body.velocity.y);
    }
    
    public void Jump()
    {
        if(!IsJumping && IsGrounded)
        {
            Body.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }
    }
}
