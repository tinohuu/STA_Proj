using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STA.Mapmaker;

public class Crop : MonoBehaviour
{
    public enum State { locked, unlocking, immature, mature }

    [Header("Ref")]
    public GameObject SpinePrefab;
    public List<AnimatorOverrideController> Controllers = new List<AnimatorOverrideController>();

    [Header("Config")]
    public Mapmaker_CropConfig MapmakerConfig;

    [Header("Data")]
    public int UnlockedLevel = 1;
    public int UnlockingLevel = 1;
    public State MinState = State.locked;
    public State MaxState = State.mature;
    public State CropState = State.locked;

    GameObject spineObject = null;

    public void UpdateView(bool updateLocPos = false)
    {
        if (updateLocPos) transform.localPosition = MapmakerConfig.LocPos;

        StopAllCoroutines(); // Stop delay animation
        if (spineObject) Destroy(spineObject); // test: rebuild prefab, e.g. set progress backward

        UpdateState();
        if (HasState(CropState))
        {
            spineObject = Instantiate(SpinePrefab, transform);
            spineObject.transform.localScale = new Vector3(MapmakerConfig.Scale, Mathf.Abs(MapmakerConfig.Scale), Mathf.Abs(MapmakerConfig.Scale));
            StartCoroutine(SetAnimatorState((int)CropState));
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
        return state >= MinState && state <= MaxState;
    }

    public bool IsInScreen()
    {
        Vector2 minPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 pos = transform.position;
        return pos.x >= minPos.x && pos.x <= maxPos.x && pos.y >= minPos.y && pos.y <= maxPos.y;
    }

    public bool UpdateAnimator(bool updateState = true)
    {
        bool inScreen = IsInScreen();
        if (!inScreen) GetComponentInChildren<Animator>()?.SetTrigger("Force");
        if (updateState) StartCoroutine(SetAnimatorState((int)CropState));
        return inScreen;
    }

    IEnumerator SetAnimatorState(int state)
    {
        if (!spineObject) yield break;
        Animator animator = spineObject.GetComponent<Animator>();
        if (Controllers.Count > 0) animator.runtimeAnimatorController = Controllers[Mathf.Clamp(MapmakerConfig.Variant, 0, Controllers.Count - 1)];
        if (state >= 3)
        {
            Vector2 minPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
            Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

            float viewWidth = maxPos.x - minPos.x;
            float viewHeight = maxPos.y - minPos.y;

            float cropWidth = transform.position.x - minPos.x;
            float cropHeight = maxPos.y - transform.position.y;

            if (cropWidth > viewWidth || cropWidth < 0)
            {
                animator.SetInteger("State", 3);
                animator.SetTrigger("Force");
            }
            else
            {
                float pause = cropWidth / viewWidth * 0.3f + cropHeight / viewHeight * 0.5f;
                animator.SetInteger("State", 2);
                yield return new WaitForSeconds(pause);
                animator.SetInteger("State", 3);
            }
        }
        else
        {
            animator.SetInteger("State", state);
        }
    }

    public void PlayHarvestEffect(ParticleSystemForceField[] fields, Collider[] triggers)
    {
        GameObject particleObj = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + MapmakerConfig.Name);
        ParticleSystem particle = Instantiate(particleObj, transform).transform.GetChild(0).GetComponent<ParticleSystem>();
        particle.transform.localScale = Vector3.one * Mathf.Abs(MapmakerConfig.Scale);
        particle.transform.SetParent(CropManager.Instance.transform);
        var externalForcesModule = particle.externalForces;
        foreach (var field in fields) externalForcesModule.AddInfluence(field);
        var triggerModule = particle.trigger;
        foreach (var trigger in triggers) triggerModule.AddCollider(trigger);
    }
}
