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

    State UpdateState()
    {
        State state;
        if (MapManager.Instance.Data.CompelteLevel < Config.MinLevel) state = State.locked;
        else if (MapManager.Instance.Data.CompelteLevel >= Config.Level) state = State.immature;
        else
        {
            float fieldLength = Config.Level - Config.MinLevel + 1;
            float progressRatio = (float)(MapManager.Instance.Data.CompelteLevel - Config.MinLevel) / fieldLength;
            bool canShowUnlocking = progressRatio >= OrderRatio;
            state = canShowUnlocking ? State.unlocking : State.locked;
        }
        CropState = state;
        return state;
    }

    public void UpdateView()
    {
        if (spineObject) Destroy(spineObject); // TEST
        UpdateState();
        if (HasState(CropState))
        {
            spineObject = Instantiate(SpinePrefab, transform);
            spineObject.transform.localScale = new Vector3(Scale, Mathf.Abs(Scale), Mathf.Abs(Scale));
            Animator animator = spineObject.GetComponent<Animator>();
            if (Controllers.Count > 0) animator.runtimeAnimatorController = Controllers[Mathf.Clamp(Variant, 0, Controllers.Count - 1)];
            float height = Mathf.Clamp(Mathf.Abs(0 - transform.localPosition.y), 0, 10);
            float width = Mathf.Clamp(Mathf.Abs(-10 - transform.position.x), 0, 20);
            float ratio = (1 - height / 10) * 1.5f + (1 - width / 20) * 0.5f - 0.2f;// + Random.Range(0, 0.3f);

            animator.SetFloat("Pause", Mathf.Clamp(ratio, 0.2f, 1));//Random.Range(0.5f, 1f));
            animator.SetInteger("State", (int)CropState + 1);
        }
    }

    public bool HasState(State state)
    {
        return state >= MinState && state <= MaxState;
    }
}
