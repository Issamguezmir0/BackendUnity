using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Android;
using UnityEngine.SceneManagement; // Import the SceneManagement library

public class CameraController : MonoBehaviour
{
    public RawImage cameraDisplay; // Assign in Unity for displaying the camera feed.
    public InputField userNameInput; // Assign in Unity for user name input.
    private WebCamTexture webcamTexture;

    void Start()
    {
        // Check for camera permissions on Android
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            StartCamera();
        }
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

        if (webcamTexture == null)
        {
            Debug.LogError("No front camera found");
            return;
        }

        // Set the camera display texture and play the camera feed
        webcamTexture.Play();
        cameraDisplay.texture = webcamTexture;

        // Rotate and mirror the display for front camera orientation
        AdjustDisplayRotation();
    }
    
    // Method to adjust display rotation and aspect ratio
    private void AdjustDisplayRotation()
    {
        // Rotate 90 degrees counterclockwise (reverse clockwise)
        cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -90);

        // Flip the display horizontally if using the front camera
        cameraDisplay.rectTransform.localScale = new Vector3(-1, 1, 1);

        // Adjust aspect ratio to fit the camera display size
        float aspectRatio = (float)webcamTexture.width / webcamTexture.height;
        cameraDisplay.rectTransform.sizeDelta = new Vector2(
            cameraDisplay.rectTransform.rect.height * aspectRatio, 
            cameraDisplay.rectTransform.rect.height
        );
    }

    // Method to capture the image and send it to the server
    public void CaptureAndSendImage()
    {
        if (!webcamTexture.isPlaying)
        {
            Debug.LogError("Camera is not active.");
            return;
        }
        Debug.Log("Capturing and sending image...");
        StartCoroutine(CaptureImage());
    }

private IEnumerator CaptureImage()
{
    yield return new WaitForEndOfFrame();

    // Capture a snapshot from the camera feed
    Texture2D snap = new Texture2D(webcamTexture.width, webcamTexture.height);
    snap.SetPixels(webcamTexture.GetPixels());
    snap.Apply();

    // Debug log to confirm image capture
    Debug.Log($"Image captured. Width: {snap.width}, Height: {snap.height}");

    // Encode the image to JPG format
    byte[] imageBytes = snap.EncodeToJPG();
    Debug.Log("Image encoded to JPG format. Byte size: " + imageBytes.Length);

    // Retrieve the username from the input field
    string userName = userNameInput.text;
    if (string.IsNullOrEmpty(userName))
    {
        Debug.LogError("User name is empty!");
        yield break;
    }
    Debug.Log("Username captured: " + userName);

    // Create a form to send the name and image
    WWWForm form = new WWWForm();
    form.AddField("name", userName);
    form.AddBinaryData("image", imageBytes, "face.jpg", "image/jpeg");

    // URL for your Flask register API
    string url = "http://192.168.1.35:5000/register_face";
    Debug.Log("Sending request to: " + url);

    using (UnityWebRequest www = UnityWebRequest.Post(url, form))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Image and name uploaded successfully!");
            SceneManager.LoadScene("Login");
        }
        else
        {
            Debug.LogError("Image upload failed: " + www.error);
            SceneManager.LoadScene("WelcomePage");
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
