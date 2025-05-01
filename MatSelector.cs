using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Immersal.Samples.ContentPlacement;

public class MatSelector : MonoBehaviour
{
    [System.Serializable]
    public class MaterialOption
    {
        public Material material; // Материал с шейдером
        public string displayName; // Отображаемое имя (например, "CARBON")
    }

    [SerializeField] private List<MaterialOption> materialOptions; // Список материалов с их отображаемыми именами
    [SerializeField] private Button swipeLeftButton; // Левая кнопка
    [SerializeField] private Button swipeRightButton; // Правая кнопка
    [SerializeField] private TextMeshProUGUI matOptionName; // Текст для отображения имени текущего материала

    private int currentIndex = 0; // Текущий индекс в списке материалов

    private void Start()
    {
        if (materialOptions.Count > 0)
        {
            UpdateMaterialAndText();
        }

        if (swipeLeftButton != null)
        {
            swipeLeftButton.onClick.AddListener(OnLeftClick);
        }
        if (swipeRightButton != null)
        {
            swipeRightButton.onClick.AddListener(OnRightClick);
        }
    }

    private void OnLeftClick()
    {
        if (materialOptions.Count > 0)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = materialOptions.Count - 1;
            UpdateMaterialAndText();
        }
    }

    private void OnRightClick()
    {
        if (materialOptions.Count > 0)
        {
            currentIndex++;
            if (currentIndex >= materialOptions.Count) currentIndex = 0;
            UpdateMaterialAndText();
        }
    }

    private void UpdateMaterialAndText()
    {
        if (currentIndex >= 0 && currentIndex < materialOptions.Count)
        {
            MaterialOption currentOption = materialOptions[currentIndex];

            if (MovableContent.CurrentActive != null)
            {
                Debug.Log($"Active object: {MovableContent.CurrentActive.name} (Tag: {MovableContent.CurrentActive.tag})");
            }
            else
            {
                Debug.Log("No active object in focus.");
            }

            // Применяем материал только если есть текущий активный объект с тегом "paintable"
            if (MovableContent.CurrentActive != null && MovableContent.CurrentActive.CompareTag("paintable"))
            {
                Renderer renderer = MovableContent.CurrentActive.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = currentOption.material;
                }
            }

            // Обновляем текст независимо от наличия объекта
            if (matOptionName != null && !string.IsNullOrEmpty(currentOption.displayName))
            {
                matOptionName.text = currentOption.displayName.ToUpper();
            }
        }
    }
}

