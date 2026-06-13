using UnityEngine;
using UnityEngine.InputSystem;

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

    // Reads A/D + Left/Right arrows as a -1..1 axis via the new Input System.
    float ReadHorizontal()
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;

        float move = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += 1f;
        return move;
    }

    void FixedUpdate()
    {
        float move = ReadHorizontal();
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    void Update()
    {
        float move = ReadHorizontal();

        // Animation
        anim.SetBool("isRunning", Mathf.Abs(move) > 0.1f);

        // Flip
        if (move > 0)
            transform.localScale = new Vector3(originalXScale, transform.localScale.y, transform.localScale.z);
        else if (move < 0)
            transform.localScale = new Vector3(-originalXScale, transform.localScale.y, transform.localScale.z);

        // Jump
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame && isGrounded)
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
