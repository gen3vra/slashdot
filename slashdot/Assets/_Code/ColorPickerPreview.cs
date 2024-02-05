using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ColorPickerPreview : MonoBehaviour
{
    public bool isText;
    private Image previewGraphic;
    private TMP_InputField previewText;

    public ColorPicker colorPicker;

    private void Start()
    {
        if (isText)
            previewText = GetComponent<TMP_InputField>();
        else
        {
            previewGraphic = GetComponent<Image>();
            previewGraphic.color = colorPicker.color;
        }
        colorPicker.onColorChanged += OnColorChanged;
    }

    public void OnColorChanged(Color c)
    {
        if (isText)
            previewText.text = "#" + ColorUtility.ToHtmlStringRGB(c);
        else
            previewGraphic.color = c;
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
            colorPicker.onColorChanged -= OnColorChanged;
    }
}