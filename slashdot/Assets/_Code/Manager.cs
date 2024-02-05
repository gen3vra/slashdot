using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    private BackgroundEnviromentManager backgroundEnviromentManager;
    private PostProcessingManager postProcessingManager;
    private AudioManager audioManager;
    public Player player;
    public GameObject enemyPrefab;
    public GameObject shootingEnemyPrefab;
    public GameObject bulletPrefab;
    public GameObject explosionParticlesPrefab;

    public int screenWidth;
    public int screenHeight;


    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }
    // speed
    private bool playing;

    public GameState CurrentGameState = GameState.Menu;
    private Coroutine spawnEnemyCoroutine;


    public GameData CurrentGameData;

    public Color? overridePlayerColor;
    public Color? overrideAttackColor;
    /// <summary>
    /// Set override colors to random every game
    /// </summary>
    private bool DoRandomColors;

    [Header("UI")]
    public GameObject scoreText;
    public GameObject gameOverScoreText;
    public GameObject mainMenu;
    public GameObject GameOverUI;
    public GameObject loadingOverlayUI;
    public GameObject creditsUI;
    public GameObject titleCredits;
    public GameObject comboDisplay;
    public Animator GameUIDisplay;
    public GameObject comboMultiDisplay;
    public GameObject optionsBtn;
    public GameObject optionsPanel;
    public Image comboBarDisplay;
    public GameObject colorPanel;
    [Header("Sounds")]

    public AudioSource gameComboSoundSource;
    public AudioClip newComboNoise;
    public AudioClip octave1Noise;
    public AudioClip octave2Noise;
    public AudioClip octave3Noise;
    public AudioClip octave4Noise;


    public List<Bullet> TrackedActiveBullets = new List<Bullet>();
    public List<Enemy> TrackedActiveEnemies = new List<Enemy>();
    public List<ExplosionParticles> TrackedActiveExplosionParticles = new List<ExplosionParticles>();
    public List<Enemy> PooledEnemies = new List<Enemy>();
    public List<Bullet> PooledBullets = new List<Bullet>();
    public List<ExplosionParticles> PooledExplosionParticles = new List<ExplosionParticles>();
    private float currentRoundTimer;
    private float maxDifficultyScale = 3f;
    private float hardDifficultyScale = 2f;
    private float mediumDifficultyScale = 1.25f;
    private DateTime lastRoundEndTime;
    // For the sake of progression we will skip the 13th note and start over at 1
    string[] notesDisplay = new string[12] { "C", "C#|Db", "D", "D#|Eb", "E", "F", "F#|Gb", "G", "G#|Ab", "A", "A#|Bb", "B" };

    public float comboTimer = 0f;
    /// <summary>
    // Unsure if too long
    /// </summary>
    public readonly float comboTimerMax = 4.5f;
    private bool comboActive;
    public bool ComboActive { get => comboActive; }

    private GameObject currentColorPickingBtn;

    public bool MobileControls;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        loadingOverlayUI.SetActive(true);
        player = FindObjectOfType<Player>();
        backgroundEnviromentManager = GetComponent<BackgroundEnviromentManager>();
        postProcessingManager = GetComponent<PostProcessingManager>();
        audioManager = FindObjectOfType<AudioManager>();

        LoadGame();
        SetupPools();
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        postProcessingManager.Init();
        mainMenu.SetActive(true);
        scoreText.SetActive(false);
        comboDisplay.SetActive(false);
        comboMultiDisplay.SetActive(false);
        comboBarDisplay.fillAmount = 0f;
        gameOverScoreText.SetActive(false);
        GameOverUI.SetActive(false);
        loadingOverlayUI.SetActive(false);
        creditsUI.SetActive(true);
        titleCredits.SetActive(true);
        //optionsBtn.SetActive(true);
        optionsPanel.SetActive(false);
        colorPanel.SetActive(false);

        backgroundEnviromentManager.SetPlayerRevealColor(overridePlayerColor ?? player.PlayerColor);
#if UNITY_EDITOR
        Debug.Log("Calling CheckOrientation for Editor");
        CheckOrientation((int)ScreenOrientation.LandscapeLeft);
#endif
    }

    /// <summary>
    /// External
    /// </summary>
    /// <param name="orient"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    public void CheckOrientation(int orient)
    {
        ScreenOrientation orientation = (ScreenOrientation)orient;

        if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown || orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
            Debug.Log("Orientation changed to " + orientation.ToString());

        // if orientation potraits activate mobile controls
        MobileControls = orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown;

        if (MobileControls)
        {
            Debug.LogError("Mobile Controls Activated");
            throw new System.NotImplementedException();
        }

        backgroundEnviromentManager.CheckOrientation(orientation);
    }

    void Update()
    {
        if (!playing)
            return;

        currentRoundTimer += Time.deltaTime;

        if (GetDifficultyScale() > hardDifficultyScale)
            comboTimer -= Time.deltaTime / 2f;
        else
            comboTimer -= Time.deltaTime / 2.5f;

        if (comboActive && comboTimer <= 0)
        {
            comboActive = false;
            Debug.Log("Combo ended - " + CurrentGameData.combo.ToString());
            UpdateState();
        }

        DisplayConstantGraphics();
    }
    public void StartGameClick()
    {
        if (lastRoundEndTime != null && (DateTime.Now - lastRoundEndTime).TotalSeconds < 2f)
            return;

        StartGame();
    }
    /// <summary>
    /// Player has started new game
    /// </summary>
    void StartGame()
    {
        loadingOverlayUI.SetActive(true);
        CleanupLastGame();
        CurrentGameState = GameState.Playing;
        CurrentGameData.score = 0;
        CurrentGameData.combo = 0;
        CurrentGameData.gameHighestCombo = 0;
        comboTimer = 0f;

        currentRoundTimer = 0f;
        spawnEnemyCoroutine = StartCoroutine(SpawnEnemyCoroutine());

        playing = true;
        player.InitGame();
        //player.FreezeControls(unfreeze: true);
        GameOverUI.SetActive(false);
        scoreText.SetActive(true);
        comboDisplay.SetActive(true);
        GameUIDisplay.SetInteger("displayState", 1);
        comboMultiDisplay.SetActive(true);
        gameOverScoreText.SetActive(false);
        creditsUI.SetActive(false);
        titleCredits.SetActive(false);
        //optionsBtn.SetActive(false);
        optionsPanel.SetActive(false);

        postProcessingManager.StartGame();
        mainMenu.SetActive(false);
        UpdateState();
        loadingOverlayUI.SetActive(false);

        backgroundEnviromentManager.SetPlayerRevealColor(overridePlayerColor ?? player.PlayerColor);
        backgroundEnviromentManager.SetBackgroundColor(overridePlayerColor ?? player.PlayerColor, overrideAttackColor ?? player.PlayerAttackColor);
        backgroundEnviromentManager.StartGame();
        if (DoRandomColors)
        {
            Color newOverridePlayerColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            Color newOverrideAttackColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            overridePlayerColor = newOverridePlayerColor;
            overrideAttackColor = newOverrideAttackColor;
            player.SetColors();
        }
        audioManager.StartGame();
    }

    void SetupPools()
    {
        for (int i = 0; i < 100; i++)
        {
            var enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            var shootingEnemy = Instantiate(shootingEnemyPrefab, Vector3.zero, Quaternion.identity);
            var bullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
            var explosionParticles = Instantiate(explosionParticlesPrefab, Vector3.zero, Quaternion.identity);

            PooledEnemies.Add(enemy.GetComponent<Enemy>());
            PooledEnemies.Add(shootingEnemy.GetComponent<Enemy>());
            PooledBullets.Add(bullet.GetComponent<Bullet>());
            PooledExplosionParticles.Add(explosionParticles.GetComponent<ExplosionParticles>());

            enemy.SetActive(false);
            shootingEnemy.SetActive(false);
            bullet.SetActive(false);
            explosionParticles.SetActive(false);
        }
    }

    /// <summary>
    /// Get enemy from pool
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns></returns>
    public GameObject GetEnemy(Enemy.EnemyType enemyType)
    {
        foreach (var enemy in PooledEnemies)
        {
            if (enemy.CurrentEnemyType == enemyType)
            {
                PooledEnemies.Remove(enemy);
                return enemy.gameObject;
            }
        }

        Debug.LogWarning("Pool not big enough, instantiating new enemy");
        var newEnemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        newEnemy.GetComponent<Enemy>().CurrentEnemyType = enemyType;
        return newEnemy;
    }

    /// <summary>
    /// Remove enemy from active list and add to pool
    /// </summary>
    /// <param name="enemy"></param>
    public void RemoveEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);

        TrackedActiveEnemies.Remove(enemy);
        PooledEnemies.Add(enemy);
    }

    public GameObject GetBullet()
    {
        foreach (var bullet in PooledBullets)
        {
            PooledBullets.Remove(bullet);
            TrackedActiveBullets.Add(bullet);
            return bullet.gameObject;
        }

        Debug.LogError("Pool not big enough, instantiating new bullet");
        var newBullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity);
        TrackedActiveBullets.Add(newBullet.GetComponent<Bullet>());
        return newBullet;
    }

    public void RemoveBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);

        TrackedActiveBullets.Remove(bullet);
        PooledBullets.Add(bullet);
    }

    public GameObject GetExplosionParticles(Color col, Vector3 position)
    {
        foreach (var explosionParticles in PooledExplosionParticles)
        {
            PooledExplosionParticles.Remove(explosionParticles);
            TrackedActiveExplosionParticles.Add(explosionParticles);
            explosionParticles.Init(col);
            explosionParticles.transform.position = position;
            explosionParticles.gameObject.SetActive(true);
            return explosionParticles.gameObject;
        }

        Debug.LogError("Pool not big enough, instantiating new explosion particles");
        var newExplosionParticles = Instantiate(explosionParticlesPrefab, Vector3.zero, Quaternion.identity);
        TrackedActiveExplosionParticles.Add(newExplosionParticles.GetComponent<ExplosionParticles>());
        newExplosionParticles.GetComponent<ExplosionParticles>().Init(col);
        newExplosionParticles.transform.position = position;
        newExplosionParticles.gameObject.SetActive(true);
        return newExplosionParticles;
    }

    public void RemoveExplosionParticles(ExplosionParticles explosionParticles)
    {
        explosionParticles.gameObject.SetActive(false);

        TrackedActiveExplosionParticles.Remove(explosionParticles);
        PooledExplosionParticles.Add(explosionParticles);
    }

    void CleanupLastGame()
    {
        while (TrackedActiveBullets.Count > 0)
        {
            var bullet = TrackedActiveBullets[0];
            RemoveBullet(bullet);
        }

        while (TrackedActiveEnemies.Count > 0)
        {
            var enemy = TrackedActiveEnemies[0];
            RemoveEnemy(enemy);
        }

        while (TrackedActiveExplosionParticles.Count > 0)
        {
            var explosionParticles = TrackedActiveExplosionParticles[0];
            RemoveExplosionParticles(explosionParticles);
        }
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        // Beginning animation time -> Let's not already have enemies flying across at you before the screen is even transitioned
        yield return new WaitForSeconds(2f);
        while (true)
        {
            yield return new WaitForSeconds(GetDifficultyScale() > hardDifficultyScale ? 0.5f : 1f);

            if (TrackedActiveEnemies.Count > 2 * GetDifficultyScale())
                continue;

            var randomEnemyType = 0;
            if (GetDifficultyScale() > mediumDifficultyScale)
            {
                randomEnemyType = Random.Range(0, 10) == 0 ? 1 : 0;
            }
            if (GetDifficultyScale() > hardDifficultyScale)
            {
                randomEnemyType = Random.Range(0, 3) == 0 ? 1 : 0;
            }
            var enemy = GetEnemy((Enemy.EnemyType)randomEnemyType);

            //enemy.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
            // screen edges only
            var randomEdge = Random.Range(0, 4);
            // get screen width and hieght
            var width = Screen.width;
            var height = Screen.height;
            // get random position on edge
            Vector3 randomPosition = Vector3.zero;
            switch (randomEdge)
            {
                case 0: // top
                    randomPosition = new Vector3(Random.Range(0f, width), height, 0f);
                    break;
                case 1: // bottom
                    randomPosition = new Vector3(Random.Range(0f, width), 0f, 0f);
                    break;
                case 2: // left
                    randomPosition = new Vector3(0f, Random.Range(0f, height), 0f);
                    break;
                case 3: // right
                    randomPosition = new Vector3(width, Random.Range(0f, height), 0f);
                    break;
            }
            enemy.transform.position = Camera.main.ScreenToWorldPoint(randomPosition);
            enemy.transform.position = new Vector3(enemy.transform.position.x, enemy.transform.position.y, 0f);

            enemy.SetActive(true);

            var enemyComponent = enemy.GetComponent<Enemy>();
            enemyComponent.Init();

            TrackedActiveEnemies.Add(enemyComponent);
        }
    }

    /// <summary>
    /// Something has changed that we should ensure all visual elements are updated / refreshed
    /// </summary>
    public void UpdateState()
    {
        //scoreText.GetComponent<TextMeshProUGUI>().text = "Score: " + CurrentGameData.score.ToString() +
        //    "\nHigh Score: " + CurrentGameData.highScore.ToString() +
        //    "\nTotal Score: " + CurrentGameData.totalScore.ToString();

        scoreText.GetComponent<TextMeshProUGUI>().text = CurrentGameData.score.ToString();
        string comboLetter = notesDisplay[CurrentGameData.combo % 12];
        if (comboLetter.Contains("|"))
        {
            comboLetter = comboLetter.Split('|')[Random.Range(0, 2)];
        }
        if (comboTimer > 0)
        {
            comboDisplay.GetComponent<TextMeshProUGUI>().color = Color.white;
            comboDisplay.GetComponent<TextMeshProUGUI>().text = comboLetter;
            comboMultiDisplay.GetComponent<TextMeshProUGUI>().text = "x" + (CurrentGameData.combo / 12 + 1).ToString();
        }
        else
        {
            comboDisplay.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0.4f);
            comboDisplay.GetComponent<TextMeshProUGUI>().text = "N/A";
            comboMultiDisplay.GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    public void DisplayConstantGraphics()
    {
        comboBarDisplay.fillAmount = Mathf.Lerp(comboBarDisplay.fillAmount, comboTimer / comboTimerMax, Time.deltaTime * 2f);

        if (comboBarDisplay.fillAmount < 0.17f)
        {
            comboBarDisplay.color = Color.Lerp(Color.clear, Color.red, comboBarDisplay.fillAmount / 0.17f);
        }
        else
        {
            comboBarDisplay.color = Color.white;
        }
    }

    /// <summary>
    /// Player died
    /// </summary>
    public void GameOver()
    {
        CurrentGameState = GameState.GameOver;
        if (spawnEnemyCoroutine != null)
            StopCoroutine(spawnEnemyCoroutine);
        SaveGame();

        // If we want enemies to disappear on game over
        /*while (TrackedActiveEnemies.Count > 0)
        {
            var enemy = TrackedActiveEnemies[0];
            RemoveEnemy(enemy);
        }*/
        playing = false;

        //scoreText.SetActive(false);
        GameUIDisplay.SetInteger("displayState", 0);

        GameOverUI.SetActive(true);
        gameOverScoreText.GetComponent<TextMeshProUGUI>().color = overridePlayerColor ?? player.PlayerColor;
        gameOverScoreText.GetComponent<TextMeshProUGUI>().text = "SCORE: " + CurrentGameData.score.ToString() +
        "\nHIGH SCORE: " + CurrentGameData.highScore.ToString() +
        "\nHIGHEST COMBO: " + CurrentGameData.gameHighestCombo.ToString() +
        "\nALL TIME BEST COMBO: " + CurrentGameData.highestCombo.ToString() +
        "\nTOTAL SCORE: " + CurrentGameData.totalScore.ToString();

        gameOverScoreText.SetActive(true);
        //mainMenu.SetActive(true);
        creditsUI.SetActive(true);
        //optionsBtn.SetActive(true);
        optionsPanel.SetActive(false);

        postProcessingManager.GameOver();

        backgroundEnviromentManager.GameOver();
        StartCoroutine(GameOverTimeEffect());
        // Now that we cover screen, looks weird kicking player around
        StartCoroutine(DelayedAfterGameCleanup());
        audioManager.GameOver();

        lastRoundEndTime = DateTime.Now;
    }

    IEnumerator GameOverTimeEffect()
    {
        Time.timeScale = 0f;
        for (float i = 0f; i < 0.5f; i += Time.unscaledDeltaTime / 2)
        {
            Time.timeScale = Mathf.Lerp(0, 0.5f, i);
            yield return null;
        }
        // animation control
        for (float i = 0f; i < 1; i += Time.unscaledDeltaTime * 5)
        {
            Time.timeScale = Mathf.Lerp(0.5f, 1f, i);
            yield return null;
        }
        Time.timeScale = 1f;
    }

    IEnumerator DelayedAfterGameCleanup()
    {
        yield return new WaitForSeconds(3f);
        CleanupLastGame();
    }

    public void AddScore(int score = 1)
    {
        CurrentGameData.score += score;

        if (CurrentGameData.score > CurrentGameData.highScore)
        {
            CurrentGameData.highScore = CurrentGameData.score;
        }
        CurrentGameData.totalScore += score;

        UpdateState();
    }

    public void AddMultikill(int multikill)
    {
        Debug.Log("Multikill of " + multikill.ToString());
        int theScore = (int)Mathf.Pow(multikill, 2);
        CurrentGameData.score += theScore;
        if (CurrentGameData.score > CurrentGameData.highScore)
        {
            CurrentGameData.highScore = CurrentGameData.score;
        }
        CurrentGameData.totalScore += theScore;

        UpdateState();
    }

    public void DoCombo(Enemy killedEnemy)
    {
        bool newCombo = false;
        // No Combo
        if (comboTimer <= 0)
        {
            Debug.Log("No Combo - starting new one");
            newCombo = true;
            comboActive = true;
        }
        if (newCombo)
        {
            CurrentGameData.combo = 0;
            comboTimer = comboTimerMax;
        }
        else
        // Continuing combo
        {
            CurrentGameData.combo++;
            float plusTimerAmount = comboTimerMax;
            if (Instance.GetDifficultyScale() > Instance.hardDifficultyScale)
                plusTimerAmount -= Instance.GetDifficultyScale();
            else if (CurrentGameData.combo > 10 && Instance.GetDifficultyScale() > Instance.mediumDifficultyScale)
                plusTimerAmount = comboTimerMax - 1;

            comboTimer += plusTimerAmount;
        }
        if (comboTimer > comboTimerMax)
        {
            comboTimer = comboTimerMax;
        }

        if (CurrentGameData.combo > CurrentGameData.gameHighestCombo)
            CurrentGameData.gameHighestCombo = CurrentGameData.combo;

        if (CurrentGameData.combo > CurrentGameData.highestCombo)
            CurrentGameData.highestCombo = CurrentGameData.combo;

        PlayComboSound();

        UpdateState();
    }

    /// <summary>
    /// Temporary sound effect for combo
    /// </summary>
    public void PlayComboSound()
    {
        int step = CurrentGameData.combo % 12;
        int octaveLevel = Mathf.Clamp(CurrentGameData.combo / 12, 0, 3);
        gameComboSoundSource.clip = octaveLevel switch
        {
            0 => octave1Noise,
            1 => octave2Noise,
            2 => octave3Noise,
            3 => octave4Noise,
            _ => octave1Noise,
        };
        gameComboSoundSource.pitch = Mathf.Pow(2f, (step - 9f) / 12f);
        //gameComboSoundSource.pitch = Mathf.Pow(2f, (step - 9f) / 12f) * (1f + octaveLevel);
        gameComboSoundSource.Play();

    }

    /// <summary>
    /// Save current state of CurrentGameData to disk
    /// </summary>
    public void SaveGame()
    {
#if !UNITY_WEBGL || UNITY_ANDROID
        // Create a binary formatter
        BinaryFormatter bf = new BinaryFormatter();
        // Create a file at the path
        FileStream file = File.Create(Application.persistentDataPath + "/slash.dot");
        // Populate the object with the current game data
        //gameData.score = player.Score;
        //CurrentGameData.score = player.Score;
        // Serialize the object to the file
        bf.Serialize(file, CurrentGameData);
        // Close the file
        file.Close();
#else
        // use playerprefs
        PlayerPrefs.SetInt("highScore", CurrentGameData.highScore);
        PlayerPrefs.SetInt("totalScore", CurrentGameData.totalScore);
        PlayerPrefs.SetInt("highestCombo", CurrentGameData.highestCombo);
#endif
    }

    /// <summary>
    /// Load game data. CurrentGameData now ready to use
    /// </summary>
    public void LoadGame()
    {
#if !UNITY_WEBGL || UNITY_ANDROID
        // Check if the file exists
        if (File.Exists(Application.persistentDataPath + "/slash.dot"))
        {
            // Create a binary formatter
            BinaryFormatter bf = new BinaryFormatter();
            // Open the file
            FileStream file = File.Open(Application.persistentDataPath + "/slash.dot", FileMode.Open);
            // Deserialize the file into the object
            CurrentGameData = (GameData)bf.Deserialize(file);

            Debug.Log("Loaded game data with high score " + CurrentGameData.highScore.ToString());
            Debug.Log("Loaded game data with total score " + CurrentGameData.totalScore.ToString());
            // Close the file
            file.Close();
        }
        else
        {
            // Create a new game data object
            CurrentGameData = new GameData();
            Debug.Log("No save file found, creating new game data");
        }
#else 
        // use playerprefs
        CurrentGameData = new GameData
        {
            highScore = PlayerPrefs.GetInt("highScore", 0),
            totalScore = PlayerPrefs.GetInt("totalScore", 0),
            highestCombo = PlayerPrefs.GetInt("highestCombo", 0)
        };
#endif
    }

    public float GetDifficultyScale()
    {
        //Debug.Log("Fix this for release. We are scaling up quickly <color=yellow>for testing</color> | Difficulty Scale: " + Mathf.Clamp(1f + (currentRoundTimer / 10f), 1f, maxDifficultyScale).ToString());
        return Mathf.Clamp(1f + (currentRoundTimer / 10f), 1f, maxDifficultyScale);
    }

    /// <summary>
    /// Used by options btn
    /// </summary>
    public void _ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void SetPlayerPositionForMaterial(Vector3 position)
    {
        backgroundEnviromentManager.SetPlayerPosition(position);
    }


    public void _ShowColorPanel(GameObject colorPickingBtn)
    {
        colorPanel.SetActive(true);
        colorPickingBtn.GetComponent<ColorPickerBtn>().ColorPickerIsActive = true;
        currentColorPickingBtn = colorPickingBtn;
    }
    public void _DirectHexInputColorPanel(string text)
    {
        if (!text.StartsWith("#"))
            text = "#" + text;
        if (ColorUtility.TryParseHtmlString(text, out Color color))
        {
            currentColorPickingBtn.GetComponent<ColorPickerBtn>().colorPicker.color = color;
            return;
        }
        idek(text);
    }
    public void _CloseColorPanel()
    {
        colorPanel.SetActive(false);
        currentColorPickingBtn.GetComponent<ColorPickerBtn>().ColorPickerIsActive = false;
        currentColorPickingBtn = null;
    }

    public void SetPlayerPrimaryOverrideColor(Color color)
    {
        overridePlayerColor = color;
        player.SetColors();
    }
    public void SetPlayerAttackOverrideColor(Color color)
    {
        overrideAttackColor = color;
        player.SetColors();
    }

    void idek(string text)
    {
        switch (text.Trim().ToLower())
        {
            case "#hi":
                NotificationManager.Instance.ShowNotification("hello :3");
                break;
            case "#hello":
                NotificationManager.Instance.ShowNotification("hi");
                break;
            case "#lol":
                NotificationManager.Instance.ShowNotification("lol");
                break;
        }
        AudioManager.Instance.PlaySoundOnce(AudioManager.SoundType.notification);
    }

    public void _SetDoRandomColors(bool doRandomColors)
    {
        DoRandomColors = doRandomColors;
        if (DoRandomColors)
        {
            overridePlayerColor = null;
            overrideAttackColor = null;
            player.SetColors();
        }
    }

    // OLD DIRECT INPUTS

    /// <summary>
    /// Comes in from input field as a hex string
    /// </summary>
    /// <param name="input"></param>
    public void _SetPlayerColor(string input)
    {
        if (!input.StartsWith("#"))
            input = "#" + input;

        if (ColorUtility.TryParseHtmlString(input, out Color color))
        {
            overridePlayerColor = color;
            player.SetColors();
        }
    }

    /// <summary>
    /// Comes in from input field as a hex string   
    /// </summary>
    /// <param name="input"></param>
    public void _SetAttackColor(string input)
    {
        if (!input.StartsWith("#"))
            input = "#" + input;

        if (ColorUtility.TryParseHtmlString(input, out Color color))
        {
            overrideAttackColor = color;
            player.SetColors();
        }
    }
}
