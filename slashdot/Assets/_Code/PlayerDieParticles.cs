using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieParticles : MonoBehaviour
{
    private ParticleSystem pS;
    void Awake()
    {
        pS = GetComponent<ParticleSystem>();
    }

    public void Init(Color color)
    {
        var main = pS.main;
        main.startColor = color;
        pS.Play();
    }
}
