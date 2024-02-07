using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static Enemy;

public class Player : MonoBehaviour
{
    private Animator playerAnimator;
    public SpriteRenderer playerSprite;
    public GameObject playerDieEffect;
    private Mesh mesh;

    public TrailRenderer trailRenderer;

    [Space(10)]
    private Color playerColor;
    public Color PlayerColor { get => playerColor; private set => playerColor = value; }
    private Color playerAttackColor;
    public Color PlayerAttackColor { get => playerAttackColor; private set => playerAttackColor = value; }

    private Color trailAttackColor;

    public Image dashFatigueVisual;
    public float rotationSpeed = 5f;
    public float moveSpeed = 10f;
    public float maxDistance = 1f;
    public float minSpeed = 1f;

    /// <summary>
    /// Player can't do anything
    /// </summary>
    private bool freezePlayer;

    public bool FreezePlayer { get { return freezePlayer; } }
    private bool playerDead;
    public bool PlayerDead { get { return playerDead; } }

    private bool isDashing;
    /// <summary>
    /// Enables a dash fatigue system to prevent spamming dash
    /// </summary>
    //private bool useDashFatigue = true;
    private float dashFrequency = 0.33f;
    private readonly float dashMultiplier = 5f;
    private float dashCurrentMultiplier = 1f;
    private float dashFatigue = 0f;
    private float dashFatigueIncrement = 0.5f;
    private readonly float dashFatigueMax = 2.75f;
    private bool dashFatigued;
    private bool killedEnemyThisDash;
    private bool didMultikillThisDash;
    private int killedEnemiesThisDash = 0;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isAttacking;
    public bool IsAttacking { get { return isAttacking; } }


    private Coroutine dashCoroutine;
    public float bendFactor = 1f;

    private bool mobileControls;
    private bool buttonDash;

    void Awake()
    {
        PlayerColor = new Color(0.62f, 0.85f, 0.99f, 1f);
        PlayerAttackColor = new Color(0.92f, 0.71f, 1f, 1f);
        trailAttackColor = PlayerAttackColor;

        dashFatigueVisual.enabled = false;
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerSprite.color = PlayerColor;
    }

    public void SetColors()
    {
        PlayerColor = Manager.Instance.overridePlayerColor ?? PlayerColor;
        PlayerAttackColor = Manager.Instance.overrideAttackColor ?? PlayerAttackColor;
        trailAttackColor = Manager.Instance.overrideAttackColor ?? trailAttackColor;
    }

    public void InitGame()
    {
        Debug.Log("InitGame");
        transform.rotation = Quaternion.identity;
        // Could be drifting
        rb.velocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.None;

        // I like resuming from same position
        //transform.position = Vector3.zero;
        dashFatigue = 0f;
        isDashing = false;
        isAttacking = false;
        dashFatigued = false;
        trailRenderer.emitting = false;
        playerSprite.color = PlayerColor;
        dashFatigueVisual.enabled = false;

        FreezeControls(unfreeze: true);
        playerDead = false;
        playerAnimator.Play("Spawn", 0);
    }

    public void EnableMobileControls()
    {
        mobileControls = true;
    }

    public void ButtonDash()
    {
        buttonDash = true;
    }

    private void Update()
    {
        if (freezePlayer)
            return;

        if (dashFatigue > 0)
        {
            if (dashFatigue > dashFatigueMax)
            {
                dashFatigued = true;
            }

            // Player bias
            dashFatigue -= Time.deltaTime * 2;
        }
        else
        {
            dashFatigued = false;
        }

        // Repetitive dashing fine until spam it
        //Debug.Log(dashFatigue);
        if (((!mobileControls && Input.GetMouseButtonDown(0)) || (mobileControls && buttonDash)) && !isDashing && !dashFatigued)
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = StartCoroutine(DashCoroutine());
            buttonDash = false;
        }
    }
    void FixedUpdate()
    {
        Manager.Instance.SetPlayerPositionForMaterial(transform.position);
        if (freezePlayer)
            return;

        // TODO: Should check only first touch on mobile controls
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;

        Vector3 direction = mousePosition - transform.position;
        float distance = direction.magnitude;

        Vector3 spriteDirection = mousePosition - playerSprite.transform.position;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, spriteDirection);
        playerSprite.transform.rotation = Quaternion.Lerp(playerSprite.transform.rotation, rotation, rotationSpeed);


        if (isDashing)
        {
            currentSpeed = moveSpeed * dashCurrentMultiplier;
            isAttacking = true;
        }
        else
        {
            currentSpeed = Mathf.Max(moveSpeed, minSpeed);
            isAttacking = false;
        }

        if (distance > maxDistance)
        {
            rb.MovePosition(transform.position + direction.normalized * currentSpeed * Time.fixedDeltaTime);
        }
        else // We are sitting at mouse
        {
            if (isDashing)
            {
                StartCoroutine(StopDashing(immediate: true));
                //  In boosting animation
                playerAnimator.Play("Idle", 0);
            }
        }
    }
    void LateUpdate()
    {
        UpdatePlayerSprite();
    }
    void UpdatePlayerSprite()
    {
        Color setCol = PlayerColor;
        if (dashFatigued)
        {
            dashFatigueVisual.enabled = true;
            dashFatigueVisual.fillAmount = dashFatigue / dashFatigueMax;
        }
        else if (dashFatigue > 0)
        {
            setCol = Color.Lerp(PlayerColor, Color.red, dashFatigue / dashFatigueMax);
        }
        else
        {
            dashFatigueVisual.enabled = false;
        }

        if (isDashing)
            setCol = PlayerAttackColor;

        playerSprite.color = setCol;
    }
    private IEnumerator DashCoroutine()
    {
        trailRenderer.Clear();
        trailRenderer.emitting = true;
        trailRenderer.time = 0.3f;
        trailRenderer.startColor = trailAttackColor;
        trailRenderer.endColor = trailAttackColor;

        isDashing = true;
        dashCurrentMultiplier = dashMultiplier;
        playerAnimator.Play("Boost", 0);
        AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.shipWoosh);

        yield return new WaitForSeconds(dashFrequency);

        trailRenderer.startColor = PlayerColor;

        while (dashCurrentMultiplier > 1f)
        {
            dashCurrentMultiplier -= Time.deltaTime * 10f;

            yield return null;
        }

        trailRenderer.endColor = PlayerColor;

        yield return StopDashing();
    }
    IEnumerator StopDashing(bool immediate = false)
    {
        isDashing = false;
        killedEnemiesThisDash = 0;
        killedEnemyThisDash = false;

        if (!dashFatigued)
            dashFatigue += dashFatigueIncrement;
        if (immediate)
        {
            // looks bad
            //trailRenderer.time = 0f;
            trailRenderer.emitting = false;
            yield break;
        }
        while (trailRenderer.time > 0)
        {
            trailRenderer.time -= Time.deltaTime * 2f;
            yield return null;
        }
        trailRenderer.emitting = false;
    }

    public void KillEnemy(EnemyType enemyType, Enemy enemy)
    {
        Manager.Instance.AddScore();
        Manager.Instance.DoCombo(enemy);

        if (killedEnemyThisDash)
        {
            Manager.Instance.AddMultikill(killedEnemiesThisDash);
            if (!didMultikillThisDash)
                AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.slashKill);
            didMultikillThisDash = true;
        }
        else
        {
            AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.kill);
            didMultikillThisDash = false;
        }
        killedEnemyThisDash = true;
        killedEnemiesThisDash++;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            if (!isAttacking)
            {
                EnemyHit(enemy);
            }
        }
    }

    public void FreezeControls(bool unfreeze = false)
    {
        freezePlayer = !unfreeze;
    }

    public void EnemyHit(Enemy enemy)
    {
        rb.freezeRotation = false;
        Vector3 direction = (transform.position - enemy.transform.position).normalized;
        float randForce = Random.Range(100f, 200f);
        rb.AddForce(direction * randForce);
        rb.AddTorque(Random.Range(-100f, 100f));
        DoGameOver();
    }

    public void BulletHit(Bullet bullet)
    {
        rb.freezeRotation = false;
        Vector3 direction = (transform.position - bullet.transform.position).normalized;
        float randForce = Random.Range(20f, 100f);
        rb.AddForce(direction * randForce);
        rb.AddTorque(Random.Range(-10f, 10f));
        DoGameOver();
    }
    void DoGameOver()
    {
        if (playerDead)
        {
            AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.hit);
            return;
        }
        playerDead = true;
        //playerAnimator.Play("Die", 0);
        HideAllDiagetic();
        playerDieEffect.gameObject.SetActive(true);
        playerDieEffect.GetComponent<PlayerDieParticles>().Init(PlayerColor);

        Manager.Instance.GameOver();
        FreezeControls();
        Manager.Instance.UpdateState();
        AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.shipBreak);
    }

    void HideAllDiagetic()
    {
        trailRenderer.emitting = false;
        dashFatigueVisual.enabled = false;
    }
}
