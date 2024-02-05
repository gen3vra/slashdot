using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ShootingEnemy : Enemy
{
    private Coroutine shootCoroutine;

    public override void Init()
    {
        base.Init();
        moveSpeed = (1 + Random.Range(0, movementVariance)) * Manager.Instance.GetDifficultyScale();
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }
        shootCoroutine = StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(3f);
            if (!isDead)
            {
                GameObject bullet = Manager.Instance.GetBullet();
                bullet.GetComponent<Bullet>().Init(transform.position, player.transform.position);
                AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.enemyLaser);
            }
        }
    }
}
