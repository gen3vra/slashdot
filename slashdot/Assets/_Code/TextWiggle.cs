using UnityEngine;
using TMPro;

public class TextWiggle : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 1f;
    public TextMeshProUGUI textMesh;
    private string origText;
    void Start()
    {
        origText = textMesh.text;
    }
    void FixedUpdate()
    {
        string newText = "";
        for (int i = 0; i < origText.Length; i++)
        {
            char c = origText[i];
            float offset = 10 * Mathf.Sin(Time.time * frequency + i) * amplitude;
            newText += $"<voffset={offset}>{c}</voffset>";
        }
        textMesh.text = newText;
    }
}
