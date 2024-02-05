
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerBtn : MonoBehaviour
{
    public bool ColorPickerIsActive;
    private Image previewGraphic;

    public ColorPicker colorPicker;

    public enum ColorSettingIndex
    {
        PlayerPrimaryColor,
        PlayerAttackColor
    }
    public ColorSettingIndex colorSettingIndex;

    private void Start()
    {
        previewGraphic = GetComponent<Image>();
        previewGraphic.color = colorPicker.color;
        colorPicker.onColorChanged += OnColorChanged;
    }

    public void OnColorChanged(Color c)
    {
        if (!ColorPickerIsActive)
            return;
        previewGraphic.color = c;
        switch (colorSettingIndex)
        {
            case ColorSettingIndex.PlayerPrimaryColor:
                Manager.Instance.SetPlayerPrimaryOverrideColor(c);
                break;
            case ColorSettingIndex.PlayerAttackColor:
                Manager.Instance.SetPlayerAttackOverrideColor(c);
                break;
        }
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
            colorPicker.onColorChanged -= OnColorChanged;
    }
}