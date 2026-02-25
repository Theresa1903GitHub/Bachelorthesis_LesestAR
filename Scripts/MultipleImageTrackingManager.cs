using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class MultipleImageTrackingManager : MonoBehaviour
{
    [Header("AR Prefabs")]
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();

    [Header("UI References")]
    [SerializeField] private TMP_Text HighscoreText;
    [SerializeField] public TMP_Text LevelText;

    private ARTrackedImageManager _trackedImageManager;
    private Dictionary<string, GameObject> _arObjects;
    private Coroutine resetAnimatorCoroutine = null;
    private Dictionary<string, Coroutine> hideCoroutines = new Dictionary<string, Coroutine>();

    public static string currentTargetSentence = null;
    public static string currentTrackedMarker = null;

    // mehrere Animatoren
    public static Animator[] activePrefabAnimators = null;

    private Dictionary<string, string> targetSentences = new Dictionary<string, string>()
    {
        // { "Testbild", "Das ist ein Test" },
        // { "Testbild2", "Wenn alle Kinder schlafen ist Theo wach" },
        { "Mia", "Lena" },
        { "Theo", "Theo"},
        { "Page1", "Lena und ihr kleiner Bruder Theo sind allein zu Hause" },
        { "Page2", "Plötzlich hören die Beiden ein lautes Geräusch"},
        { "Page3", "Sie steigen auf den Dachboden, um dort nachzusehen"},
        { "Page4", "Auf dem Boden finden sie ein großes, offenes Buch"},
        { "Page5", "Ein winziges Mädchen mit blonden Zöpfen sitzt darauf und weint"},
        { "Page6", "Ich kann meinen Bruder Hänsel nicht finden"},
        { "Page7", "Lena bietet der kleinen Gretel an, bei der Suche nach ihrem Bruder zu helfen"},
        { "Page8", "Gretel ist dankbar und zeigt den Kindern, wo sie Hänsel zuletzt gesehen hatte"},
        { "Page9", "Theo entdeckt eine Spur aus leuchtenden Buchstaben"},
        { "Page10", "Sie folgen der Buchstabenspur bis in die Küche"},
        { "Page11", "Hänsel sitzt unter einem Sieb gefangen und zittert vor Angst"},
        { "Page12", "Die Hexe steht vor ihm und rührt wild in ihrem Kessel"},
        { "Page13", "Lena und Theo überlegen sich einen Plan, um die Hexe zu überlisten"},
        { "Page14", "Wo habe ich nur schon wieder mein Hexenbuch liegen gelassen?"},
        { "Page15", "Theo streckt ihr das große Märchenbuch entgegen, welches er vom Dachboden mitgenommen hatte"},
        { "Page16", "Die Hexe beugt sich so tief darüber, dass ihre lange Zinkennase fast die Buchseite streift"},
        { "Page17", "Mit einem kräftigen Stoß schubst Lena die Hexe vorwärts in das Buch hinein"},
        { "Page18", "Ein goldener Wirbel zieht die Hexe zurück ins Märchen"},
        { "Page19", "Zusammen steigen die Vier die Leiter zum Dachboden hinauf"},
        { "Page20", "Hänsel greift Gretels Hand und so springen die beiden zurück in ihr Buch"}
    };

    private Dictionary<string, float> targetHighscores = new Dictionary<string, float>();
    private string lastTrackedMarker = null;

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        if (_trackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager fehlt!");
            return;
        }

        _trackedImageManager.trackedImagesChanged += OnImagesTrackedChanged;
        _arObjects = new Dictionary<string, GameObject>();

        foreach (var kvp in targetSentences) //KeyValuePair
        {
            float highscore = GameManager.Instance.GetHighscore(kvp.Value);
            targetHighscores[kvp.Key] = highscore;
        }

        SetupSceneElements();
    }

    private void OnDestroy()
    {
        if (_trackedImageManager != null)
            _trackedImageManager.trackedImagesChanged -= OnImagesTrackedChanged;
    }

    private void SetupSceneElements()
    {
        foreach (var prefab in prefabsToSpawn)
        {
            if (prefab == null) continue;

            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            arObject.name = prefab.name;
            arObject.SetActive(false);

            if (!_arObjects.ContainsKey(arObject.name))
                _arObjects.Add(arObject.name, arObject);
            else
                Debug.LogWarning($"Duplicate prefab name '{arObject.name}' detected. Skipping.");
        }
    }

    private void OnImagesTrackedChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            UpdateTrackedImage(trackedImage);

        foreach (var trackedImage in eventArgs.updated)
            UpdateTrackedImage(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
        {
            if (_arObjects.TryGetValue(trackedImage.referenceImage.name, out var arObject))
                arObject.SetActive(false);
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        string imageName = trackedImage.referenceImage.name;

        if (!_arObjects.TryGetValue(imageName, out var arObject))
        {
            Debug.LogWarning($"Kein Prefab für Bild {imageName} gefunden.");
            return;
        }

        if (trackedImage.trackingState == TrackingState.Limited ||
            trackedImage.trackingState == TrackingState.None)
        {
            if (!hideCoroutines.ContainsKey(imageName))
                hideCoroutines[imageName] =
                    StartCoroutine(HideAfterDelay(arObject, imageName, 0.5f));

            if (resetAnimatorCoroutine != null)
                StopCoroutine(resetAnimatorCoroutine);

            resetAnimatorCoroutine = StartCoroutine(ResetAnimatorAfterDelay(0.3f));
            return;
        }

        if (hideCoroutines.ContainsKey(imageName))
        {
            StopCoroutine(hideCoroutines[imageName]);
            hideCoroutines.Remove(imageName);
        }

        if (lastTrackedMarker != imageName)
        {
            RecordingCanvas.ClearRecognizedText();

            Stars stars = FindObjectOfType<Stars>();
            if (stars != null)
                stars.SetHighscoreInstant(targetSentences[imageName]);
        }

        arObject.SetActive(true);

        float smoothSpeed = 10f * Time.deltaTime;
        arObject.transform.position =
            Vector3.Lerp(arObject.transform.position, trackedImage.transform.position, smoothSpeed);
        arObject.transform.rotation =
            Quaternion.Slerp(arObject.transform.rotation, trackedImage.transform.rotation, smoothSpeed);

        if (targetSentences.TryGetValue(imageName, out string sentence))
        {
            currentTargetSentence = sentence;
            currentTrackedMarker = imageName;

            if (LevelText != null)
                LevelText.text = "Level " + GameManager.Instance.CurrentLevel;
        }

        // alle Animator-Komponenten holen
        activePrefabAnimators = arObject.GetComponentsInChildren<Animator>();

        lastTrackedMarker = imageName;
    }

    private IEnumerator HideAfterDelay(GameObject obj, string imageName, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        hideCoroutines.Remove(imageName);
    }

    private IEnumerator ResetAnimatorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        activePrefabAnimators = null;
        currentTargetSentence = null;
        currentTrackedMarker = null;
    }

    public static void PlayTargetSound(string imageName)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{imageName}");
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        else
            Debug.LogWarning($"Kein Sound gefunden für Marker: {imageName}");
    }
}
