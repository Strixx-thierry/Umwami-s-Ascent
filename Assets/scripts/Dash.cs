using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : MonoBehaviour
{
    public float dashForce = 12f;
    public float dashTime = 0.15f;

    Rigidbody2D rb;
    bool canDash = true;
    bool isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.leftShiftKey.wasPressedThisFrame && canDash)
        {
            StartCoroutine(DoDash());
        }
    }

    float ReadHorizontal()
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;

        float move = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += 1f;
        return move;
    }

    System.Collections.IEnumerator DoDash()
    {
        canDash = false;
        isDashing = true;

        float dir = Mathf.Sign(ReadHorizontal());
        if (dir == 0) dir = transform.localScale.x;

        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(dir * dashForce, 0);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = 4;
        isDashing = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            canDash = true;
    }
}
