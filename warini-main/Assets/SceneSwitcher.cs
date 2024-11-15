using UnityEngine;
using UnityEngine.SceneManagement; // Import the SceneManagement library

public class SceneSwitcher : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Method to switch to the Register scene
    public void GoToRegister()
{
    Debug.Log("GoToRegister function called");
    SceneManager.LoadScene("Register");
}

public void GoToLogin()
{
    Debug.Log("GoToLogin function called");
    SceneManager.LoadScene("Login");
}
}
