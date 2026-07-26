using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask objectLayerMask;

    [Header("UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text displayNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Image weightImage;

    [Header("Weight Colors")]
    [SerializeField] private Color veryLightColor = Color.white;
    [SerializeField] private Color lightColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color heavyColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color veryHeavyColor = Color.red;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        HideTooltip();
    }

    private void Update()
    {
     

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, objectLayerMask))
        {
            BalanceObject balanceObject = hit.collider.GetComponentInParent<BalanceObject>();

            if (balanceObject != null)
            {
                ShowTooltip(balanceObject);
                return;
            }
        }

        HideTooltip();
    }

    public void ShowTooltip(BalanceObject balanceObject)
    {
        if (balanceObject == null)
            return;

        Color weightColor = GetWeightColor(balanceObject.Weight);

        displayNameText.text = balanceObject.DisplayName;
        weightText.text = GetWeightName(balanceObject.Weight);

        weightImage.color = weightColor;
       

        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private Color GetWeightColor(Weight weight)
    {
        switch (weight)
        {
            case Weight.VeryLight:
                return veryLightColor;

            case Weight.Light:
                return lightColor;

            case Weight.Medium:
                return mediumColor;

            case Weight.Heavy:
                return heavyColor;

            case Weight.VeryHeavy:
                return veryHeavyColor;

            default:
                return Color.white;
        }
    }

    private string GetWeightName(Weight weight)
    {
        switch (weight)
        {
            case Weight.VeryLight:
                return "Very Light";

            case Weight.Light:
                return "Light";

            case Weight.Medium:
                return "Medium";

            case Weight.Heavy:
                return "Heavy";

            case Weight.VeryHeavy:
                return "Very Heavy";

            default:
                return weight.ToString();
        }
    }
}