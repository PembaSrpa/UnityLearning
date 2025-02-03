using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    private Rigidbody2D rb;
    private int direction = 1; // 1 for right, -1 for left

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Set velocity in the correct direction
            rb.velocity = new Vector2(speed * direction, 0);
            rb.gravityScale = 0; // Ensures the bullet moves straight
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Destroy the bullet after its lifetime
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debugging: Show what the bullet hit
        Debug.Log("Bullet hit: " + collision.gameObject.name);

        // Prevent the bullet from destroying if it hits the player
        if (collision.CompareTag("Player")) return;

        // Destroy the bullet on collision
        Destroy(gameObject);
    }

    public void SetDirection(float playerScaleX)
    {
        direction = playerScaleX > 0 ? 1 : -1;

        // Flip the bullet sprite if moving left
        transform.localScale = new Vector3(direction, 1, 1);
    }
}
