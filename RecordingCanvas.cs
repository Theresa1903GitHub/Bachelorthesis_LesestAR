using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using KKSpeech;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.Video;
using TMPro;

public class RecordingCanvas : MonoBehaviour
{
    public static HashSet<string> difficultWords = new HashSet<string>();

    public Button startRecordingButton;
    public Sprite Button;
    public Sprite ButtonStop;
    public Button HomeButton;
    public Button StarButton;
    public TMP_Text resultText;
    public TMP_Text difficultWordsText;

    public string correctWordColor;
    public string almostCorrectColor;
    public string wrongWordColor;

    private bool isContinuousRecording = false;
    private string fullTranscription = "";
    private string persistentHighlight = "";
    private bool resetColorsNextStart = false;

    private float startTime;
    private bool timerRunning = false;
    private bool hasUserStartedSpeaking = false;

    public static float lastRecognitionTime = -1f;
    public static float lastLevenshteinDistance = -1f;

    public static RecordingCanvas instance;

    private int missingWordCount = 0;
    private bool animationAlreadyTriggered = false;

    public VideoPlayer oneMissingWordVideoPlayer;
    public VideoPlayer multipleMissingWordsVideoPlayer;
    public VideoPlayer highAccuracyVideoPlayer;
    public VideoPlayer lowAccuracyVideoPlayer;

    private bool feedbackPlaying = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // ================= Speech =================
        if (SpeechRecognizer.ExistsOnDevice())
        {
            SpeechRecognizerListener listener = FindObjectOfType<SpeechRecognizerListener>();
            listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);
            listener.onAvailabilityChanged.AddListener(OnAvailabilityChange);
            listener.onErrorDuringRecording.AddListener(OnError);
            listener.onErrorOnStartRecording.AddListener(OnError);
            listener.onFinalResults.AddListener(OnFinalResult);
            listener.onPartialResults.AddListener(OnPartialResult);
            listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
            SpeechRecognizer.RequestAccess();
        }
        else
        {
            resultText.text = "Dein Gerät unterstützt keine Spracherkennung";
            startRecordingButton.enabled = false;
        }

        // ================= VIDEO PREPARE (NEU) =================
        PrepareVideo(oneMissingWordVideoPlayer);
        PrepareVideo(multipleMissingWordsVideoPlayer);
        PrepareVideo(highAccuracyVideoPlayer);
        PrepareVideo(lowAccuracyVideoPlayer);

        if (oneMissingWordVideoPlayer != null)
            oneMissingWordVideoPlayer.loopPointReached += OnFeedbackVideoFinished;
        if (multipleMissingWordsVideoPlayer != null)
            multipleMissingWordsVideoPlayer.loopPointReached += OnFeedbackVideoFinished;
        if (highAccuracyVideoPlayer != null)
            highAccuracyVideoPlayer.loopPointReached += OnFeedbackVideoFinished;
        if (lowAccuracyVideoPlayer != null)
            lowAccuracyVideoPlayer.loopPointReached += OnFeedbackVideoFinished;
    }

    // ================= VIDEO PREPARE =================
    void PrepareVideo(VideoPlayer vp)
    {
        if (vp == null) return;
        vp.playOnAwake = false;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop = true;
        vp.Prepare();
    }

    public static void ClearRecognizedText()
    {
        if (instance != null && instance.resultText != null)
        {
            instance.resultText.text = "";
            instance.persistentHighlight = "";
            instance.fullTranscription = "";
        }
    }

    public void GoToHomeMenu() => SceneManager.LoadScene("SetLanguage");
    public void GoToStarMenu() => SceneManager.LoadScene("Test");

    public void OnStartRecordingPressed()
    {
        lastRecognitionTime = -1f;
        lastLevenshteinDistance = -1f;
        animationAlreadyTriggered = false;

        if (MultipleImageTrackingManager.activePrefabAnimators != null)
            {
                foreach (Animator anim in MultipleImageTrackingManager.activePrefabAnimators)
                    anim.SetBool("isActivePrefab", false);
            }

        if (isContinuousRecording)
        {
            isContinuousRecording = false;
            SpeechRecognizer.StopIfRecording();

            startRecordingButton.GetComponentInChildren<Text>().text = "START";
            startRecordingButton.GetComponent<Image>().sprite = Button;

            EvaluateFinalResult();

            resultText.text = persistentHighlight;
            resetColorsNextStart = true;
            timerRunning = false;
        }
        else
        {
            isContinuousRecording = true;

            hasUserStartedSpeaking = false;
            fullTranscription = "";
            missingWordCount = 0;

            if (resetColorsNextStart)
            {
                persistentHighlight = "";
                resetColorsNextStart = false;
            }

            startRecordingButton.GetComponentInChildren<Text>().text = "STOP";
            startRecordingButton.GetComponent<Image>().sprite = ButtonStop;

            resultText.text =
                "<color=#FFFFFF>" +
                MultipleImageTrackingManager.currentTargetSentence +
                "</color>";

            SpeechRecognizer.StartRecording(true);

            startTime = Time.time;
            timerRunning = true;
        }
    }

    public void OnFinalResult(string result)
    {
        if (string.IsNullOrEmpty(result)) return;

        hasUserStartedSpeaking = true;
        string recognized = result.Trim();

        if (!string.IsNullOrEmpty(fullTranscription))
            fullTranscription += " ";
        fullTranscription += recognized;

        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;

        if (!string.IsNullOrEmpty(targetSentence))
        {
            persistentHighlight =
                HighlightMatchingWordsPersistent(fullTranscription, targetSentence);
            resultText.text = persistentHighlight;

            if (timerRunning)
            {
                lastRecognitionTime = Time.time - startTime;
                timerRunning = false;
            }

            lastLevenshteinDistance =
                LevenshteinDistance(fullTranscription, targetSentence);
        }

        if (!animationAlreadyTriggered &&
            IsSimilar(fullTranscription, targetSentence, 0.05f))
        {
            animationAlreadyTriggered = true;

            if (MultipleImageTrackingManager.activePrefabAnimators != null)
            {
                foreach (Animator anim in MultipleImageTrackingManager.activePrefabAnimators)
                    anim.SetBool("isActivePrefab", true);
            }

            string marker = MultipleImageTrackingManager.currentTrackedMarker;
            if (!string.IsNullOrEmpty(marker))
                MultipleImageTrackingManager.PlayTargetSound(marker);

            isContinuousRecording = false;
            SpeechRecognizer.StopIfRecording();

            startRecordingButton.GetComponentInChildren<Text>().text = "START";
            startRecordingButton.GetComponent<Image>().sprite = Button;

            resetColorsNextStart = true;
            timerRunning = false;
        }
    }

    void EvaluateFinalResult()
    {
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;
        if (string.IsNullOrEmpty(fullTranscription) || string.IsNullOrEmpty(targetSentence))
            return;

        if (missingWordCount == 1)
            PlayFeedbackVideo(oneMissingWordVideoPlayer);
        else if (missingWordCount >= 2)
            PlayFeedbackVideo(multipleMissingWordsVideoPlayer);
        else if (IsSimilar(fullTranscription, targetSentence, 0.10f))
            PlayFeedbackVideo(highAccuracyVideoPlayer);
        else if (IsSimilar(fullTranscription, targetSentence, 0.90f))
            PlayFeedbackVideo(lowAccuracyVideoPlayer);
    }

    public void OnPartialResult(string result)
    {
        string targetSentence = MultipleImageTrackingManager.currentTargetSentence;

        if (!hasUserStartedSpeaking)
        {
            resultText.text = "<color=#FFFFFF>" + targetSentence + "</color>";
            return;
        }

        if (string.IsNullOrEmpty(targetSentence))
        {
            resultText.text = result;
            return;
        }

        string[] partialWords =
            result.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        string[] previousWords =
            ExtractWordsFromColoredText(persistentHighlight).ToArray();

        List<string> combinedWords = new List<string>(previousWords);

        for (int i = previousWords.Length; i < partialWords.Length; i++)
            combinedWords.Add(partialWords[i]);

        persistentHighlight =
            HighlightMatchingWordsPersistent(
                string.Join(" ", combinedWords), targetSentence);

        resultText.text = persistentHighlight;
    }

    public void OnAvailabilityChange(bool available)
    {
        startRecordingButton.enabled = available;
        resultText.text = available ? "START" : "Speech Recognition not available";
    }

    public void OnAuthorizationStatusFetched(AuthorizationStatus status)
    {
        if (status == AuthorizationStatus.Authorized)
            startRecordingButton.enabled = true;
        else
        {
            startRecordingButton.enabled = false;
            resultText.text = "Cannot use Speech Recognition, status: " + status;
        }
    }

    public void OnEndOfSpeech()
    {
        if (isContinuousRecording)
            StartCoroutine(RestartRecordingWithDelay(0.01f));
    }

    IEnumerator RestartRecordingWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isContinuousRecording)
            SpeechRecognizer.StartRecording(true);
    }

    public void OnError(string error)
    {
        Debug.LogError(error);
        if (isContinuousRecording)
            StartCoroutine(RestartAfterError());
    }

    IEnumerator RestartAfterError()
    {
        yield return new WaitForSeconds(0.5f);
        if (isContinuousRecording)
            SpeechRecognizer.StartRecording(true);
    }

    // ================= HILFSMETHODEN =================

    void AlignWords(string[] spoken, string[] target,
        out List<string> alignedSpoken, out List<string> alignedTarget)
    {
        int n = spoken.Length;
        int m = target.Length;
        int[,] score = new int[n + 1, m + 1];
        int gap = -1;

        for (int i = 0; i <= n; i++) score[i, 0] = i * gap;
        for (int j = 0; j <= m; j++) score[0, j] = j * gap;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int match = score[i - 1, j - 1] +
                    ((Normalize(spoken[i - 1]) == Normalize(target[j - 1])) ? 2 : -1);
                int delete = score[i - 1, j] + gap;
                int insert = score[i, j - 1] + gap;
                score[i, j] = Mathf.Max(match, Mathf.Max(delete, insert));
            }
        }

        alignedSpoken = new List<string>();
        alignedTarget = new List<string>();
        int x = n, y = m;

        while (x > 0 || y > 0)
        {
            if (x > 0 && y > 0 &&
                score[x, y] == score[x - 1, y - 1] +
                ((Normalize(spoken[x - 1]) == Normalize(target[y - 1])) ? 2 : -1))
            {
                alignedSpoken.Insert(0, spoken[x - 1]);
                alignedTarget.Insert(0, target[y - 1]);
                x--; y--;
            }
            else if (x > 0 && score[x, y] == score[x - 1, y] + gap)
            {
                alignedSpoken.Insert(0, spoken[x - 1]);
                alignedTarget.Insert(0, "-");
                x--;
            }
            else
            {
                alignedSpoken.Insert(0, "-");
                alignedTarget.Insert(0, target[y - 1]);
                y--;
            }
        }
    }

    string HighlightMatchingWordsPersistent(string spoken, string target)
    {
        missingWordCount = 0;

        string[] spokenWords = spoken.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        string[] targetWords = target.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        AlignWords(spokenWords, targetWords,
            out List<string> alignedSpoken, out List<string> alignedTarget);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < alignedSpoken.Count; i++)
        {
            string sWord = alignedSpoken[i];
            string tWord = alignedTarget[i];

            if (sWord != "-")
            {
                string normS = Normalize(CleanWord(sWord));
                string normT = tWord == "-" ? "" : Normalize(CleanWord(tWord));
                string color;

                if (tWord == "-")
                    color = wrongWordColor;
                else if (normS == normT)
                    color = correctWordColor;
                else
                {
                    int distance = LevenshteinDistance(normS, normT);
                    float ratio = (float)distance / Mathf.Max(1, normT.Length);
                    float tolerance =
                        normT.Length <= 3 ? 0.05f :
                        normT.Length <= 7 ? 0.25f : 0.3f;

                    color = ratio <= tolerance ? almostCorrectColor : wrongWordColor;
                }

                sb.Append($"<color={color}>{CleanWord(sWord)}</color> ");
            }
            else
            {
                missingWordCount++;
                sb.Append($"<color=#808080>{CleanWord(tWord)}</color> ");
            }
        }

        return sb.ToString().TrimEnd();
    }

    string CleanWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return new string(input.Where(c => !char.IsControl(c)).ToArray()).Trim();
    }

    List<string> ExtractWordsFromColoredText(string coloredText)
    {
        List<string> words = new List<string>();
        if (string.IsNullOrEmpty(coloredText)) return words;

        var matches = Regex.Matches(coloredText, "<color=[^>]+>([^<]+)</color>");
        foreach (Match match in matches)
            words.Add(match.Groups[1].Value);

        return words;
    }

    string Normalize(string input)
    {
        string lower = input.ToLower();
        string noPunct = new string(lower.Where(c => !char.IsPunctuation(c)).ToArray());
        return Regex.Replace(noPunct, @"\s+", " ").Trim();
    }

    int LevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];
        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[s.Length, t.Length];
    }

    bool IsSimilar(string spoken, string expected, float tolerance = 0.1f)
    {
        string normSpoken = Normalize(spoken);
        string normExpected = Normalize(expected);
        int distance = LevenshteinDistance(normSpoken, normExpected);
        int maxDistance = Mathf.CeilToInt(normExpected.Length * tolerance);
        return distance <= maxDistance;
    }

    void PlayFeedbackVideo(VideoPlayer vp)
    {
        if (feedbackPlaying || vp == null) return;
        feedbackPlaying = true;
        StartCoroutine(PlayPreparedVideo(vp));
    }

    IEnumerator PlayPreparedVideo(VideoPlayer vp)
    {
        while (!vp.isPrepared)
            yield return null;

        vp.Stop();
        vp.time = 0;
        vp.Play();
    }

    void OnFeedbackVideoFinished(VideoPlayer vp)
    {
        feedbackPlaying = false;
    }

    public static void AddDifficultWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        string normalized = word.ToLower().Trim();

        if (difficultWords.Add(normalized))
        {
            if (instance == null || instance.difficultWordsText == null) return;
            instance.difficultWordsText.text = string.Join("\n", difficultWords);
        }
    }
}
