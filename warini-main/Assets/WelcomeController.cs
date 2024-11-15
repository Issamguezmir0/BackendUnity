using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

public class WelcomeController : MonoBehaviour
{
    public Text welcomeText;
    public Button logoutButton;
    public AudioSource audioSource; // Add reference to the AudioSource component

    void Start()
    {
        // Set the welcome message using the matched name
        string matchedName = PlayerPrefs.GetString("MatchedName", "Guest");
        welcomeText.text = $"Welcome, {matchedName}, to our app!";

        // Load and play TTS audio
        string ttsFileURL = PlayerPrefs.GetString("TTSFileURL");
        if (!string.IsNullOrEmpty(ttsFileURL))
        {
            StartCoroutine(PlayTTSAudio(ttsFileURL));
        }

        // Set logout button action
        logoutButton.onClick.AddListener(Logout);
    }

    // Coroutine to download and play TTS audio
    private IEnumerator PlayTTSAudio(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load TTS audio: " + www.error);
            }
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("MatchedName");
        PlayerPrefs.DeleteKey("TTSFileURL");
        SceneManager.LoadScene("WelcomePage");
    }
}
