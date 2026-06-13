using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform spawnPoint;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            Die();
        }
    }

    void Die()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint.position;
        Debug.Log("PLAYER DIED → RESPAWN");
    }
}
