using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public System.Action OnDataReset;

    public int CurrentLevel = 1;
    public float CumulativeScore = 0f;
    public float CurrentProgress = 0f;
    public Dictionary<string, float> SentenceHighscores = new Dictionary<string, float>();

    // Nur noch die schwierigen Wörter
    public List<string> DifficultWords = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgressAndLevel();
            LoadHighscores();
            LoadDifficultWords();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Highscore Save/Load
    [System.Serializable]
    private class Serialization<TKey, TValue>
    {
        public List<TKey> keys;
        public List<TValue> values;

        public Serialization(Dictionary<TKey, TValue> dict)
        {
            keys = dict.Keys.ToList();
            values = dict.Values.ToList();
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
                dict[keys[i]] = values[i];
            return dict;
        }
    }

    public void SaveHighscores()
    {
        string json = JsonUtility.ToJson(new Serialization<string, float>(SentenceHighscores));
        PlayerPrefs.SetString("SentenceHighscores", json);
        PlayerPrefs.Save();
    }

    public void LoadHighscores()
    {
        string json = PlayerPrefs.GetString("SentenceHighscores", "");
        if (!string.IsNullOrEmpty(json))
            SentenceHighscores = JsonUtility.FromJson<Serialization<string, float>>(json).ToDictionary();
    }
    #endregion

    #region Save / Load Progress + Level
    public void SaveProgressAndLevel()
    {
        PlayerPrefs.SetFloat("GM_CurrentProgress", CurrentProgress);
        PlayerPrefs.SetFloat("GM_CumulativeScore", CumulativeScore);
        PlayerPrefs.SetInt("GM_CurrentLevel", CurrentLevel);
        SaveDifficultWords();
        PlayerPrefs.Save();
    }

    private void LoadProgressAndLevel()
    {
        CurrentProgress = PlayerPrefs.GetFloat("GM_CurrentProgress", 0f);
        CumulativeScore = PlayerPrefs.GetFloat("GM_CumulativeScore", 0f);
        CurrentLevel = PlayerPrefs.GetInt("GM_CurrentLevel", 1);
    }

    private void OnApplicationQuit() => SaveProgressAndLevel();
    private void OnApplicationPause(bool pause) { if (pause) SaveProgressAndLevel(); }
    #endregion

    #region Highscore / Progress Helpers
    public float GetHighscore(string sentence)
    {
        if (SentenceHighscores.TryGetValue(sentence, out float score))
            return score;
        return 0f;
    }

    public bool TryUpdateHighscore(string sentence, float newScore)
    {
        float oldScore = GetHighscore(sentence);
        if (newScore > oldScore)
        {
            SentenceHighscores[sentence] = newScore;
            SaveHighscores();
            return true;
        }
        return false;
    }

    public void AddProgress(float score)
    {
        CurrentProgress += score;
        if (CurrentProgress >= 100f)
        {
            CurrentProgress -= 100f;
            CurrentLevel++;
        }
        SaveProgressAndLevel();
    }
    #endregion

    #region Reset Button
    public void ResetAllData()
    {
        SentenceHighscores.Clear();
        SaveHighscores();

        CurrentProgress = 0f;
        CumulativeScore = 0f;
        CurrentLevel = 1;
        SaveProgressAndLevel();

        DifficultWords.Clear();
        SaveDifficultWords();

        Debug.Log("Alle Daten erfolgreich zurückgesetzt!");
        OnDataReset?.Invoke();

        Stars stars = FindObjectOfType<Stars>();
        if (stars != null)
        {
            stars.RefreshLocalHighscores();
            stars.ResetCurrentScore();
        }

        MultipleImageTrackingManager[] managers = FindObjectsOfType<MultipleImageTrackingManager>();
        foreach (var manager in managers)
            if (manager.LevelText != null) manager.LevelText.text = "Level 1";
    }

    public void ResetSceneData(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (SentenceHighscores.ContainsKey(sceneName))
            SentenceHighscores[sceneName] = 0f;
        else
            SentenceHighscores.Add(sceneName, 0f);

        SaveHighscores();

        CurrentProgress = 0f;
        CumulativeScore = 0f;
        CurrentLevel = 1;
        SaveProgressAndLevel();

        DifficultWords.Clear();
        SaveDifficultWords();

        Debug.Log("Daten für Szene " + sceneName + " zurückgesetzt!");
        OnDataReset?.Invoke();

        Stars stars = FindObjectOfType<Stars>();
        if (stars != null)
        {
            stars.RefreshLocalHighscores();
            stars.ResetCurrentScore();
        }

        MultipleImageTrackingManager[] managers = FindObjectsOfType<MultipleImageTrackingManager>();
        foreach (var manager in managers)
            if (manager.LevelText != null) manager.LevelText.text = "Level 1";
    }
    #endregion

    #region Difficult Words Save/Load
    public void SaveDifficultWords()
    {
        string json = JsonUtility.ToJson(new SerializationHelper(DifficultWords));
        PlayerPrefs.SetString("DifficultWords", json);
        PlayerPrefs.Save();
    }

    public void LoadDifficultWords()
    {
        if (!PlayerPrefs.HasKey("DifficultWords")) return;

        string json = PlayerPrefs.GetString("DifficultWords");
        var helper = JsonUtility.FromJson<SerializationHelper>(json);
        if (helper?.words != null)
            DifficultWords = helper.words;
    }

    [System.Serializable]
    private class SerializationHelper
    {
        public List<string> words;
        public SerializationHelper(List<string> list) { words = list; }
    }
    #endregion
}
