using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ClickableDifficultWordsList : MonoBehaviour, IPointerClickHandler
{
    [Header("Text-Referenz")]
    public TMP_Text text;

    [Header("Highlight Einstellungen")]
    [Tooltip("HEX-Farbe für das Klick-Highlight, z.B. EF8834")]
    public string clickColorHex = "EF8834";

    [Tooltip("Wie lange das Wort die Farbe behält (Sekunden)")]
    public float highlightDuration = 0.3f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (text == null) return;

        // Prüfen, ob Wort angeklickt wurde
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(
            text,
            eventData.position,
            eventData.pressEventCamera
        );

        if (wordIndex == -1) return;

        TMP_TextInfo textInfo = text.textInfo;
        string clickedWord = textInfo.wordInfo[wordIndex].GetWord();
        Debug.Log("DifficultWord angeklickt: " + clickedWord);

        // Highlight starten
        StartCoroutine(HighlightWord(wordIndex));

        // Vorlesen
        SpeakWord(clickedWord);
    }

    private void SpeakWord(string word)
    {
        // TTS
        Debug.Log("Vorlesen (Liste): " + word);
        AndroidTTS.Instance.Speak(word);
    }

    private IEnumerator HighlightWord(int wordIndex)
    {
        TMP_WordInfo wordInfo = text.textInfo.wordInfo[wordIndex];

        // Originaltext speichern
        string originalText = text.text;

        // Rich Text Highlight mit HEX-Farbe
        string highlightedWord = $"<color=#{clickColorHex}>{wordInfo.GetWord()}</color>";

        int startIndex = wordInfo.firstCharacterIndex;
        int length = wordInfo.characterCount;

        // Text ersetzen
        string newText = originalText.Substring(0, startIndex)
                         + highlightedWord
                         + originalText.Substring(startIndex + length);

        text.text = newText;

        // Layout aktualisieren
        text.ForceMeshUpdate();

        // Kurze Wartezeit
        yield return new WaitForSeconds(highlightDuration);

        // Originaltext zurücksetzen
        text.text = originalText;
        text.ForceMeshUpdate();
    }
}
