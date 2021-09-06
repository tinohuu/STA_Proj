using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public enum State { locked, unlocking, immature, mature }

    [Header("Config")]
    public string Name = "Crop";
    public float Scale = 0.5f;
    public int Variant = 0;

    [Header("Spine Config")]
    public GameObject SpinePrefab;
    public State MinState = State.locked;
    public State MaxState = State.mature;

    [Header("Debug")]
    public List<AnimatorOverrideController> Controllers = new List<AnimatorOverrideController>();
    public CropConfig Config = new CropConfig();
    public float OrderRatio = 0;
    public State CropState = State.locked;
    public float YDis = 0;
    GameObject spineObject = null;

    private void Awake()
    {
        // Turn off placeholder
        //GetComponent<SpriteRenderer>().enabled = false;
    }

    public State UpdateState()
    {
        State state;
        if (MapManager.Instance.Data.CompleteLevel < Config.MinLevel) state = State.locked;
        else if (MapManager.Instance.Data.CompleteLevel >= Config.Level)
        {
            state = CropManager.Instance.IsMature ? State.mature : State.immature;
        }
        else
        {
            float fieldLength = Config.Level - Config.MinLevel + 1;
            float progressRatio = (float)(MapManager.Instance.Data.CompleteLevel - Config.MinLevel) / fieldLength;
            bool canShowUnlocking = progressRatio >= OrderRatio;
            state = canShowUnlocking ? State.unlocking : State.locked;
        }
        CropState = state;
        return state;
    }

    public void UpdateView()
    {
        StopAllCoroutines();
        if (spineObject) Destroy(spineObject); // TEST
        UpdateState();
        if (HasState(CropState))
        {
            spineObject = Instantiate(SpinePrefab, transform);
            spineObject.transform.localScale = new Vector3(Scale, Mathf.Abs(Scale), Mathf.Abs(Scale));
            StartCoroutine(SetAnimatorState((int)CropState));
        }
    }
    public bool UpdateAnimator(bool includeState = true)
    {
        bool inScreen = IsInScreen();
        if (!inScreen) GetComponentInChildren<Animator>()?.SetTrigger("Force");
        if (includeState) //GetComponentInChildren<Animator>()?.SetInteger("State", (int)CropState);
        {
            StartCoroutine(SetAnimatorState((int)CropState));
        }
        return inScreen;
    }

    public bool IsInScreen()
    {
        Vector2 minPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 maxPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 pos = transform.position;
        return pos.x >= minPos.x && pos.x <= maxPos.x && pos.y >= minPos.y && pos.y <= maxPos.y;
    }

    public void PlayHarvestEffect(ParticleSystemForceField[] fields, Collider[] triggers)
    {
        GameObject particleObj = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + Name);
        ParticleSystem particle = Instantiate(particleObj,transform).transform.GetChild(0).GetComponent<ParticleSystem>();
        particle.transform.localScale = Vector3.one * Mathf.Abs(Scale);
        particle.transform.SetParent(CropManager.Instance.transform);
        var externalForcesModule = particle.externalForces;
        foreach (var field in fields) externalForcesModule.AddInfluence(field);
        var triggerModule = particle.trigger;
        foreach (var trigger in triggers) triggerModule.AddCollider(trigger);
    }

    public bool HasState(State state)
    {
        return state >= MinState && state <= MaxState;
    }

    IEnumerator SetAnimatorState(int state)
    {
        if (!spineObject) yield break;
        Animator animator = spineObject.GetComponent<Animator>();
        if (Controllers.Count > 0) animator.runtimeAnimatorController = Controllers[Mathf.Clamp(Variant, 0, Controllers.Count - 1)];
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
                //float height = Mathf.Clamp(Mathf.Abs(0 - transform.localPosition.y), 0, 10);
                //float width = Mathf.Clamp(Mathf.Abs(-10 - transform.position.x), 0, 20);
                //float ratio = height / 10 * 1f + width / 20 * 0.5f - 0.5f;// + Random.Range(0, 0.3f);
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
}
