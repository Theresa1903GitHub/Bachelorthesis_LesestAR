using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SettingsMenu : MonoBehaviour
{
    [Header("space between menu items")]
    [SerializeField] Vector2 spacing;

    [Space]
    [Header("Main button rotation")]
    [SerializeField] float rotationDuration;
    [SerializeField] Ease rotationEase;

    [Space]
    [Header("Animation")]
    [SerializeField] float expandDuration;
    [SerializeField] float collapseDuration;
    [SerializeField] Ease expandEase;
    [SerializeField] Ease collapseEase;

    [Space]
    [Header("Fading")]
    [SerializeField] float expandFadeDuration;
    [SerializeField] float collapseFadeDuration;

    [Space]
    [Header("Volume Panel Settings")]
    [SerializeField] GameObject volumePanel;
    [SerializeField] float volumePanelFadeDuration;
    [SerializeField] Ease volumePanelEase = Ease.OutQuad;

    Button mainButton;
    SettingsMenuItem[] menuItems;
    bool isExpanded = false;
    bool volumePanelVisible = false;

    Vector2 mainButtonPosition;
    int itemsCount;

    void Start()
    {
        itemsCount = transform.childCount - 1;
        menuItems = new SettingsMenuItem[itemsCount];

        for (int i = 0; i < itemsCount; i++)
        {
            menuItems[i] = transform.GetChild(i + 1).GetComponent<SettingsMenuItem>();
        }

        mainButton = transform.GetChild(0).GetComponent<Button>();
        mainButton.onClick.AddListener(ToggleMenu);
        mainButton.transform.SetAsLastSibling();

        mainButtonPosition = mainButton.transform.position;
        ResetPositions();

        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    void ResetPositions()
    {
        for (int i = 0; i < itemsCount; i++)
        {
            menuItems[i].trans.position = mainButtonPosition;
        }
    }

    void ToggleMenu()
    {
        isExpanded = !isExpanded;

        if (isExpanded)
        {
            // Menü öffnen
            for (int i = 0; i < itemsCount; i++)
            {
                menuItems[i].trans
                    .DOMove(mainButtonPosition + spacing * (i + 1), expandDuration)
                    .SetEase(expandEase);
                menuItems[i].img
                    .DOFade(1f, expandFadeDuration)
                    .From(0f);
            }
        }
        else
        {
            // Menü schließen
            for (int i = 0; i < itemsCount; i++)
            {
                menuItems[i].trans
                    .DOMove(mainButtonPosition, collapseDuration)
                    .SetEase(collapseEase);
                menuItems[i].img
                    .DOFade(0f, collapseFadeDuration);
            }

            // VolumePanel weich ausblenden, wenn Menü geschlossen wird
            if (volumePanelVisible)
                HideVolumePanel();
        }

        // Hauptbutton rotieren
        mainButton.transform
            .DORotate(Vector3.forward * 360f, rotationDuration)
            .SetEase(rotationEase)
            .SetRelative(true);
    }

    public void OnItemClick(int index)
    {
        switch (index)
        {
            case 0: // Volume button
                Debug.Log("Volume button clicked");

                if (!volumePanelVisible)
                    ShowVolumePanel();
                else
                    HideVolumePanel();
                break;

            case 1: // Info button
                Debug.Log("Info");
                break;
        }
    }

    void ShowVolumePanel()
    {
        if (volumePanel == null) return;

        volumePanelVisible = true;
        volumePanel.SetActive(true);

        CanvasGroup cg = volumePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, volumePanelFadeDuration)
              .SetEase(volumePanelEase);
        }
    }

    void HideVolumePanel()
    {
        if (volumePanel == null) return;

        volumePanelVisible = false;

        CanvasGroup cg = volumePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.DOFade(0f, volumePanelFadeDuration)
              .SetEase(volumePanelEase)
              .OnComplete(() => volumePanel.SetActive(false));
        }
        else
        {
            volumePanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        mainButton.onClick.RemoveListener(ToggleMenu);
    }
}
