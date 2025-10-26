using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXClickMarker : MonoBehaviour
{

    public ParticleSystem particle;

   public void PlayParticle()
    {
        if (particle) particle.Play();
    }
}
