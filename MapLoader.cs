using System.Collections;
using UnityEngine;
using Immersal.AR;
using Immersal.REST;
using System.Threading.Tasks;
using Immersal;
using UnityEngine.Networking;

public class MapLoader : MonoBehaviour
{
    [SerializeField] private float metadataTimeout = 10f;
    [SerializeField] private Color visualizationColor = new Color(0.57f, 0.93f, 0.12f);
    [SerializeField] private ARMapVisualization.RenderMode visualizationRenderMode = ARMapVisualization.RenderMode.EditorAndRuntime;

    public void LoadMapAndVisualize(int mapId, string mapName)
    {
        StartCoroutine(LoadMapCoroutine(mapId, mapName));
    }

    private IEnumerator LoadMapCoroutine(int mapId, string mapName)
    {
        ARMap arMap = FindObjectOfType<ARMap>();
        if (arMap == null)
        {
            Debug.LogError("ARMap component not found in scene!");
            yield break;
        }

        // Сброс текущей карты
        arMap.Uninitialize();
        yield return new WaitForEndOfFrame();

        // Настройка параметров карты
        arMap.SetIdAndName(mapId, mapName, true);
        arMap.OnDeviceBehaviour = OnDeviceBehaviour.Download;
        arMap.DownloadVisualizationAtRuntime = true;
        arMap.LocalizationMethod = LocalizationMethod.Server;

        // Загрузка метаданных
        SDKMapMetadataGetResult metadata = new SDKMapMetadataGetResult();
        bool metadataReceived = false;

        JobMapMetadataGetAsync metadataJob = new JobMapMetadataGetAsync
        {
            id = mapId,
            token = ImmersalSDK.Instance.developerToken
        };

        metadataJob.OnResult += (result) => {
            metadata = result;
            metadataReceived = true;
        };

        metadataJob.RunJobAsync();

        // Ожидание метаданных
        float timer = 0f;
        while (!metadataReceived && timer < metadataTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!metadataReceived)
        {
            Debug.LogError("Timeout loading metadata");
            yield break;
        }

        // Применение метаданных
        arMap.SetMetadata(metadata, true);
        arMap.ApplyAlignment();

        // Загрузка данных карты
        arMap.Configure();

        timer = 0f;
        while (!arMap.IsConfigured && timer < 30f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!arMap.IsConfigured)
        {
            Debug.LogError("Map configuration timeout");
            yield break;
        }

        // Создание визуализации (создается дочерний объект ARMapVisualization)
        CreateMapVisualization(arMap);
        Debug.Log($"Created visualization for map {mapId}");

        // 7. Запуск регистрации карты (если требуется)
        yield return StartCoroutine(RegisterMaps());

        // 8. Скачивание sparse point cloud (.ply) и загрузка в визуализацию
        yield return StartCoroutine(DownloadSparsePointCloud(arMap));

        // 9. Запуск локализации
        ImmersalSDK.Instance.LocalizeOnce();
    }

    private void CreateMapVisualization(ARMap arMap)
    {
        // Если старая визуализация существует, удаляем её
        if (arMap.Visualization != null)
        {
            arMap.RemoveVisualization();
        }

        // Создаем новый GameObject для визуализации
        GameObject visObject = new GameObject($"{arMap.mapId}-Visualization");
        visObject.transform.SetParent(arMap.transform, false);

        ARMapVisualization visualization = visObject.AddComponent<ARMapVisualization>();
        visualization.Initialize(arMap, visualizationRenderMode, visualizationColor);

        arMap.Visualization = visualization;
    }

    private IEnumerator DownloadSparsePointCloud(ARMap arMap)
    {
        // Считываем токен
        string tokenFilePath = System.IO.Path.Combine(Application.persistentDataPath, "immersal_token.txt");
        if (!System.IO.File.Exists(tokenFilePath))
        {
            Debug.LogError("Token file not found! Cannot download sparse point cloud.");
            yield break;
        }
        string token = System.IO.File.ReadAllText(tokenFilePath).Trim();

        // Формируем URL для скачивания sparse point cloud (PLY-файл)
        string url = $"https://api.immersal.com/sparse?token={token}&id={arMap.mapId}&ply=1";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Sparse point cloud download failed: " + request.error);
            yield break;
        }

        byte[] plyData = request.downloadHandler.data;
        Debug.Log($"Sparse point cloud downloaded. Size: {plyData.Length} bytes");

        // Передаем байты в ARMapVisualization через метод LoadPly
        if (arMap.Visualization != null)
        {
            arMap.Visualization.LoadPly(plyData, $"{arMap.mapId}-{arMap.mapName}-sparse");
            Debug.Log("Sparse point cloud loaded into visualization.");
        }
        else
        {
            Debug.LogError("ARMap.Visualization is null. Cannot load sparse point cloud.");
        }
    }

    private IEnumerator RegisterMaps()
    {
        // Перезапуск SDK
        ImmersalSDK.Instance.RestartSdk();
        yield return new WaitForSeconds(1f);

        // Регистрация карт через MapManager
        ARMap[] maps = FindObjectsOfType<ARMap>();
        foreach (ARMap map in maps)
        {
            ISceneUpdateable sceneUpdateable = map.transform.parent?.GetComponent<ISceneUpdateable>();
            if (sceneUpdateable != null)
            {
                MapManager.RegisterAndLoadMap(map, sceneUpdateable);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}