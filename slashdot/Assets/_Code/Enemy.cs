using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public partial class Enemy : MonoBehaviour
{
    public Player player;
    public Color enemyBaseColor;
    private Vector3 targetPosition;
    public float moveSpeed;
    private float spinSpeed;

    public SpriteRenderer spriteRenderer;

    public enum EnemyType
    {
        Moving,
        Shooting
    }
    public EnemyType CurrentEnemyType = EnemyType.Moving;
    /// <summary>
    /// Set in editor
    /// </summary>
    public float movementVariance;
    public bool isDead;
    private Rigidbody2D rb;
    /// <summary>
    ///  Called after spawn
    /// </summary>
    public virtual void Init()
    {
        isDead = false;
        moveSpeed = (2f + Random.Range(-movementVariance, movementVariance)) * Manager.Instance.GetDifficultyScale();
        spinSpeed = Random.Range(100f, 200f) * (Random.Range(0, 2) == 0 ? 1 : -1);

        // Color sprite based on difficulty
        spriteRenderer.color = Color.Lerp(Color.white, enemyBaseColor, moveSpeed / 3f);
    }

    void Start()
    {
        player = Manager.Instance.player;
        rb = GetComponent<Rigidbody2D>();

    }

    void FixedUpdate()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        targetPosition = transform.position + moveSpeed * Time.deltaTime * direction;

        rb.MovePosition(targetPosition);
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            if (player.IsAttacking && !isDead)
            {
                player.KillEnemy(CurrentEnemyType, this);
                Manager.Instance.UpdateState();
                CameraEffectsHandler.Instance.RequestCameraShake(0.1f, 0.3f);
                var explosion = Manager.Instance.GetExplosionParticles(spriteRenderer.color, transform.position);

                isDead = true;
                Manager.Instance.RemoveEnemy(this);
            }
            // Handled by player normal collision2D for hitbox size biasing 
            /*else
            {
                player.EnemyHit();
            }*/
        }
    }
}
