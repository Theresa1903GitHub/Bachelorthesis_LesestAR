using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResetButton : MonoBehaviour
{
    public Button button;               // Reset-Button
    public Button playButton;           // Play-Button

    [Header("Confirm Panel")]
    public GameObject confirmPanel;     // Panel
    public CanvasGroup confirmCanvas;   // Fade-In/Out
    public RectTransform confirmTransform;  // Scale-Animation

    public Button confirmYesButton;     // "Ja"
    public Button confirmNoButton;      // "Nein"

    [Header("Animations")]
    public float openFadeDuration = 0.25f;
    public float openScaleDuration = 0.25f;
    public float closeFadeDuration = 0.2f;
    public float closeScaleDuration = 0.2f;

    public float rotationDuration = 0.5f;
    public float fadeDuration = 0.3f;

    private Image buttonImage;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        buttonImage = button.GetComponent<Image>();

        // Panel deaktiviert halten
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        confirmYesButton.onClick.AddListener(ConfirmReset);
        confirmNoButton.onClick.AddListener(CancelReset);
    }

    private void Update()
    {
        if (playButton != null)
            button.interactable = playButton.interactable;
    }

    public void OnResetClicked()
    {
        if (!button.interactable) return;
        OpenConfirmPanel();
    }

    private void OpenConfirmPanel()
    {
        confirmPanel.SetActive(true);

        // Startzustand
        confirmCanvas.alpha = 0f;
        confirmTransform.localScale = new Vector3(0.7f, 0.7f, 1f);

        // Fade In
        confirmCanvas.DOFade(1f, openFadeDuration);

        // Scale Pop Up
        confirmTransform.DOScale(1f, openScaleDuration)
            .SetEase(Ease.OutBack);  // Pop-Up Effekt
    }

    private void CloseConfirmPanel()
    {
        // Fade Out
        confirmCanvas.DOFade(0f, closeFadeDuration);

        // Scale Down
        confirmTransform.DOScale(0.7f, closeScaleDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => confirmPanel.SetActive(false));
    }

    private void ConfirmReset()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllData();
            Debug.Log("Reset bestätigt!");

            AnimateButton();
        }

        CloseConfirmPanel();
    }

    private void CancelReset()
    {
        CloseConfirmPanel();
    }

    private void AnimateButton()
    {
        button.interactable = false;

        // Rotation
        button.transform.DORotate(new Vector3(0, 0, -360), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic);

        // Ausgrauen
        if (buttonImage != null)
        {
            Color targetColor = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0.5f);
            buttonImage.DOColor(targetColor, fadeDuration).SetDelay(rotationDuration);
        }
    }
}
