using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using KKSpeech;

namespace KKSpeech {
	public class SpeechRecognitionLanguageDropdown : MonoBehaviour {

		private Dropdown dropdown;
		private List<LanguageOption> languageOptions;

		public Button Button;

		public Sprite ButtonNormal;
		public Sprite ButtonAttention;

		void Start () {
			dropdown = GetComponent<Dropdown>();
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			dropdown.ClearOptions();

			GameObject.FindObjectOfType<SpeechRecognizerListener>().
				onSupportedLanguagesFetched.
				AddListener(OnSupportedLanguagesFetched);

			SpeechRecognizer.GetSupportedLanguages();

			StartBlinkAnimation();
		}

private void StartBlinkAnimation() {
    // Warte 22 Sekunden
    DOVirtual.DelayedCall(22f, () =>
    {
        Image img = Button.GetComponent<Image>();
        img.sprite = ButtonAttention;

        Button.transform
            .DOScale(1.05f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    });
}

		public void GoToRecordingScene() {
			SceneManager.LoadScene("ExampleScene");
		}

		public void GoToMenuScene() {
			SceneManager.LoadScene("MainMenu");
		}

		void OnDropdownValueChanged(int index) {
			LanguageOption languageOption = languageOptions[index];

			SpeechRecognizer.SetDetectionLanguage(languageOption.id);
		}

		void OnSupportedLanguagesFetched(List<LanguageOption> languages) {
			List<Dropdown.OptionData> dropdownOptions = new List<Dropdown.OptionData>();

			foreach (LanguageOption langOption in languages) {
				dropdownOptions.Add(new Dropdown.OptionData(langOption.displayName));
			}

			dropdown.AddOptions(dropdownOptions);

			languageOptions = languages;
		} 

	}
}

