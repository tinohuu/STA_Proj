using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCoinWindow : MonoBehaviour
{
    Vector3 m_StartPos;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IPlay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Vector3 pos)
    {
        m_StartPos = pos;
    }
    IEnumerator IPlay()
    {
        SoundManager.Instance.PlaySFX("coinRewardComplete");
        RewardNumber.Switches[RewardType.Coin] = true;
        Reward.Coin += 0;
        yield return new WaitForSeconds(0.25f);
        ParticleManager.Instance.CreateCoin(m_StartPos);
        yield return new WaitForSeconds(0.25f);
        ParticleManager.Instance.CreateCoin(m_StartPos);
        yield return new WaitForSeconds(0.25f);
        ParticleManager.Instance.CreateCoin(m_StartPos);

        yield return new WaitForSeconds(2f);
        Finish();
    }

    public void Finish()
    {
        StopAllCoroutines();
        GetComponent<WindowAnimator>().Close();
        //ShopManager.Instance.Open();
    }
}
