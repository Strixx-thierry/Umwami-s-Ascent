using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(DoDash());
        }
    }

    System.Collections.IEnumerator DoDash()
    {
        canDash = false;
        isDashing = true;

        float dir = Mathf.Sign(Input.GetAxisRaw("Horizontal"));
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
