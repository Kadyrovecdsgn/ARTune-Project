using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class ProductManager : MonoBehaviour
{
    [SerializeField] private GameObject productPrefab; // Префаб карточки продукта
    [SerializeField] private Transform scrollContent;  // Контейнер для карточек в ScrollView
    [SerializeField] private GameObject detailedDescriptionPrefab; // Префаб окна детального описания

    // Класс данных о продукте
    [System.Serializable]
    public class ProductData
    {
        public Sprite productImage;   // Изображение продукта
        public string productName;    // Название продукта
        public string productType;    // Тип продукта
        public string productCost;    // Стоимость продукта

        [TextArea(3, 10)] // Минимальная высота 3 строки, максимальная 10 строк
        public string productDescription; // Описание продукта

        [TextArea(3, 10)] // Минимальная высота 3 строки, максимальная 10 строк
        public string productCharacteristics; // Характеристики продукта

        public UnityEvent onAddToSceneAction; // Действие для кнопки "Добавить на сцену"
        public UnityEvent onBuyAction;        // Действие для кнопки "Купить"
    }

    [SerializeField] private List<ProductData> products = new List<ProductData>(); // Список продуктов

    // Метод для очистки существующих карточек
    private void ClearProducts()
    {
        // Удаляем все дочерние объекты из scrollContent
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
    }

    // Метод для создания карточек продуктов
    public void AddProducts()
    {
        // Сначала очищаем существующие карточки
        ClearProducts();

        // Затем добавляем новые карточки
        foreach (ProductData productData in products)
        {
            GameObject newProduct = Instantiate(productPrefab, scrollContent);
            ConfigureProduct(newProduct, productData);
        }
    }

    // Метод для настройки карточки продукта
    private void ConfigureProduct(GameObject product, ProductData data)
    {
        // Настройка изображения
        Image image = product.transform.Find("Image").GetComponent<Image>();
        if (image != null)
        {
            image.sprite = data.productImage;
        }

        // Настройка текстовых полей
        TextMeshProUGUI productNameText = product.transform.Find("descriptionContainer/nameAndType/productName").GetComponent<TextMeshProUGUI>();
        if (productNameText != null)
        {
            productNameText.text = data.productName;
        }

        TextMeshProUGUI productTypeText = product.transform.Find("descriptionContainer/nameAndType/productType").GetComponent<TextMeshProUGUI>();
        if (productTypeText != null)
        {
            productTypeText.text = data.productType;
        }

        TextMeshProUGUI productCostText = product.transform.Find("descriptionContainer/Cost/productCost").GetComponent<TextMeshProUGUI>();
        if (productCostText != null)
        {
            productCostText.text = data.productCost + " $";
        }

        // Настройка кнопки для открытия детального описания
        Button button = product.GetComponent<Button>();
        if (button == null)
        {
            button = product.AddComponent<Button>(); // Добавляем кнопку, если её нет
        }

        // Привязка действия к кнопке: открытие окна детального описания
        button.onClick.AddListener(() => OpenDetailedDescription(data));
    }

    // Метод для открытия окна детального описания
    private void OpenDetailedDescription(ProductData data)
    {
        GameObject detailedWindow = Instantiate(detailedDescriptionPrefab);
        DetailedDescriptionController controller = detailedWindow.GetComponent<DetailedDescriptionController>();
        if (controller != null)
        {
            controller.Setup(data);
        }
    }
}
