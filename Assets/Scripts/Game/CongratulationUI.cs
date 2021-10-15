using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CongratulationUI : MonoBehaviour
{
    Image imgCongratulation;

    Image imgOutstanding;

    Image imgGreyStar_1;

    Image imgGreyStar_2;

    Image imgGreyStar_3;

    Image imgStar_1;
    Image imgStar_2;
    Image imgStar_3;

    private void Awake()
    {
        imgCongratulation = gameObject.transform.Find("Congratulation").GetComponent<Image>();

        imgOutstanding = gameObject.transform.Find("Outstanding").GetComponent<Image>();

        imgGreyStar_1 = gameObject.transform.Find("GreyStar_1").GetComponent<Image>();
        imgGreyStar_2 = gameObject.transform.Find("GreyStar_2").GetComponent<Image>();
        imgGreyStar_3 = gameObject.transform.Find("GreyStar_3").GetComponent<Image>();

        imgStar_1 = gameObject.transform.Find("Star_1").GetComponent<Image>();
        imgStar_2 = gameObject.transform.Find("Star_2").GetComponent<Image>();
        imgStar_3 = gameObject.transform.Find("Star_3").GetComponent<Image>();

        ResetStarImage();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeIn(int nScoreStar)
    {
        imgCongratulation.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
        imgCongratulation.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        imgOutstanding.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
        imgOutstanding.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        imgGreyStar_1.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        imgGreyStar_1.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        imgGreyStar_1.CrossFadeColor(Color.grey, 0.1f, true, true);

        imgGreyStar_2.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        imgGreyStar_2.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        imgGreyStar_2.CrossFadeColor(Color.grey, 0.1f, true, true);

        imgGreyStar_3.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        imgGreyStar_3.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        imgGreyStar_3.CrossFadeColor(Color.grey, 0.1f, true, true);

        imgStar_1.transform.localScale = new Vector3(0.875f * 5, 0.875f * 5, 1.0f);
        imgStar_2.transform.localScale = new Vector3(1.0f * 5, 1.0f * 5, 1.0f);
        imgStar_3.transform.localScale = new Vector3(0.875f * 5, 0.875f * 5, 1.0f);

        ResetStarImage();

        StartCoroutine(FadeInCoroutine(nScoreStar));
    }

    public void FadeOut()
    {
        imgCongratulation.CrossFadeAlpha(0.0f, 0.5f, true);
        imgOutstanding.CrossFadeAlpha(0.0f, 0.5f, true);

        imgGreyStar_1.CrossFadeAlpha(0.0f, 0.5f, true);
        imgGreyStar_2.CrossFadeAlpha(0.0f, 0.5f, true);
        imgGreyStar_3.CrossFadeAlpha(0.0f, 0.5f, true);

        imgStar_1.CrossFadeAlpha(0.0f, 0.5f, true);
        imgStar_2.CrossFadeAlpha(0.0f, 0.5f, true);
        imgStar_3.CrossFadeAlpha(0.0f, 0.5f, true);
    }

    void ResetStarImage()
    {
        imgStar_1.enabled = false;
        imgStar_2.enabled = false;
        imgStar_3.enabled = false;
    }

    IEnumerator FadeInCoroutine(int nStar)
    {
        float fTime = Time.time;

        imgCongratulation.CrossFadeAlpha(1.0f, 1.0f, true);
        imgCongratulation.transform.DOScale(1.2f, 0.8f);

        while(Time.time - fTime < 0.8f)
        {
            yield return null;
        }

        imgCongratulation.transform.DOScale(1.0f, 0.2f);

        while (Time.time - fTime < 1.0f)
        {
            yield return null;
        }

        imgOutstanding.CrossFadeAlpha(1.0f, 1.0f, true);
        imgOutstanding.transform.DOScale(1.2f, 0.8f);

        while (Time.time - fTime < 1.8f)
        {
            yield return null;
        }

        imgOutstanding.transform.DOScale(1.0f, 0.2f);

        while (Time.time - fTime < 2.0f)
        {
            yield return null;
        }

        imgGreyStar_1.CrossFadeAlpha(1.0f, 0.8f, true);
        imgGreyStar_1.transform.DOScale(0.875f, 0.8f);

        imgGreyStar_2.CrossFadeAlpha(1.0f, 0.8f, true);
        imgGreyStar_2.transform.DOScale(1.0f, 0.8f);
        //imgStar_2.CrossFadeColor()

        imgGreyStar_3.CrossFadeAlpha(1.0f, 0.8f, true);
        imgGreyStar_3.transform.DOScale(0.875f, 0.8f);

        while (Time.time - fTime < 2.8f)
        {
            yield return null;
        }

        //todo: here we show the stars...
        if(nStar > 0)
        {
            imgStar_1.enabled = true;
            imgStar_1.transform.DOScale(0.875f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        if(nStar > 1)
        {
            imgStar_2.enabled = true;
            imgStar_2.transform.DOScale(1.0f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        if (nStar > 2)
        {
            imgStar_3.enabled = true;
            imgStar_3.transform.DOScale(0.875f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        StopCoroutine(FadeInCoroutine(nStar));

        FadeOut();

        yield return new WaitForSeconds(0.5f);

        GameplayMgr.Instance.gameplayUI.FadeInSettlementUI(0, 0);
    }
}
