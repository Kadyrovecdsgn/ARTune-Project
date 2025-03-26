using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class ImmersalRegister : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField userNameField;  // Поле "Name"
    [SerializeField] private TMP_InputField mailField;      // Поле "Login" (email)
    [SerializeField] private TMP_InputField passField;      // Поле "Password"
    [SerializeField] private Button registerButton;         // Кнопка "Register"
    [SerializeField] private TMP_Text statusMessage;        // Поле для вывода статуса

    [Header("API Configuration")]
    private const string REGISTER_URL = "https://api.immersal.com/register";

    private void Awake()
    {
        // Скрываем текстовое поле статуса при старте (если оно есть)
        if (statusMessage != null)
        {
            statusMessage.gameObject.SetActive(false);
        }

        // Привязываем метод регистрации к кнопке
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    private void OnRegisterClicked()
    {
        // Считываем значения из полей ввода
        string userName = userNameField.text;  
        string email = mailField.text;         
        string password = passField.text;      

        // Простейшая валидация
        if (string.IsNullOrEmpty(userName) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            ShowStatusMessage("One of the required fields is empty!", true);
            return;
        }

        // Запускаем корутину для отправки запроса
        StartCoroutine(SendRegisterRequest(userName, email, password));
    }

    private IEnumerator SendRegisterRequest(string userName, string email, string password)
    {
        // Формируем тело запроса с учётом нужного порядка полей:
        // login → name → password
        SDKRegisterRequest registerRequest = new SDKRegisterRequest
        {
            login = email,
            name = userName,
            password = password
        };

        // Сериализуем объект в JSON
        string jsonBody = JsonUtility.ToJson(registerRequest);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        // Создаём WebRequest
        using (UnityWebRequest request = new UnityWebRequest(REGISTER_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            // Отключаем кнопку на время выполнения запроса
            registerButton.interactable = false;

            yield return request.SendWebRequest();

            // Включаем кнопку обратно
            registerButton.interactable = true;

            // Проверка на сетевые или протокольные ошибки
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Registration failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                ShowStatusMessage("Registration failed: " + request.error, true);
            }
            else
            {
                // Парсим ответ
                string jsonResponse = request.downloadHandler.text;
                SDKRegisterResult response = JsonUtility.FromJson<SDKRegisterResult>(jsonResponse);

                // Проверяем поле error
                if (response != null && response.error == "none")
                {
                    // Сообщаем об успешной регистрации
                    ShowStatusMessage($"Registration successful! User ID: {response.userId}", false);
                    Debug.Log("Received token: " + response.token);

                    // При необходимости можно закрыть или скрыть окно регистрации
                    // StartCoroutine(CloseWindowAfterDelay(3f));
                }
                else
                {
                    Debug.LogError("Registration failed or response is invalid.");
                    ShowStatusMessage("Registration failed or invalid response", true);
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
    /// При желании можно скрыть окно через некоторое время
    /// </summary>
    private IEnumerator CloseWindowAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        gameObject.SetActive(false);
    }

    [System.Serializable]
    private class SDKRegisterRequest
    {
        public string login;     // Username (email address)
        public string name;      // Name of the user
        public string password;  // Password
    }

    [System.Serializable]
    private class SDKRegisterResult
    {
        public string error;     // "none" при успешной регистрации
        public int userId;       // ID нового пользователя
        public string token;     // Токен при успешной регистрации
    }
}
