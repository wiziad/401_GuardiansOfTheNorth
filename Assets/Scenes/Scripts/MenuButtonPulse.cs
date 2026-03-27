using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float idlePulseAmount = 0.035f;
    [SerializeField] private float idlePulseSpeed = 2.5f;
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private float hoverBrightness = 0.2f;

    private RectTransform rectTransform;
    private Image image;
    private Color baseColor;
    private Color hoverColor;
    private Vector3 baseScale;
    private bool hovered;

    public void Configure()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image != null)
        {
            baseColor = image.color;
            hoverColor = Color.Lerp(baseColor, Color.white, Mathf.Clamp01(hoverBrightness));
        }

        baseScale = Vector3.one;
    }

    private void Awake()
    {
        Configure();
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * idlePulseSpeed) * idlePulseAmount;
        float target = hovered ? hoverScale : pulse;
        Vector3 targetScale = baseScale * target;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);

        if (image != null)
        {
            Color targetColor = hovered ? hoverColor : baseColor;
            image.color = Color.Lerp(image.color, targetColor, Time.unscaledDeltaTime * lerpSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
