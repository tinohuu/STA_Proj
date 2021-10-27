using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPokerCallBack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleTrigger()
    {
        int nRandom = Random.Range(0, 100);

        int nOdd = nRandom % 2;

        if(nOdd == 1 )
            SoundManager.Instance.PlaySFX("gameClearFlyingCardHigh");
        else
            SoundManager.Instance.PlaySFX("gameClearFlyingCardLow");

        //ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        //Debug.Log("---------------------------------here we collide with the plane...");

        //List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();
        //int nCount = particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles);

        //Debug.Log("---------------------------------here we collide with the plane... the count is: " + nCount);
    }
}
