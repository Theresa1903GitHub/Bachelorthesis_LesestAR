using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CollectingStars : MonoBehaviour
{
    public GameObject[] starIcons;

    [Header("Connect-the-Dots Lines")]
    public GameObject[] connectingLines; // Länge = starIcons.Length - 1

    [Header("Pop & Fade Settings")]
    public float popDuration = 0.5f;      // Dauer Stern-Pop
    public float popDelay = 1f;         // Zeit zwischen Sternen
    public float rotationAmount = 15f;

    [Header("Line Settings")]
    public float lineDelay = 0.2f;        // Zeit nach Stern-Pop bis Linie startet
    public float lineFadeDuration = 0.5f; // Dauer Linien-Fade

    [Header("Random Rotation")]
    public Vector2 randomZRotation = new Vector2(-25f, 25f);

    [Header("Glitzereffekt")]
    public float sparkleDuration = 2f;
    public float sparkleIntensity = 0.2f;

    private int lastLevel = -1;

    public void GoToHomeMenu() => SceneManager.LoadScene("ExampleScene");

    void Start()
    {
        // Sterne initial ausblenden
        foreach (var star in starIcons)
            star.SetActive(false);

        // Linien initial vorbereiten
        if (connectingLines != null)
        {
            foreach (var line in connectingLines)
            {
                line.SetActive(false);

                CanvasGroup cg = line.GetComponent<CanvasGroup>();
                if (cg == null) cg = line.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
        }
    }

    void Update()
    {
        int currentLevel = GameManager.Instance.CurrentLevel;

        if (currentLevel > lastLevel)
        {
            AnimateStars(currentLevel);
            lastLevel = currentLevel;
        }
    }

    // --------------------------------------------------
    // Sterne animieren + Linien sanft einblenden
    // --------------------------------------------------
    private void AnimateStars(int level)
    {
        int starsToShow = Mathf.Clamp(level, 0, starIcons.Length);

        for (int i = 0; i < starsToShow; i++)
        {
            GameObject star = starIcons[i];
            int index = i; // wichtig für Closure

            if (star.activeSelf)
                continue;

            star.SetActive(true);

            // Zufällige leichte Rotation
            float randomRotationZ = Random.Range(randomZRotation.x, randomZRotation.y);
            star.transform.localRotation = Quaternion.Euler(0, 0, randomRotationZ);

            // Startzustand
            star.transform.localScale = Vector3.zero;

            CanvasGroup cg = star.GetComponent<CanvasGroup>();
            if (cg == null) cg = star.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(index * popDelay);

            seq.Append(cg.DOFade(1f, popDuration));
            seq.Join(
                star.transform
                    .DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack)
            );
            seq.Join(
                star.transform
                    .DORotate(
                        new Vector3(0, 0, randomRotationZ + rotationAmount),
                        popDuration
                    )
                    .SetEase(Ease.OutBack)
            );

            seq.OnComplete(() =>
            {
                // Glitzereffekt Stern
                star.transform
                    .DOPunchScale(
                        Vector3.one * sparkleIntensity,
                        sparkleDuration,
                        1,
                        0.5f
                    )
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);

                // ----------------------------
                // Linie sanft einblenden
                // ----------------------------
                if (index > 0 && connectingLines != null && index - 1 < connectingLines.Length)
                {
                    GameObject line = connectingLines[index - 1];
                    line.SetActive(true);

                    CanvasGroup lineCg = line.GetComponent<CanvasGroup>();
                    if (lineCg == null) lineCg = line.AddComponent<CanvasGroup>();

                    lineCg.alpha = 0f;

                    // Scale-Start (von links nach rechts)
                    Vector3 originalScale = line.transform.localScale;
                    line.transform.localScale = new Vector3(0f, originalScale.y, originalScale.z);

                    DOVirtual.DelayedCall(lineDelay, () =>
                    {
                        Sequence lineSeq = DOTween.Sequence();
                        lineSeq.Append(
                            lineCg.DOFade(1f, lineFadeDuration)
                        );
                        lineSeq.Join(
                            line.transform
                                .DOScaleX(originalScale.x, lineFadeDuration)
                                .SetEase(Ease.OutSine)
                        );
                    });
                }
            });
        }
    }
}
