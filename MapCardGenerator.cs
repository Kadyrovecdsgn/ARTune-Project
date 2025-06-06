using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Immersal;

public class MapCardGenerator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform mapListPanel;

    [Header("Card Settings")]
    [SerializeField] private Sprite mapCardBg;

    private const string JOBS_FILE_NAME = "jobslist.json";
    private string jobsFilePath;

    private void Awake()
    {
        jobsFilePath = Path.Combine(Application.persistentDataPath, JOBS_FILE_NAME);
    }

    private void Start()
    {
        string token = TokenManager.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            ImmersalSDK sdk = ImmersalSDK.Instance;
            if (sdk != null)
            {
                sdk.developerToken = token;
                sdk.ValidateUser();
                Debug.Log("Вход выполнен успешно (токен передан).");
            }
            else
            {
                Debug.LogError("ImmersalSDK instance не найден в сцене.");
            }
        }
        else
        {
            Debug.LogError("Токен не найден. Пожалуйста, выполните вход.");
        }

        GenerateMapCards();
    }

    public void RefreshMapCards()
    {
        foreach (Transform child in mapListPanel)
        {
            Destroy(child.gameObject);
        }
        GenerateMapCards();
    }

    private void GenerateMapCards()
    {
        if (!File.Exists(jobsFilePath))
        {
            Debug.LogError("Jobs file not found! Please fetch jobs first.");
            return;
        }

        string json = File.ReadAllText(jobsFilePath);
        JobsListData data = JsonUtility.FromJson<JobsListData>(json);
        if (data == null || data.jobs == null)
        {
            Debug.LogError("Invalid jobs data!");
            return;
        }

        foreach (JobData job in data.jobs)
        {
            CreateMapCard(job);
        }
    }

    private void CreateMapCard(JobData job)
    {
        GameObject mapCardGO = new GameObject("mapCard", typeof(RectTransform));
        RectTransform rt = mapCardGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(925, 200);

        mapCardGO.transform.SetParent(mapListPanel, false);

        Image mapCardImage = mapCardGO.AddComponent<Image>();
        Button mapCardButton = mapCardGO.AddComponent<Button>();
        HorizontalLayoutGroup hlg = mapCardGO.AddComponent<HorizontalLayoutGroup>();
        LayoutElement leCard = mapCardGO.AddComponent<LayoutElement>();

        leCard.preferredWidth = 925f;
        leCard.preferredHeight = 200f;

        if (mapCardBg != null)
            mapCardImage.sprite = mapCardBg;
        mapCardImage.color = new Color32(0x0C, 0x0C, 0x0C, 0x60);

        hlg.padding.left = 55;
        hlg.padding.right = 55;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        mapCardButton.onClick.AddListener(() => {
            MapLoader loader = FindObjectOfType<MapLoader>();
            if (loader != null)
            {
                loader.LoadMapAndVisualize(job.id, job.name);
            }
            else
            {
                Debug.LogError("MapLoader not found!");
            }
        });

        GameObject mapInfoLeftGO = new GameObject("mapInfoLeft", typeof(RectTransform));
        mapInfoLeftGO.transform.SetParent(mapCardGO.transform, false);
        VerticalLayoutGroup vlLeft = mapInfoLeftGO.AddComponent<VerticalLayoutGroup>();
        LayoutElement leLeft = mapInfoLeftGO.AddComponent<LayoutElement>();
        leLeft.preferredWidth = 382f;
        leLeft.preferredHeight = 193f;
        vlLeft.padding.top = 0;
        vlLeft.childAlignment = TextAnchor.UpperLeft;
        vlLeft.childForceExpandWidth = true;
        vlLeft.childForceExpandHeight = true;

        GameObject mapNameGO = new GameObject("mapName", typeof(TextMeshProUGUI));
        mapNameGO.transform.SetParent(mapInfoLeftGO.transform, false);
        TextMeshProUGUI mapNameText = mapNameGO.GetComponent<TextMeshProUGUI>();
        mapNameText.text = job.name.ToUpper();
        mapNameText.fontSize = 48;
        mapNameText.fontStyle = FontStyles.Bold;
        mapNameText.alignment = TextAlignmentOptions.TopLeft;

        GameObject mapIdGO = new GameObject("mapId", typeof(TextMeshProUGUI));
        mapIdGO.transform.SetParent(mapInfoLeftGO.transform, false);
        TextMeshProUGUI mapIdText = mapIdGO.GetComponent<TextMeshProUGUI>();
        mapIdText.text = job.id.ToString("D6");
        mapIdText.fontSize = 36;
        mapIdText.alignment = TextAlignmentOptions.TopLeft;

        GameObject mapInfoRightGO = new GameObject("mapInfoRight", typeof(RectTransform));
        mapInfoRightGO.transform.SetParent(mapCardGO.transform, false);
        VerticalLayoutGroup vlRight = mapInfoRightGO.AddComponent<VerticalLayoutGroup>();
        LayoutElement leRight = mapInfoRightGO.AddComponent<LayoutElement>();
        leRight.preferredWidth = 433f;
        leRight.preferredHeight = 193f;
        vlRight.padding.top = 15;
        vlRight.childAlignment = TextAnchor.UpperLeft;
        vlRight.childForceExpandWidth = true;
        vlRight.childForceExpandHeight = true;

        GameObject imagesCountGO = new GameObject("imagesCount", typeof(TextMeshProUGUI));
        imagesCountGO.transform.SetParent(mapInfoRightGO.transform, false);
        TextMeshProUGUI imagesCountText = imagesCountGO.GetComponent<TextMeshProUGUI>();
        imagesCountText.text = $"Images - {job.size}";
        imagesCountText.fontSize = 28;
        imagesCountText.alignment = TextAlignmentOptions.TopRight;

        GameObject dateOfCreationGO = new GameObject("dateOfCreation", typeof(TextMeshProUGUI));
        dateOfCreationGO.transform.SetParent(mapInfoRightGO.transform, false);
        TextMeshProUGUI dateOfCreationText = dateOfCreationGO.GetComponent<TextMeshProUGUI>();
        dateOfCreationText.text = job.created;
        dateOfCreationText.fontSize = 28;
        dateOfCreationText.alignment = TextAlignmentOptions.TopRight;
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
