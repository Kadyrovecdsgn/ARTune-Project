using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using System.IO;

public class ImmersalLogin : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField mailField;
    [SerializeField] private TMP_InputField passField;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text statusMessage;


    [Header("API Configuration")]
    private const string API_URL = "https://api.immersal.com/rest/v1/login";

    private const string TOKEN_FILE_NAME = "immersal_token.txt";
    private string tokenFilePath;

    private void Awake()
    {
        // Скрываем текстовое поле статуса, если оно есть
        if (statusMessage != null)
        {
            statusMessage.gameObject.SetActive(false);
        }

        // Привязываем метод к кнопке
        loginButton.onClick.AddListener(OnLoginClicked);

        // Путь к файлу с токеном во внутреннем хранилище приложения
        tokenFilePath = Path.Combine(Application.persistentDataPath, TOKEN_FILE_NAME);
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
        // Формируем тело запроса
        SDKLoginRequest loginRequest = new SDKLoginRequest
        {
            login = email,
            password = password
        };

        // Сериализуем объект в JSON
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

            // Проверка на сетевые или протокольные ошибки
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Login failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                ShowStatusMessage("Login failed: " + request.error, true);
            }
            else
            {
                // Парсим ответ
                string jsonResponse = request.downloadHandler.text;
                SDKLoginResult response = JsonUtility.FromJson<SDKLoginResult>(jsonResponse);

                // Проверяем, что ошибок нет и токен получен
                if (response != null && response.error == "none")
                {
                    // Сохраняем токен в файл
                    File.WriteAllText(tokenFilePath, response.token);
                    Debug.Log("Token successfully written to: " + tokenFilePath);

                    // Выводим сообщение об успешном входе
                    ShowStatusMessage("Login successful!", false);

                    // Запускаем корутину, которая через 3 секунды закроет окно
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

    /// <summary>
    /// Показываем сообщение пользователю
    /// </summary>
    private void ShowStatusMessage(string message, bool isError)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
            statusMessage.color = isError ? Color.red : Color.green;
            statusMessage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Закрываем окно логина через заданное время
    /// </summary>
    private IEnumerator CloseWindowAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        // Скрываем окно логина (или уничтожаем объект, по необходимости)
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
