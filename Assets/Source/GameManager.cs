using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Режим игры")]
    public GameMode gameMode = GameMode.Moves;
    
    [Header("Настройки режима ходов")]
    public int maxMoves = 30;
    
    [Header("Настройки режима времени")]
    public float gameTime = 60f;

    [Header("Ссылки")]
    public Board board;
    public ScoreManager scoreManager;

    // Текущее состояние
    public GameState State { get; private set; } = GameState.Playing;
    public int MovesRemaining { get; private set; }
    public float TimeRemaining { get; private set; }

    // События для UI
    public event System.Action<int> OnMovesChanged;
    public event System.Action<float> OnTimeChanged;
    public event System.Action<GameState> OnGameStateChanged;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (State != GameState.Playing) return;
        
        if (gameMode == GameMode.Time)
        {
            TimeRemaining -= Time.deltaTime;
            OnTimeChanged?.Invoke(TimeRemaining);
            
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame();
            }
        }
    }

    void OnEnable()
    {
        if (board != null)
        {
            board.OnMoveCompleted += HandleMoveCompleted;
        }
    }

    void OnDisable()
    {
        if (board != null)
        {
            board.OnMoveCompleted -= HandleMoveCompleted;
        }
    }

    // Инициализирует новую игру
    public void InitializeGame()
    {
        MovesRemaining = maxMoves;
        TimeRemaining = gameTime;
        State = GameState.Playing;
        
        OnMovesChanged?.Invoke(MovesRemaining);
        OnTimeChanged?.Invoke(TimeRemaining);
        OnGameStateChanged?.Invoke(State);
    }

    // Обработка завершённого хода
    void HandleMoveCompleted()
    {
        if (gameMode != GameMode.Moves) return;
        if (State != GameState.Playing) return;
        
        MovesRemaining--;
        OnMovesChanged?.Invoke(MovesRemaining);
        
        if (MovesRemaining <= 0)
        {
            EndGame();
        }
    }

    // Завершает игру
    void EndGame()
    {
        State = GameState.GameOver;
        OnGameStateChanged?.Invoke(State);
        
        Debug.Log($"Game Over! Final Score: {scoreManager?.Score ?? 0}");
    }

    // Перезапускает игру
    public void RestartGame()
    {
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        if (board != null)
        {
            board.ResetBoard();
        }
        
        InitializeGame();
    }

    // Ставит игру на паузу
    public void PauseGame()
    {
        if (State == GameState.Playing)
        {
            State = GameState.Paused;
            Time.timeScale = 0f;
            OnGameStateChanged?.Invoke(State);
        }
    }

    // Возобновляет игру
    public void ResumeGame()
    {
        if (State == GameState.Paused)
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
            OnGameStateChanged?.Invoke(State);
        }
    }
}

public enum GameMode
{
    Moves,
    Time
}

public enum GameState
{
    Playing,
    Paused,
    GameOver
}
