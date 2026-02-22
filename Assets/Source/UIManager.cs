using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Текстовые элементы")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI comboText;

    [Header("Панели")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Кнопки")]
    public Button restartButton;
    public Button pauseButton;

    [Header("Ссылки на менеджеры")]
    public GameManager gameManager;
    public ScoreManager scoreManager;

    [System.Obsolete]
    void Awake()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        SetupButtons();
        UpdateInitialValues();
    }

    #region Подписка на события

    void SubscribeToEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += UpdateScore;
            scoreManager.OnComboChanged += UpdateCombo;
        }

        if (gameManager != null)
        {
            gameManager.OnMovesChanged += UpdateMoves;
            gameManager.OnTimeChanged += UpdateTime;
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= UpdateScore;
            scoreManager.OnComboChanged -= UpdateCombo;
        }

        if (gameManager != null)
        {
            gameManager.OnMovesChanged -= UpdateMoves;
            gameManager.OnTimeChanged -= UpdateTime;
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    #endregion

    #region Обновление UI

    void UpdateInitialValues()
    {
        if (scoreManager != null)
        {
            UpdateScore(scoreManager.Score);
            UpdateCombo(scoreManager.ComboCount);
        }

        if (gameManager != null)
        {
            UpdateMoves(gameManager.MovesRemaining);
            UpdateTime(gameManager.TimeRemaining);
        }
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Счёт: {score}";
        }
    }

    void UpdateMoves(int moves)
    {
        if (movesText != null)
        {
            movesText.text = $"Ходы: {moves}";
        }
    }

    void UpdateTime(float time)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = $"Время: {minutes:00}:{seconds:00}";
        }
    }

    void UpdateCombo(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.text = $"Комбо x{combo}!";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Обработка состояния игры

    void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                HideGameOverPanel();
                break;
                
            case GameState.GameOver:
                ShowGameOverPanel();
                break;
                
            case GameState.Paused:
                break;
        }
    }

    void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null && scoreManager != null)
            {
                finalScoreText.text = $"Итоговый счёт: {scoreManager.Score}";
            }
        }
    }

    void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    #endregion

    #region Кнопки

    void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseClicked);
        }
    }

    void OnRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    void OnPauseClicked()
    {
        if (gameManager == null) return;
        
        if (gameManager.State == GameState.Playing)
        {
            gameManager.PauseGame();
        }
        else if (gameManager.State == GameState.Paused)
        {
            gameManager.ResumeGame();
        }
    }

    #endregion
}
