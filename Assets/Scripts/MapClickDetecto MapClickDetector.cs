using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapClickDetector : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D referenceTexture;   // flat-color reference PNG
    public Camera cam;

    [Header("Popup UI")]
    public GameObject popupPanel;        // assign in Inspector
    public TextMeshProUGUI popupText;    // assign in Inspector
    public Button yesButton;             // assign in Inspector
    public Button noButton;              // assign in Inspector

    private string selectedProvince;

    void Start()
    {
        // Make sure popup is hidden at start
        popupPanel.SetActive(false);

        // Wire up the buttons
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // Don't detect clicks if popup is already open
        if (popupPanel.activeSelf) return;

        Vector2 mousePos = Input.mousePosition;
        RaycastHit2D hit = Physics2D.Raycast(
            cam.ScreenToWorldPoint(mousePos), Vector2.zero);

        if (hit.collider == null) return;

        SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Vector2 localPoint = hit.point - (Vector2)sr.transform.position;
        Sprite sprite = sr.sprite;

        float pixelX = (localPoint.x / sprite.bounds.size.x + 0.5f) * referenceTexture.width;
        float pixelY = (localPoint.y / sprite.bounds.size.y + 0.5f) * referenceTexture.height;

        Color c = referenceTexture.GetPixel((int)pixelX, (int)pixelY);

        if (c.a < 0.1f) return;

        string province = ColorToProvince(c);
        if (province != null)
        {
            ShowPopup(province);
        }
    }

    void ShowPopup(string province)
    {
        selectedProvince = province;
        popupText.text = "Level: " + province + "\nDo you wish to play?";
        popupPanel.SetActive(true);
    }

    void OnYesClicked()
    {
        popupPanel.SetActive(false);
        Debug.Log("Loading level for: " + selectedProvince);
        // Later replace this with:
        // SceneManager.LoadScene(selectedProvince);
    }

    void OnNoClicked()
    {
        popupPanel.SetActive(false);
        selectedProvince = null;
    }

    string ColorToProvince(Color c)
    {
        if (Approx(c, 1f, 0f, 0f))    return "British Columbia";
        if (Approx(c, 1f, 0.53f, 0f)) return "Alberta";
        if (Approx(c, 1f, 1f, 0f))    return "Saskatchewan";
        if (Approx(c, 0f, 1f, 0f))    return "Manitoba";
        if (Approx(c, 0f, 0f, 1f))    return "Ontario";    // swapped: Ontario = blue
        if (Approx(c, 0f, 1f, 1f))    return "Quebec";     // swapped: Quebec = cyan
        if (Approx(c, 1f, 0f, 1f))    return "New Brunswick";
        if (Approx(c, 1f, 0f, 0.53f)) return "Nova Scotia";
        if (Approx(c, 0.53f, 1f, 0f)) return "PEI";
        if (Approx(c, 0f, 1f, 0.53f)) return "Newfoundland";
        if (Approx(c, 0.53f, 0f, 1f)) return "Yukon";
        if (Approx(c, 0f, 0.53f, 1f)) return "NWT";
        if (Approx(c, 1f, 0.4f, 0f))  return "Nunavut";
        return null;
    }

    bool Approx(Color c, float r, float g, float b, float tolerance = 0.1f)
    {
        return Mathf.Abs(c.r - r) < tolerance &&
               Mathf.Abs(c.g - g) < tolerance &&
               Mathf.Abs(c.b - b) < tolerance;
    }
}