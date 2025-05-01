using System.Collections;
using UnityEngine;
using Immersal.AR;
using Immersal.REST;
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

        arMap.Uninitialize();
        yield return new WaitForEndOfFrame();

        arMap.SetIdAndName(mapId, mapName, true);
        arMap.OnDeviceBehaviour = OnDeviceBehaviour.Download;
        arMap.DownloadVisualizationAtRuntime = true;
        arMap.LocalizationMethod = LocalizationMethod.Server;

        SDKMapMetadataGetResult metadata = new SDKMapMetadataGetResult();
        bool metadataReceived = false;

        string token = TokenManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token not found! Cannot load map.");
            yield break;
        }

        JobMapMetadataGetAsync metadataJob = new JobMapMetadataGetAsync
        {
            id = mapId,
            token = token
        };

        metadataJob.OnResult += (result) => {
            metadata = result;
            metadataReceived = true;
        };

        metadataJob.RunJobAsync();

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

        arMap.SetMetadata(metadata, true);
        arMap.ApplyAlignment();

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

        CreateMapVisualization(arMap);
        Debug.Log($"Created visualization for map {mapId}");

        yield return StartCoroutine(RegisterMaps());

        yield return StartCoroutine(DownloadSparsePointCloud(arMap));

        ImmersalSDK.Instance.LocalizeOnce();
    }

    private void CreateMapVisualization(ARMap arMap)
    {
        if (arMap.Visualization != null)
        {
            arMap.RemoveVisualization();
        }

        GameObject visObject = new GameObject($"{arMap.mapId}-Visualization");
        visObject.transform.SetParent(arMap.transform, false);

        ARMapVisualization visualization = visObject.AddComponent<ARMapVisualization>();
        visualization.Initialize(arMap, visualizationRenderMode, visualizationColor);

        arMap.Visualization = visualization;
    }

    private IEnumerator DownloadSparsePointCloud(ARMap arMap)
    {
        string token = TokenManager.GetToken();
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token not found! Cannot download sparse point cloud.");
            yield break;
        }

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
        ImmersalSDK.Instance.RestartSdk();
        yield return new WaitForSeconds(1f);

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
