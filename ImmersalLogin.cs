using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using Immersal;

public class ImmersalLogin : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField mailField;
    [SerializeField] private TMP_InputField passField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TMP_Text statusMessage;

    [Header("API Configuration")]
    private const string API_URL = "https://api.immersal.com/rest/v1/login";

    private void Awake()
    {
        if (statusMessage != null)
        {
            statusMessage.gameObject.SetActive(false);
        }

        loginButton.onClick.AddListener(OnLoginClicked);
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
    }

    private void OnLoginClicked()
    {
        string email = mailField.text;
        string password = passField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatusMessage("Email or password is empty!", true);
            return;
        }

        StartCoroutine(SendLoginRequest(email, password));
    }

    private IEnumerator SendLoginRequest(string email, string password)
    {
        SDKLoginRequest loginRequest = new SDKLoginRequest
        {
            login = email,
            password = password
        };

        string jsonBody = JsonUtility.ToJson(loginRequest);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            loginButton.interactable = false;

            yield return request.SendWebRequest();

            loginButton.interactable = true;

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Login failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                ShowStatusMessage("Login failed: " + request.error, true);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                SDKLoginResult response = JsonUtility.FromJson<SDKLoginResult>(jsonResponse);

                if (response != null && response.error == "none")
                {
                    TokenManager.SetToken(response.token);
                    if (ImmersalSDK.Instance != null)
                    {
                        ImmersalSDK.Instance.developerToken = response.token;
                        ImmersalSDK.Instance.ValidateUser();
                    }
                    ShowStatusMessage("Login successful!", false);
                    StartCoroutine(CloseWindowAfterDelay(3f));
                }
                else
                {
                    Debug.LogError("Failed to parse login response or login failed");
                    ShowStatusMessage("Login failed or token not received", true);
                }
            }
        }
    }

    private void OnLogoutClicked()
    {
        if (ImmersalSDK.Instance != null)
        {
            ImmersalSDK.Instance.developerToken = null;
            ImmersalSDK.Instance.RestartSdk();
        }
        TokenManager.SetToken(null);
        ShowStatusMessage("Logged out successfully!", false);
        gameObject.SetActive(true);
    }

    private void ShowStatusMessage(string message, bool isError)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
            statusMessage.color = isError ? Color.red : Color.green;
            statusMessage.gameObject.SetActive(true);
        }
    }

    private IEnumerator CloseWindowAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        gameObject.SetActive(false);
    }

    [System.Serializable]
    private class SDKLoginRequest
    {
        public string login;
        public string password;
    }

    [System.Serializable]
    private class SDKLoginResult
    {
        public string error;
        public string token;
    }
}
