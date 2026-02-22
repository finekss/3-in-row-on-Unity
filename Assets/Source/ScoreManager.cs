using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Настройки очков")]
    public int pointsPerGem = 10;
    public float comboMultiplier = 1.5f;

    // Текущий счёт
    public int Score { get; private set; } = 0;
    
    // Текущее комбо (увеличивается при каскадах)
    public int ComboCount { get; private set; } = 0;

    // Ссылка на Board
    private Board board;

    // События для UI
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnComboChanged;

    [System.Obsolete]
    void Awake()
    {
        board = FindObjectOfType<Board>();
    }

    void OnEnable()
    {
        if (board != null)
        {
            board.OnGemsDestroyed += HandleGemsDestroyed;
            board.OnMoveCompleted += ResetCombo;
        }
    }

    void OnDisable()
    {
        if (board != null)
        {
            board.OnGemsDestroyed -= HandleGemsDestroyed;
            board.OnMoveCompleted -= ResetCombo;
        }
    }

    // Обработка уничтожения гемов
    void HandleGemsDestroyed(int count)
    {
        ComboCount++;
        OnComboChanged?.Invoke(ComboCount);
        
        float multiplier = 1f + (ComboCount - 1) * (comboMultiplier - 1f);
        int points = Mathf.RoundToInt(count * pointsPerGem * multiplier);
        
        AddScore(points);
    }

    // Сброс комбо после завершения хода
    void ResetCombo()
    {
        ComboCount = 0;
        OnComboChanged?.Invoke(ComboCount);
    }

    // Добавляет очки
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    // Сбрасывает счёт
    public void ResetScore()
    {
        Score = 0;
        ComboCount = 0;
        OnScoreChanged?.Invoke(Score);
        OnComboChanged?.Invoke(ComboCount);
    }
}
