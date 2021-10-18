using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropSpine : MonoBehaviour
{
    public string SpineName = "Crop";
    public Crop.State MinState = Crop.State.locked;
    public Crop.State MaxState = Crop.State.mature;
    public List<AnimatorOverrideController> Controllers = new List<AnimatorOverrideController>();
    Animator m_Animator;
    Renderer m_Renderer;
    public bool IsVisible => m_Renderer && m_Renderer.isVisible;
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Renderer = GetComponent<Renderer>();
    }
    // Start is called before the first frame update


    private void OnBecameVisible()
    {
        m_Animator.SetTrigger("Force");
    }

    public void SetState(int variant, int state)
    {
        StartCoroutine(ISetAnimatorState(variant, state));
    }

    IEnumerator ISetAnimatorState(int variant, int state)
    {
        if (Controllers.Count > 0) m_Animator.runtimeAnimatorController = Controllers[Mathf.Clamp(variant, 0, Controllers.Count - 1)];
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
                m_Animator.SetInteger("State", 3);
                m_Animator.SetTrigger("Force");
            }
            else
            {
                float pause = cropWidth / viewWidth * 0.3f + cropHeight / viewHeight * 0.5f;
                m_Animator.SetInteger("State", 2);
                yield return new WaitForSeconds(pause);
                m_Animator.SetInteger("State", 3);
            }
        }
        else
        {
            m_Animator.SetInteger("State", state);
        }
    }
}
