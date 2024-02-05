using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingManager : MonoBehaviour
{
    private Coroutine postProcessingCoroutine;
    public PostProcessVolume NormalPostProcessingVolume;
    public PostProcessVolume GameOverPostProcessingVolume;

    public void Init()
    {
        NormalPostProcessingVolume.weight = 1;
        GameOverPostProcessingVolume.weight = 0;
    }

    public void StartGame()
    {
        if (postProcessingCoroutine != null)
        {
            StopCoroutine(postProcessingCoroutine);
        }
        NormalPostProcessingVolume.weight = 1;
        GameOverPostProcessingVolume.weight = 0;
    }

    public void GameOver()
    {
        NormalPostProcessingVolume.weight = 0;
        GameOverPostProcessingVolume.weight = 1;
        if (postProcessingCoroutine != null)
        {
            StopCoroutine(postProcessingCoroutine);
        }
        postProcessingCoroutine = StartCoroutine(PostProcessingCoroutine(true));
    }

    IEnumerator PostProcessingCoroutine(bool fadeToNormal)
    {
        float t = 0;
        float duration = 10f;

        while (t < duration)
        {
            t += Time.deltaTime;
            if (fadeToNormal)
            {
                NormalPostProcessingVolume.weight = Mathf.Lerp(0, 1, t / duration);
                GameOverPostProcessingVolume.weight = Mathf.Lerp(1, 0, t / duration);
            }
            else
            {
                NormalPostProcessingVolume.weight = Mathf.Lerp(1, 0, t / duration);
                GameOverPostProcessingVolume.weight = Mathf.Lerp(0, 1, t / duration);
            }
            yield return null;
        }
    }
}
