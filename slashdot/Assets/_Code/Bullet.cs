using UnityEngine;
public class Bullet : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color bulletColor;
    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;
    private Vector3 targetDir;
    public float moveSpeed;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }
    /// <summary>
    /// Must call init, cleans up after pool too
    /// </summary>
    /// <param name="playerPos"></param>
    public void Init(Vector3 startPos, Vector3 playerPos)
    {
        spriteRenderer.color = bulletColor;
        trailRenderer.startColor = bulletColor;
        trailRenderer.endColor = bulletColor;
        moveSpeed = Manager.Instance.GetDifficultyScale() > 2 ? 7.5f : 5f;
        transform.position = startPos;
        // get player angle from us
        Vector3 direction = (playerPos - transform.position).normalized;
        // calculate the new position normalized
        targetDir = direction;
        // rotate to face player
        transform.up = targetDir;
        gameObject.SetActive(true);
    }
    void FixedUpdate()
    {
        // move the Rigidbody2D to the new position
        rb.MovePosition(transform.position + targetDir * moveSpeed * Time.deltaTime);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            if (player.IsAttacking)
            {
                Manager.Instance.RemoveBullet(this);
            }
            else
            {
                // highlight bullet
                /*spriteRenderer.color = Color.red;
                moveSpeed = 0f;*/
                Manager.Instance.RemoveBullet(this);
                // TODO: Spawn particles
                Debug.Log("TODO: Spawn particles");

                player.BulletHit(this);
            }
        }
        else if (collision.gameObject.TryGetComponent(out Bullet bullet))
        {
            // highlight bullet
            /*
            spriteRenderer.color = Color.blue;
            moveSpeed *= 0.5f;
            bullet.moveSpeed *= 0.5f;
            bullet.spriteRenderer.color = Color.blue;*/
            Manager.Instance.RemoveBullet(this);
        }
        else if (collision.gameObject.CompareTag("BorderWall"))
        {
            Manager.Instance.RemoveBullet(this);
        }
    }
}