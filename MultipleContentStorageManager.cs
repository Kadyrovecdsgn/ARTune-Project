using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Immersal.Samples.ContentPlacement
{
    public class MultipleContentStorageManager : MonoBehaviour
    {
        [HideInInspector]
        public List<MovableContent> contentList = new List<MovableContent>();

        [SerializeField]
        private List<GameObject> m_ContentPrefabs = new List<GameObject>();  // Список префабов
        [SerializeField]
        private Immersal.AR.ARSpace m_ARSpace;
        [SerializeField]
        private string m_Filename = "content.json";
        private Savefile m_Savefile;
        private List<Vector3> m_Positions = new List<Vector3>();

        [System.Serializable]
        public struct Savefile
        {
            public List<Vector3> positions;
        }

        public static MultipleContentStorageManager Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null && !Application.isPlaying)
                {
                    instance = UnityEngine.Object.FindObjectOfType<MultipleContentStorageManager>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError("No MultipleContentStorageManager instance found. Ensure one exists in the scene.");
                }
                return instance;
            }
        }

        private static MultipleContentStorageManager instance = null;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("There must be only one MultipleContentStorageManager object in a scene.");
                UnityEngine.Object.DestroyImmediate(this);
                return;
            }

            if (m_ARSpace == null)
            {
                m_ARSpace = GameObject.FindObjectOfType<Immersal.AR.ARSpace>();
            }
        }

        private void Start()
        {
            contentList.Clear();
            LoadContents();
        }

        // Метод для добавления контента по индексу префаба
        public void AddContent(int prefabIndex)
        {
            if (prefabIndex < 0 || prefabIndex >= m_ContentPrefabs.Count)
            {
                Debug.LogError("Invalid prefab index.");
                return;
            }

            Transform cameraTransform = Camera.main.transform;
            GameObject go = Instantiate(m_ContentPrefabs[prefabIndex], cameraTransform.position + cameraTransform.forward, Quaternion.identity, m_ARSpace.transform);
            MovableContent movableContent = go.GetComponent<MovableContent>();
            if (movableContent != null)
            {
                contentList.Add(movableContent);
            }
        }

        public void DeleteAllContent()
        {
            List<MovableContent> copy = new List<MovableContent>();

            foreach (MovableContent content in contentList)
            {
                copy.Add(content);
            }

            foreach (MovableContent content in copy)
            {
                content.RemoveContent();
            }
        }

        public void SaveContents()
        {
            m_Positions.Clear();
            foreach (MovableContent content in contentList)
            {
                m_Positions.Add(content.transform.localPosition);
            }
            m_Savefile.positions = m_Positions;

            string jsonstring = JsonUtility.ToJson(m_Savefile, true);
            string dataPath = Path.Combine(Application.persistentDataPath, m_Filename);
            File.WriteAllText(dataPath, jsonstring);
        }

        public void LoadContents()
        {
            string dataPath = Path.Combine(Application.persistentDataPath, m_Filename);
            Debug.LogFormat("Trying to load file: {0}", dataPath);

            try
            {
                Savefile loadFile = JsonUtility.FromJson<Savefile>(File.ReadAllText(dataPath));

                foreach (Vector3 pos in loadFile.positions)
                {
                    GameObject go = Instantiate(m_ContentPrefabs[0], m_ARSpace.transform); // Загрузка первого префаба как пример
                    go.transform.localPosition = pos;
                }

                Debug.Log("Successfully loaded file!");
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarningFormat("{0}\n.json file for content storage not found. Created a new file!", e.Message);
                File.WriteAllText(dataPath, "");
            }
            catch (NullReferenceException err)
            {
                Debug.LogWarningFormat("{0}\n.json file for content storage not found. Created a new file!", err.Message);
                File.WriteAllText(dataPath, "");
            }
        }
    }
}
