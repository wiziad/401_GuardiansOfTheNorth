using UnityEngine;
using TMPro;

public class CurvedTMPText : MonoBehaviour
{
    [SerializeField] private float curveAmount = 24f;
    private TextMeshProUGUI text;

    public void SetCurveAmount(float value)
    {
        curveAmount = value;
    }

    public void RefreshNow()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            return;
        }

        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;
        float width = text.rectTransform.rect.width;
        if (width <= 0f)
        {
            return;
        }

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
            {
                continue;
            }

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            float centerX = (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) * 0.5f;
            float normalized = Mathf.InverseLerp(-width * 0.5f, width * 0.5f, centerX) * 2f - 1f;
            float yOffset = -(normalized * normalized) * curveAmount + curveAmount;

            Vector3 offset = new Vector3(0f, yOffset, 0f);
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
