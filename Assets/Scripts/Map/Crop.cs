using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.Mapmaker;

public class Crop : MonoBehaviour
{
    public enum State { locked, unlocking, immature, mature }

    [Header("Ref")]
    CropSpine m_Spine;
    CropSpine m_SpinePrefab;
    //public List<AnimatorOverrideController> Controllers = new List<AnimatorOverrideController>();

    [Header("Config")]
    //public Mapmaker_CropConfig MapmakerConfig;
    public string CropName = "Cabbage";
    public string SpineName = "";
    public int Variant = 0;
    [Header("Data")]
    public int UnlockedLevel = 1;
    public int UnlockingLevel = 1;

    public State CropState = State.locked;

    public void UpdateView(bool updateLocPos = false)
    {
        m_SpinePrefab = Resources.Load<GameObject>("Prefabs/Crops/CropSpine_" + CropName + SpineName).GetComponent<CropSpine>();

        StopAllCoroutines(); // Stop delay animation
        if (m_Spine) Destroy(m_Spine.gameObject); // test: rebuild prefab, e.g. set progress backward

        UpdateState();
        if (HasState(CropState))
        {
            m_Spine = Instantiate(m_SpinePrefab.gameObject, transform).GetComponent<CropSpine>();

            m_Spine.SetState(Variant, (int)CropState);
        }
    }


    public State UpdateState()
    {
        int curLevel = MapManager.Instance.Data.CompleteLevel;

        if (curLevel < UnlockingLevel)
            CropState = State.locked;
        else if (curLevel < UnlockedLevel)
            CropState = State.unlocking;
        else
            CropState = CropManager.Instance.IsMature ? State.mature : State.immature;

        return CropState;
    }

    public bool HasState(State state)
    {
        return state >= m_SpinePrefab.MinState && state <= m_SpinePrefab.MaxState;
    }

    public bool IsInScreen()
    {
        Vector2 minPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 pos = transform.position;
        return pos.x >= minPos.x && pos.x <= maxPos.x && pos.y >= minPos.y && pos.y <= maxPos.y;
    }

    public void UpdateAnimator(bool includeState = true)
    {
        if (includeState) UpdateState();
        if (m_Spine)
        m_Spine.SetState(Variant, (int)CropState);
    }



    public void PlayHarvestEffect(ParticleSystemForceField[] fields, Collider[] triggers)
    {
        GameObject particleObj = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + CropName + SpineName);
        if (!particleObj) return;
        ParticleSystem particle = Instantiate(particleObj, transform).transform.GetChild(0).GetComponent<ParticleSystem>();
        //particle.transform.localScale = Vector3.one * Mathf.Abs(Scale);
        particle.transform.SetParent(CropManager.Instance.transform);
        var externalForcesModule = particle.externalForces;
        foreach (var field in fields) externalForcesModule.AddInfluence(field);
        var triggerModule = particle.trigger;
        foreach (var trigger in triggers) triggerModule.AddCollider(trigger);
    }

    public bool IsVisible => m_Spine.IsVisible;
}
