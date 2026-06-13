using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float jumpForce = 12f;

    Rigidbody2D rb;
    Animator anim;
    bool isGrounded;
    float originalXScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalXScale = transform.localScale.x;

        rb.gravityScale = 3f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");

        // Animation
        anim.SetBool("isRunning", Mathf.Abs(move) > 0.1f);

        // Flip
        if (move > 0)
            transform.localScale = new Vector3(originalXScale, transform.localScale.y, transform.localScale.z);
        else if (move < 0)
            transform.localScale = new Vector3(-originalXScale, transform.localScale.y, transform.localScale.z);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
