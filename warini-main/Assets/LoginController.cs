using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    public RawImage cameraDisplay; // Display for the camera feed
    public Button loginButton; // Button for triggering login
    private WebCamTexture webcamTexture;

    void Start()
    {
        // Initialize the front-facing camera
        StartCamera();
        
        // Attach login function to the button
        loginButton.onClick.AddListener(CaptureAndSendImage);
    }

    // Method to start the camera feed with front-facing camera preference
    private void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected!");
            return;
        }

        // Find the front-facing camera
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)  // Select front-facing camera
            {
                webcamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                break;
            }
        }

        // If no front-facing camera found, return
        if (webcamTexture == null)
        {
            Debug.LogError("No front camera found");
            return;
        }

        // Set the camera display texture and play the camera feed
        cameraDisplay.texture = webcamTexture;
        webcamTexture.Play();

        // Adjust the display rotation for correct orientation
        AdjustDisplayRotation();
    }

    // Adjust display rotation for the front camera preview
    private void AdjustDisplayRotation()
    {
        // Rotate 90 degrees counterclockwise (reverse clockwise)
        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -90);

        // Flip the display horizontally for front camera
        cameraDisplay.rectTransform.localScale = new Vector3(-1, 1, 1);

        // Adjust aspect ratio to fit the camera display size
        float aspectRatio = (float)webcamTexture.width / webcamTexture.height;
        cameraDisplay.rectTransform.sizeDelta = new Vector2(
            cameraDisplay.rectTransform.rect.height * aspectRatio, 
            cameraDisplay.rectTransform.rect.height
        );
    }

    // Capture the image and send it to the server
    public void CaptureAndSendImage()
    {
        StartCoroutine(LoginWithImage());
    }

    private IEnumerator LoginWithImage()
{
    yield return new WaitForEndOfFrame();

    Texture2D snap = new Texture2D(webcamTexture.width, webcamTexture.height);
    snap.SetPixels(webcamTexture.GetPixels());
    snap.Apply();

    byte[] imageBytes = snap.EncodeToJPG();

    WWWForm form = new WWWForm();
    form.AddBinaryData("image", imageBytes, "face.jpg", "image/jpeg");

    string url = "http://192.168.1.35:5000/login";
    using (UnityWebRequest www = UnityWebRequest.Post(url, form))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
            PlayerPrefs.SetString("MatchedName", response.matched_name);
            PlayerPrefs.SetString("TTSFileURL", response.tts_file); // Save TTS file URL

            SceneManager.LoadScene("WelcomeToWarini");
        }
        else
        {
            Debug.LogError("Login failed: " + www.error);
        }
    }
}

    // Ensure the camera starts when the scene or object is enabled
    void OnEnable()
    {
        if (webcamTexture != null && !webcamTexture.isPlaying)
        {
            webcamTexture.Play();
        }
    }

    // Ensure the camera stops when the scene or object is disabled
    void OnDisable()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}

// Class to parse the JSON response from the server
[System.Serializable]
public class LoginResponse
{
    public string message;
    public string matched_name;
    public string tts_file; // URL to the TTS audio file
}