using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public Transform respawnPoint;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            Die();
        }
    }

    void Die()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = respawnPoint.position;
        Debug.Log("PLAYER DIED");
    }
}
