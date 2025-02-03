using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;
    private CapsuleCollider2D capsuleCollider;

    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    private bool isGrounded;
    private bool isOnWall;
    private bool isSliding;
    private bool isHurt;
    private bool isDead;
    private float wallJumpCooldown;
    private int health = 3;

    [SerializeField] private GameObject sword;
    [SerializeField] private BoxCollider2D swordCollider;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        if (isDead) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        isOnWall = onWall();

        HandleMovement(horizontalInput);
        HandleJumping();
        HandleSliding();
        HandleAttacks();
        UpdateAnimations(isMoving);
    }

    private void HandleMovement(float horizontalInput)
    {
        if (wallJumpCooldown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            if (isOnWall && !isGrounded)
            {
                body.gravityScale = 0;
                body.velocity = Vector2.zero;
            }
            else
            {
                body.gravityScale = 7;
            }
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }

        if (horizontalInput > 0)
            transform.localScale = Vector3.one;
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isOnWall)
            {
                WallJump();
            }
        }
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    private void WallJump()
    {
        if (wallJumpCooldown > 0.2f)
        {
            float jumpDirection = -Mathf.Sign(transform.localScale.x);
            body.velocity = new Vector2(jumpDirection * wallJumpForceX, wallJumpForceY);
            transform.localScale = new Vector3(jumpDirection, 1, 1);
            wallJumpCooldown = 0;
        }
    }

    private void HandleSliding()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            isSliding = true;
            anim.SetTrigger("Slide");
        }
        else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            isSliding = false;
        }
    }

    private void HandleAttacks()
    {
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Return))
        {
            sword.SetActive(true);
            swordCollider.enabled = true;
        }
        else if (Input.GetKeyUp(KeyCode.J) || Input.GetKeyUp(KeyCode.Return))
        {
            sword.SetActive(false);
            swordCollider.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.RightShift))
        {
            anim.SetTrigger("GunAttack");
            gun.SetActive(true);
            ShootBullet();
        }
        else if (Input.GetKeyUp(KeyCode.K) || Input.GetKeyUp(KeyCode.RightShift))
        {
            gun.SetActive(false);
        }
    }

    private void ShootBullet()
    {
        if (bulletPrefab == null || bulletSpawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(transform.localScale.x);
        }
    }

    private void UpdateAnimations(bool isMoving)
    {
        anim.SetBool("run", isMoving);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetFloat("verticalVelocity", body.velocity.y);
        anim.SetBool("isFalling", body.velocity.y < -0.1f && !isGrounded);
    }

    public void TakeDamage()
    {
        if (isHurt || isDead) return;

        health--;
        isHurt = true;
        anim.SetTrigger("Hurt");

        if (health <= 0)
            Die();
        else
            Invoke(nameof(ResetHurt), 0.5f);
    }

    private void ResetHurt()
    {
        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        anim.SetTrigger("Dead");
        body.velocity = Vector2.zero;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.CapsuleCast(capsuleCollider.bounds.center, capsuleCollider.size, capsuleCollider.direction, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
}
