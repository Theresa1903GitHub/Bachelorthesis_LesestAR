using UnityEngine;
using UnityEngine.EventSystems; // für IPointerClickHandler + PointerEventData
using TMPro;

public class ClickableWordHandler : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text text;
    public TMP_Text difficultWordsDisplay;

    private void Start()
    {
        if (difficultWordsDisplay != null && GameManager.Instance != null)
        {
            RefreshDifficultWordsDisplay();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(
            text, eventData.position, eventData.pressEventCamera);

        if (wordIndex == -1) return;

        string clickedWord = text.textInfo.wordInfo[wordIndex].GetWord();
        Debug.Log("Angeklicktes Wort: " + clickedWord);

        ToggleDifficultWord(clickedWord);
        // SpeakWord(clickedWord);
    }

    private void ToggleDifficultWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        string normalized = word.ToLower().Trim();

        if (GameManager.Instance.DifficultWords.Contains(normalized))
            GameManager.Instance.DifficultWords.Remove(normalized);
        else
            GameManager.Instance.DifficultWords.Add(normalized);

        GameManager.Instance.SaveDifficultWords();
        RefreshDifficultWordsDisplay();
    }

    private void RefreshDifficultWordsDisplay()
    {
        if (difficultWordsDisplay == null || GameManager.Instance == null) return;

        difficultWordsDisplay.text = string.Join("\n", GameManager.Instance.DifficultWords);
    }
}
