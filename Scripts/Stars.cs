using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using DG.Tweening;
using UnityEngine.Video;

public class Stars : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private Text resultText;
    [SerializeField] private TMP_Text Level;
    [SerializeField] private TMP_Text HighscoreText;
    [SerializeField] private Image LevelImage;

    [Header("Animation Settings")]
    [SerializeField] private float progressAnimationDuration = 1.2f;
    [SerializeField] private float punchScale = 0.25f;
    [SerializeField] private float punchDuration = 0.5f;

    [Header("Level-Up Animation")]
    [SerializeField] private float levelUpScale = 1.3f;
    [SerializeField] private float levelUpDuration = 0.4f;

    [Header("Highscore Colors")]
    [SerializeField] private Color highscoreStartColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color highscoreEndColor = new Color(1f, 0.55f, 0.1f);

    [Header("Effects")]
    [SerializeField] private GameObject konfettiObject;
    [SerializeField] private VideoPlayer konfettiPlayer;

    private float currentProgress = 0f;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDataReset += RefreshUIAfterReset;
    }

    private void RefreshUIAfterReset()
    {
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;

        if (!string.IsNullOrEmpty(targetSentence))
            UpdateHighscoreUI(targetSentence);

        ResetCurrentScore();

        currentProgress = GameManager.Instance.CurrentProgress;

        if (ProgressBar != null)
            ProgressBar.value = currentProgress / 100f;

        if (Level != null)
            Level.text = "Level " + GameManager.Instance.CurrentLevel;
    }

    void Start()
    {
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;

        if (!string.IsNullOrEmpty(targetSentence))
            UpdateHighscoreUI(targetSentence);

        if (ProgressBar != null)
            ProgressBar.value = Mathf.Clamp01(GameManager.Instance.CurrentProgress / 100f);

        currentProgress = GameManager.Instance.CurrentProgress;

        if (Level != null)
            Level.text = "Level " + GameManager.Instance.CurrentLevel;
    }

void Update()
{
    if (RecordingCanvas.lastRecognitionTime > 0)
    {
        float timeTaken = RecordingCanvas.lastRecognitionTime;
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;
        if (string.IsNullOrEmpty(targetSentence))
            return;

        int wordCount = CountWords(targetSentence);
        float lixValue = CalculateLIX(targetSentence); // 0–100

        // Erwartete Zeit (nur Referenz)
        float expectedTime = wordCount * 0.7f;

        // ---------------- ACCURACY (0–1)
        float distance = RecordingCanvas.lastLevenshteinDistance;
        float maxLen = Mathf.Max(1, targetSentence.Length);
        float accuracy01 = Mathf.Clamp01(1f - (distance / maxLen));

        // ---------------- SPEED (0–1)
        float speed01 = Mathf.Clamp01(expectedTime / Mathf.Max(timeTaken, 0.1f));

        // Speed nur wirksam, wenn Accuracy hoch genug ist (ab 60%)
        float speedWeight = Mathf.InverseLerp(0.6f, 0.9f, accuracy01); // 0 bei <60%, 1 bei >90%
        float effectiveSpeed01 = speed01 * speedWeight;

        // ---------------- BASE PERFORMANCE (0–1)
        float base01 =
            accuracy01 * 0.6f +
            effectiveSpeed01 * 0.4f;

        // ---------------- LIX FACTOR (1–2)
        float lixFactor = Mathf.Lerp(1f, 2f, Mathf.Clamp01(lixValue / 100f));

        // ---------------- FINAL SCORE (not clamped)
        float currentScore = base01 * 100f * lixFactor;

        if (resultText != null)
            resultText.text = currentScore.ToString("F2");

        float oldBest = GameManager.Instance.GetHighscore(targetSentence);

        if (currentScore > oldBest)
        {
            float diff = currentScore - oldBest;
            GameManager.Instance.TryUpdateHighscore(targetSentence, currentScore);
            AnimateHighscoreUI(currentScore);
            PlayKonfetti();
            AddScoreToProgress(diff);
        }

        UpdateHighscoreUI(targetSentence);

        if (Level != null)
            Level.text = "Level " + GameManager.Instance.CurrentLevel;

        RecordingCanvas.lastRecognitionTime = -1f;
    }
}



    public void SetHighscoreInstant(string sentence)
    {
        if (HighscoreText == null || string.IsNullOrEmpty(sentence))
            return;

        float best = GameManager.Instance.GetHighscore(sentence);
        HighscoreText.text = $"{best:F2}";

        ResetCurrentScore();

        currentProgress = GameManager.Instance.CurrentProgress;

        if (ProgressBar != null)
            ProgressBar.value = currentProgress / 100f;
    }

    public void ResetCurrentScore()
    {
        if (resultText != null)
            resultText.text = "0.00";
    }

    void AddScoreToProgress(float score)
    {
        float targetProgress = currentProgress + score;

        if (targetProgress >= 100f)
        {
            float overflow = targetProgress - 100f;
            AnimateProgress(100f);

            DOVirtual.DelayedCall(progressAnimationDuration, () =>
            {
                AnimateLevelUp();
                GameManager.Instance.CurrentLevel++;
                currentProgress = overflow;
                ProgressBar.value = 0f;
                AnimateProgress(currentProgress);
                GameManager.Instance.CurrentProgress = currentProgress;
            });
        }
        else
        {
            currentProgress = targetProgress;
            AnimateProgress(currentProgress);
            GameManager.Instance.CurrentProgress = currentProgress;
        }

        GameManager.Instance.CumulativeScore += score;
    }

    void AnimateProgress(float target)
    {
        if (ProgressBar == null)
            return;

        float targetValue = Mathf.Clamp01(target / 100f);
        ProgressBar.DOValue(targetValue, progressAnimationDuration).SetEase(Ease.OutQuad);
    }

    void AnimateLevelUp()
    {
        if (LevelImage == null)
            return;

        LevelImage.transform.DOKill();
        LevelImage.transform.DOScale(levelUpScale, levelUpDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                LevelImage.transform.DOScale(1f, levelUpDuration).SetEase(Ease.InBack);
            });
    }

    void UpdateHighscoreUI(string sentence)
    {
        if (HighscoreText == null)
            return;

        float best = GameManager.Instance.GetHighscore(sentence);
        HighscoreText.text = $"{best:F2}";
    }

    void AnimateHighscoreUI(float newScore)
    {
        if (HighscoreText == null)
            return;

        HighscoreText.text = $"{newScore:F2}";
        HighscoreText.color = highscoreStartColor;

        HighscoreText.transform.DOPunchScale(
            Vector3.one * punchScale,
            punchDuration,
            6,
            0.8f
        ).OnComplete(() =>
        {
            HighscoreText.DOColor(highscoreEndColor, 0.4f);
        });
    }

    public static float CalculateLIX(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0f;

        string[] words = Regex.Matches(text, @"\b\w+\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();

        int wordCount = words.Length;
        int sentenceCount = Regex.Matches(text, @"[.!?]").Count;
        if (sentenceCount == 0) sentenceCount = 1;

        int longWords = words.Count(w => w.Length > 6);

        float lix = (float)wordCount / sentenceCount
            + (float)longWords * 100f / wordCount;

        return Mathf.Clamp(lix, 0f, 100f);
    }

    int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return Regex.Matches(text, @"\b\w+\b").Count;
    }

    void PlayKonfetti()
    {
        if (konfettiObject == null || konfettiPlayer == null)
            return;

        konfettiObject.SetActive(true);

        konfettiPlayer.Stop();
        konfettiPlayer.time = 0;
        konfettiPlayer.Play();

        konfettiPlayer.loopPointReached += (_) =>
        {
            konfettiObject.SetActive(false);
        };
    }

    public void RefreshLocalHighscores()
    {
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;

        if (!string.IsNullOrEmpty(targetSentence))
            UpdateHighscoreUI(targetSentence);
    }
}
