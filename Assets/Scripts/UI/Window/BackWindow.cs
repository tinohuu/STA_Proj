using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackWindow : Window
{
    Image image;
    Button button;
    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        WindowAnimator.OnQueueChanged += () => SetButton();
        button.onClick.AddListener(() => CloseLastWindow());
    }

    void SetButton()
    {
        if (!image) return; // Fix error when image isn't initialized
        bool enable = WindowAnimator.WindowQueue.Count > 0 && WindowAnimator.WindowQueue.Last().ShowPanel;
        image.raycastTarget = enable;
        image.DOColor(enable ? new Color(0, 0, 0, 0.75f) : Color.clear, 0.75f);
        
    }

    void CloseLastWindow()
    {

        if (WindowAnimator.WindowQueue.Count > 0 && WindowAnimator.WindowQueue.Last().CanCloseByPanel) WindowAnimator.WindowQueue.Last().FadeOut(true);
    }
}
