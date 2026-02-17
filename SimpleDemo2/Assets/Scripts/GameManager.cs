using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject gamePanel;

    // Singleton instance for easy access
    public static GameManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        SettingsManager.OnSettingsLoaded += OnSettingsLoaded;
    }

    private void OnSettingsLoaded(Settings settings)
    {
        SetGameIsStarted(settings.gameIsStarted);

        if (settings.gameIsStarted)
            LoadGame();
        else
            Restart();
    }


    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) Quit();
    }

    // Start the game
    public void NormalStartGame()
    {
        introPanel.SetActive(false);
        gamePanel.SetActive(true);
        SetGameIsStarted(true);
        AudioManager.Instance.PlayMenuSfx(GameSfx.Start);
        CardsManager.Instance.CreatePairings();
    }

    public void LoadGame()
    {
        introPanel.SetActive(false);
        gamePanel.SetActive(true);

        CardsManager.Instance.LoadPairings();
    }

    public void Restart()
    {
        DOTween.KillAll();
        Debug.Log("Restart Game...");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Destroy(gameObject);

        introPanel.SetActive(true);
        gamePanel.SetActive(false);
        CardsManager.Instance.LoadToggleGroup();
        SetGameIsStarted(false);
        CardsManager.Instance.ClearGrid();
    }

    private void Quit()
    {
        DOTween.KillAll();
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void SetGameIsStarted(bool value)
    {
        SettingsManager.Instance.settings.gameIsStarted = value;
        SettingsManager.Instance.SaveSettingsFile();
        Debug.Log("Game Started: " + SettingsManager.Instance.settings.gameIsStarted);
    }
}