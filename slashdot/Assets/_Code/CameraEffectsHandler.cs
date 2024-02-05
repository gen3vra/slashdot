using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectsHandler : MonoBehaviour
{
    public static CameraEffectsHandler Instance;
    private bool shaking;
    void Awake()
    {
        Instance = this;
    }

    public bool RequestCameraShake(float duration, float magnitude)
    {
        if (shaking)
        {
            return false;
        }
        StartCoroutine(Shake(duration, magnitude));
        return true;
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        shaking = true;
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            magnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            yield return null;
        }
        transform.localPosition = originalPos;
        shaking = false;
    }

}
