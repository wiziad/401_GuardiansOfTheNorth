using UnityEngine;
using UnityEngine.EventSystems;

public class PinHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Scale (1 = normal, 1.3 = 30% bigger on hover)")]
    public float normalScale = 1f;
    public float hoverScale = 1.3f;
    public float speed = 10f;

    private Vector3 targetScale;
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        targetScale = Vector3.one * normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
    }

    void Update()
    {
        rt.localScale = Vector3.Lerp(rt.localScale, targetScale, Time.deltaTime * speed);
    }
}
