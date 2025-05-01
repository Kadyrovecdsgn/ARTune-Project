using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailedDescriptionController : MonoBehaviour
{
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI productTypeText;
    [SerializeField] private TextMeshProUGUI productCostText;
    [SerializeField] private TextMeshProUGUI productDescriptionText;
    [SerializeField] private TextMeshProUGUI productCharacteristicsText;
    [SerializeField] private Button addToSceneButton;
    [SerializeField] private Button buyButton;

    public void Setup(ProductManager.ProductData data)
    {
        productImage.sprite = data.productImage;
        productNameText.text = data.productName;
        productTypeText.text = data.productType;
        productCostText.text = data.productCost + " $";
        productDescriptionText.text = data.productDescription;
        productCharacteristicsText.text = data.productCharacteristics;

        // Настройка кнопок
        addToSceneButton.onClick.AddListener(data.onAddToSceneAction.Invoke);
        buyButton.onClick.AddListener(data.onBuyAction.Invoke);
    }
}
