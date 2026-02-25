using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject tts;
#endif

    private static AndroidTTS instance;
    public static AndroidTTS Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitTTS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitTTS()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        tts = new AndroidJavaObject(
            "android.speech.tts.TextToSpeech",
            activity,
            null
        );
#endif
    }

   public void Speak(string text)
{
#if UNITY_ANDROID && !UNITY_EDITOR
    if (tts == null) return;

    float volume = PlayerPrefs.GetFloat("soundVolume", 1f);

    using (AndroidJavaObject paramsBundle = new AndroidJavaObject("android.os.Bundle"))
    {
        paramsBundle.Call("putFloat", "volume", volume);

        tts.Call<int>(
            "speak",
            text,
            0,
            paramsBundle,
            null
        );
    }
#else
    Debug.Log("TTS (Editor): " + text);
#endif
}
}
