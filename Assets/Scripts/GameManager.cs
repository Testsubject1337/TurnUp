using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Camera gameCamera;
    public bool GodModeEnabled = false;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    private int currentScore;
    private int highscore;

    public bool IsGameRunning { get; private set; }
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highscore = PlayerPrefs.GetInt("Highscore", 0);
        UpdateScoreUI();
        IsGameRunning = false;
        IsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsPaused)
        {
            TogglePause();
        }
        else if (!Input.GetKeyDown(KeyCode.Escape) && IsPaused)
        {
            TogglePause();
        }

        if (IsGameRunning && !IsPaused)
        {
            GameUpdate();
        }
    }

    private void GameUpdate()
    {
        CheckPlayerBelowScreen();
        IncreaseScore(0);
    }

    private void CheckPlayerBelowScreen()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float lowerScreenBound = Camera.main.transform.position.y - Camera.main.orthographicSize;

        if (player.transform.position.y < lowerScreenBound - 3f)
        {
            Die();
        }
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + currentScore.ToString();
        highscoreText.text = "Highscore: " + highscore.ToString();
    }

    public void StartGame()
    {
        IsGameRunning = true;
    }

    public void IncreaseScore(int amount)
    {
        float cameraPosY = float.Parse(gameCamera.transform.position.y.ToString());

            currentScore = Mathf.RoundToInt(cameraPosY) + amount;


        UpdateScoreUI();
    }

    public void SaveHighscore()
    {
        if (currentScore > highscore)
        {
            highscore = currentScore;
            PlayerPrefs.SetInt("Highscore", highscore);
        }
    }

    private void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    public void Die()
    {
        if (!GodModeEnabled)
        {
            SaveHighscore();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        }
    }

    public int getScore()
    {
        return currentScore;
    }
}
