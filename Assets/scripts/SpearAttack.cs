using UnityEngine;

public class SpearAttack : MonoBehaviour
{
    public float pogoForce = 10f;
    public LayerMask pogoLayer;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TryPogo();
        }
    }

    void TryPogo()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            1.2f,
            pogoLayer
        );

        if (hit.collider != null && rb.linearVelocity.y <= 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoForce);
        }
    }
}
