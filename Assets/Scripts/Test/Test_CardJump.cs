using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_CardJump : MonoBehaviour
{
    
    public Transform EndPoint;/*
    public float TimeScale = 1;

    float xSpeed = 0;
    float ySpeed = 0;
    float width = 0;
    float height = 0;
    float cardWidth = 1;
    float startTime = 0;
    public int state = 0;
    // Start is called before the first frame update
    void Start()
    {
        width = EndPoint.position.x - transform.position.x;
        height = transform.position.y - EndPoint.position.y;
        xSpeed = 5f / 3 * width;
        ySpeed = 10 * cardWidth;
        startTime = Time.time;
        transform.DORotate(new Vector3(0, 0, 360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);
        Time.timeScale = TimeScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            ySpeed -= (25f * cardWidth) * Time.deltaTime;
            transform.position += new Vector3(xSpeed, ySpeed, 0) * Time.deltaTime;
            if (Time.time - startTime >= 0.4f)
            {
                state = 1;
                ySpeed = 0;
            }
        }
        else if (state == 1)
        {
            xSpeed -= (25f / 6 * width) * Time.deltaTime;
            ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(height))) * Time.deltaTime;
            transform.position += new Vector3(xSpeed, -ySpeed, 0) * Time.deltaTime;
            if (Time.time - startTime >= 0.7f)
            {
                state = 2;
                ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(height));
            }
        }
        else if (state == 2)
        {
            transform.DOMove(EndPoint.position, 0.1f);
            //transform.position += new Vector3(xSpeed, -ySpeed, 0) * Time.deltaTime;
            if (Time.time - startTime >= 0.1f)
            {
                transform.DOComplete();
                state = 3;
            }
        }
    }
    */

    void Start()
    {
        StartCoroutine(CardJump(transform, 1, EndPoint.position));
        //Time.timeScale = 0.5f;
    }

    IEnumerator CardJump(Transform card, float cardWidth, Vector3 target)
    {
        float _Width = target.x - card.position.x;
        float _Height = card.transform.position.y - target.y;
        float _xSpeed = 5f / 3 * _Width;
        float _ySpeed = 10 * cardWidth;
        float _StartTime = Time.time;

        // Rotate card by Dotween
        transform.DORotate(new Vector3(0, 0, _Width < 0 ? 360 : -360), 0.8f, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine);

        // Stage 1
        while (Time.time - _StartTime < 0.4f)
        {
            _ySpeed -= (25f * cardWidth) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, _ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 0;

        // Stage 2
        while (Time.time - _StartTime < 0.7f)
        {
            _xSpeed -= (25f / 6 * _Width) * Time.deltaTime;
            _ySpeed += (50f / 3 * (2 * cardWidth + Mathf.Abs(_Height))) * Time.deltaTime;
            card.position += new Vector3(_xSpeed, -_ySpeed, 0) * Time.deltaTime;
            yield return null;
        }
        _ySpeed = 10f / 3 * (2 * cardWidth + Mathf.Abs(_Height));

        // Stage 3
        Vector3 oriPos = card.position;
        float dis = (oriPos - target).magnitude;
        while (Time.time - _StartTime < 0.8f)
        {
            card.position = Vector3.Lerp(oriPos, target, (Time.time - _StartTime - 0.7f) / 0.1f);
            yield return null;
        }
    }
}
