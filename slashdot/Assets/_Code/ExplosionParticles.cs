using UnityEngine;
using System.Collections;

public class ExplosionParticles : MonoBehaviour
{
    private ParticleSystem pS;

    private Color particleColor;
    private bool cleanUp = false;
    public void Init(Color col)
    {
        particleColor = col;
        pS = GetComponent<ParticleSystem>();
        ResetParticleSystem();
    }

    private void ResetParticleSystem()
    {
        var main = pS.main;
        main.startColor = particleColor;
        pS.Play();
        cleanUp = true;
    }

    public void OnEnable()
    {
        if (cleanUp)
            // coroutine to clean up the particle system after its lifetime
            StartCoroutine(CleanUp());
    }

    private IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(pS.main.startLifetime.constant);
        cleanUp = false;
        Manager.Instance.RemoveExplosionParticles(this);
    }
}
