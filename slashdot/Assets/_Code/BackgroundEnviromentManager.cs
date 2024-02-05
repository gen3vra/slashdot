using System.Collections;
using UnityEngine;

public class BackgroundEnviromentManager : MonoBehaviour
{
    public SpriteRenderer RevealBG;
    public SpriteRenderer Background;
    public SpriteRenderer Top;
    public SpriteRenderer Bottom;
    public SpriteRenderer LeftWall;
    public SpriteRenderer RightWall;

    private float highlightingPlayerAmount = 1f;
    private float highlightingPlayerAmountMax = 1;
    Coroutine highlightingPlayerCoroutine;
    Coroutine cycleMenuColorsCoroutine;

    void Start()
    {
        Init();
    }
    public void Init()
    {
        //Color color1 = Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
        Color color1 = new(0, 0, 0, 1);
        Color color2 = new(1, 1, 1, 1);
        RevealBG.material.SetColor("_BGDefaultCol", color1);
        RevealBG.material.SetColor("_PlayerRevealColor", color2);
        //cycleMenuColorsCoroutine = StartCoroutine(CycleMenuColors());
        float width = Camera.main.orthographicSize * Screen.width / Screen.height;
        LeftWall.transform.position = new Vector3(-width + 10, 0, 0);
        //LeftWall.color = Manager.Instance.player.PlayerColor;
    }

    IEnumerator CycleMenuColors()
    {
        float time = 0f;
        float duration = 10f;
        Color color1 = RevealBG.material.GetColor("_BGDefaultCol");
        while (true)
        {
            Color color2 = Color.HSVToRGB(Random.Range(0f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
            time = 0f;
            while (time < duration)
            {
                RevealBG.material.SetColor("_BGDefaultCol", Color.Lerp(color1, color2, time / duration));
                time += Time.deltaTime;
                yield return null;
            }
            color1 = color2;
        }
    }

    public void SetPlayerRevealColor(Color color)
    {
        RevealBG.material.SetColor("_PlayerRevealColor", color);
    }
    public void SetPlayerPosition(Vector3 position)
    {
        RevealBG.material.SetVector("_PlayerPosition", position);
    }

    public void StartHighlightPlayer(bool highlight = true, bool gameOver = false)
    {
        if (highlightingPlayerCoroutine != null)
        {
            StopCoroutine(highlightingPlayerCoroutine);
        }
        highlightingPlayerCoroutine = StartCoroutine(HighlightPlayer(highlight, gameOver));
    }

    IEnumerator HighlightPlayer(bool highlight = true, bool gameOver = false)
    {
        float time = 0f;
        float duration = 3.5f;
        float startAmount = highlightingPlayerAmount;
        float endAmount = highlight ? highlightingPlayerAmountMax : 0f;
        while (time < duration)
        {
            highlightingPlayerAmount = Mathf.Lerp(startAmount, endAmount, time / duration);
            RevealBG.material.SetFloat("_AmountOfHighlight", highlightingPlayerAmount);
            if (gameOver)
                RevealBG.material.SetColor("_BGDefaultCol", Color.Lerp(Color.red, Color.black, time / duration));
            time += Time.deltaTime;
            yield return null;
        }
        highlightingPlayerAmount = endAmount;
        RevealBG.material.SetFloat("_AmountOfHighlight", highlightingPlayerAmount);
        RevealBG.material.SetColor("_BGDefaultCol", Color.black);
    }

    public void SetBackgroundColor(Color color1, Color color2)
    {
        color1 = new Color(color1.r * 0.25f, color1.g * 0.25f, color1.b * 0.25f, color1.a);
        color2 = new Color(color2.r * 0.25f, color2.g * 0.25f, color2.b * 0.25f, color2.a);
        Background.material.SetColor("_Color1", color1);
        Background.material.SetColor("_Color2", color2);
    }

    public void StartGame()
    {
        /*if (cycleMenuColorsCoroutine != null)
        {
            StopCoroutine(cycleMenuColorsCoroutine);
        }*/
        StartHighlightPlayer(highlight: false);
        StartCoroutine(MenuExitCoroutine());
        //LeftWall.gameObject.SetActive(false);
        /*
        LeftWall.color = Manager.Instance.player.PlayerColor;
        RightWall.color = Manager.Instance.player.PlayerColor;
        Top.color = Manager.Instance.player.PlayerColor;
        Bottom.color = Manager.Instance.player.PlayerColor;*/
    }

    public void GameOver()
    {
        StartHighlightPlayer(highlight: true, gameOver: true);
    }

    public void CheckOrientation(ScreenOrientation orient)
    {
        ScreenOrientation orientation = (ScreenOrientation)orient;
        // Set walls to edges of screen
        // Get screen size
        float height = Camera.main.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;
        // Set wall sizes
        //LeftWall.size = new Vector2(0.1f, height);
        /*
        RightWall.size = new Vector2(0.1f, height);
        Top.size = new Vector2(width, 0.1f);
        Bottom.size = new Vector2(width, 0.1f);*/
        // Set wall positions
        /*
        LeftWall.transform.position = new Vector3(-width / 2, 0, 0);
        RightWall.transform.position = new Vector3(width / 2, 0, 0);
        Top.transform.position = new Vector3(0, height / 2, 0);
        Bottom.transform.position = new Vector3(0, -height / 2, 0);*/
        // Set background size
        Background.size = new Vector2(width, height);
        // Set background position
        Background.transform.position = new Vector3(0, 0, 0);
        // Set reveal background size
        RevealBG.size = new Vector2(width, height);
        // Set reveal background position
        RevealBG.transform.position = new Vector3(0, 0, 0);
        Debug.Log("Orientation: " + orientation);
    }

    IEnumerator MenuExitCoroutine()
    {
        float time = 0f;
        float duration = 4f;
        Vector3 start = LeftWall.transform.position;
        while (time < duration)
        {
            Debug.Log("MenuExitCoroutine");
            LeftWall.transform.position = Vector3.Lerp(start, new Vector3(-50, 0, 0), time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        LeftWall.gameObject.SetActive(false);
    }
}
