using UnityEngine;
using DG.Tweening;

public class TogglePanel : MonoBehaviour
{
    public GameObject panel;
    public Transform buttonTransform;
    public CanvasGroup canvasGroup;

    public float pressedScale = 0.92f;
    public float tweenDuration = 0.1f;

    public float panelTweenDuration = 0.25f;
    public float openScale = 1f;
    public float closedScale = 0.95f;

    private bool isOpen;

    private void Awake()
    {
        if (canvasGroup == null && panel != null)
            canvasGroup = panel.GetComponent<CanvasGroup>();
    }

    // Button 1 → Toggle
    public void Toggle()
    {
        PlayClickFeedback();

        if (isOpen)
            ClosePanel();
        else
            OpenPanel();
    }

    // Button 2 → Nur schließen
    public void ClosePanel()
    {
        if (!isOpen) return;

        isOpen = false;

        panel.transform.DOKill();
        canvasGroup.DOKill();

        canvasGroup
            .DOFade(0f, panelTweenDuration);

        panel.transform
            .DOScale(closedScale, panelTweenDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
    }

    public void OpenPanel()
    {
        isOpen = true;

        panel.SetActive(true);

        panel.transform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.one * closedScale;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        canvasGroup
            .DOFade(1f, panelTweenDuration);

        panel.transform
            .DOScale(openScale, panelTweenDuration)
            .SetEase(Ease.OutBack);
    }

    private void PlayClickFeedback()
    {
        if (buttonTransform == null) return;

        buttonTransform.DOKill();

        buttonTransform
            .DOScale(pressedScale, tweenDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                buttonTransform
                    .DOScale(1f, tweenDuration)
                    .SetEase(Ease.OutBack);
            });
    }
}
