using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

public class JobsList : MonoBehaviour
{
    //[SerializeField] private Button fetchJobsButton;

    private const string LIST_URL = "https://api.immersal.com/list";
    private const string JOBS_FILE_NAME = "jobslist.json";
    private string jobsFilePath;

    private void Awake()
    {
        jobsFilePath = Path.Combine(Application.persistentDataPath, JOBS_FILE_NAME);

        //if (fetchJobsButton != null)
        //    fetchJobsButton.onClick.AddListener(OnFetchJobsClicked);
    }

    private void Start()
    {
        StartCoroutine(AutoRefreshCoroutine());
    }

    private void OnFetchJobsClicked()
    {
        string token = TokenManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token not found! Please login first.");
            return;
        }
        StartCoroutine(FetchJobs(token));
    }

    private IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            string token = TokenManager.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                yield return StartCoroutine(FetchJobs(token));
            }
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator FetchJobs(string token)
    {
        ListJobsRequest requestData = new ListJobsRequest { token = token };
        string jsonPayload = JsonUtility.ToJson(requestData);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(LIST_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("List Jobs request failed: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("List Jobs response: " + jsonResponse);

                JobsListData newData = JsonUtility.FromJson<JobsListData>(jsonResponse);
                if (newData == null || newData.error != "none")
                {
                    Debug.LogError("Error in response or invalid data!");
                    yield break;
                }

                bool shouldSave = false;
                if (!File.Exists(jobsFilePath))
                {
                    shouldSave = true;
                }
                else
                {
                    string oldJson = File.ReadAllText(jobsFilePath);
                    JobsListData oldData = JsonUtility.FromJson<JobsListData>(oldJson);

                    if (newData.count != oldData.count || newData.jobs.Length != oldData.jobs.Length)
                    {
                        shouldSave = true;
                    }
                }

                if (shouldSave)
                {
                    File.WriteAllText(jobsFilePath, jsonResponse);
                    Debug.Log("Jobs list updated in file: " + jobsFilePath);

                    MapCardGenerator generator = FindObjectOfType<MapCardGenerator>();
                    if (generator != null)
                    {
                        generator.RefreshMapCards();
                    }
                }
            }
        }
    }
    //Удаление файла со списком карт после выхода из аккаунта
    public void ClearJobsList()
    {
        if (File.Exists(jobsFilePath))
        {
            File.Delete(jobsFilePath);
            Debug.Log("Jobs list file deleted: " + jobsFilePath);
        }
    }

    [System.Serializable]
    private class ListJobsRequest
    {
        public string token;
    }

    [System.Serializable]
    public class JobsListData
    {
        public string error;
        public int count;
        public JobData[] jobs;
    }

    [System.Serializable]
    public class JobData
    {
        public int id;
        public int type;
        public string version;
        public int creator;
        public int size;
        public string status;
        public int errno;
        public int privacy;
        public string name;
        public float latitude;
        public float longitude;
        public float altitude;
        public string created;
        public string modified;
        public string sha256_al;
        public string sha256_sparse;
        public string sha256_dense;
        public string sha256_tex;
    }
}
