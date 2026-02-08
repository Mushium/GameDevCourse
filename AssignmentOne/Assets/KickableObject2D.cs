using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KickableObject2D : MonoBehaviour
{
    [Header("Kick Force")]
    public float minForce = 7f;
    public float maxForce = 10f;

    [Header("Upward Bias")]
    [Range(0f, 1f)]
    public float upwardBias = 0.5f;

    private Rigidbody2D rb;
    private bool hasBeenKicked = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenKicked) return;

        if (other.CompareTag("Player"))
        {
            Kick(other.transform);
        }
    }

    void Kick(Transform player)
    {
        rb.gravityScale = 1f;

        hasBeenKicked = true;

        // Direction away from the player
        Vector2 direction = (transform.position - player.position).normalized;

        // Add randomness
        direction.x += Random.Range(-0.3f, 0.3f);
        direction.y = Mathf.Abs(direction.y) + upwardBias;

        direction.Normalize();

        float force = Random.Range(minForce, maxForce);
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // Optional spin
        rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
        Invoke("DestroyObjectAfterTime", 4f);
    }


    void DestroyObjectAfterTime()
    {
        Destroy(gameObject);
    }
}