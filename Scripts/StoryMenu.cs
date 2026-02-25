using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryMenu : MonoBehaviour
{
    [System.Serializable]
    public class ButtonSlot
    {
        public Button button;
        public Image image;
        public Sprite normalSprite;
        public Sprite selectedSprite;
        public string sceneName;
    }

    public ButtonSlot[] buttons = new ButtonSlot[9];
    public Button playButton;

    private int selectedIndex = -1;
    private string selectedScene = null;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].button.onClick.AddListener(() => SelectButton(index));
        }

        if (playButton != null)
            playButton.interactable = false;

        UpdateVisuals();
    }

    void SelectButton(int index)
    {
        selectedIndex = index;
        selectedScene = buttons[index].sceneName;

        if (playButton != null)
            playButton.interactable = true;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == selectedIndex)
                buttons[i].image.sprite = buttons[i].selectedSprite;
            else
                buttons[i].image.sprite = buttons[i].normalSprite;
        }
    }

    public void PlaySelectedScene()
    {
        if (string.IsNullOrEmpty(selectedScene))
        {
            Debug.LogWarning("Keine Szene ausgewählt!");
            return;
        }

        SceneManager.LoadScene(selectedScene);
    }

    public string GetSelectedSceneName()
    {
        return selectedScene;
    }
}
