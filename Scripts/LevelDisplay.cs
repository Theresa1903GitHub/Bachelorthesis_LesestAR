using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class LevelDisplay : MonoBehaviour
{
    public TMP_Text levelTMP; 
    // public Text levelText; 

    void Start()
    {
        UpdateLevelUI();
    }

    // Optional
    void Update()
    {
        UpdateLevelUI();
    }

    void UpdateLevelUI()
    {
        if (GameManager.Instance != null)
        {
            levelTMP.text =  GameManager.Instance.CurrentLevel.ToString();
            // levelText.text = "Level: " + GameManager.Instance.CurrentLevel;
        }
        else
        {
            Debug.LogWarning("GameManager.Instance ist null! Stelle sicher, dass der GameManager existiert.");
        }
    }
}
