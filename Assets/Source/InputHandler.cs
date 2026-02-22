using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Ссылки")]
    public Board board;

    [Header("Настройки")]
    public float minSwipeDistance = 0.3f;

    // Состояние ввода
    private Gem selectedGem;
    private Vector2 startTouchPos;
    private bool isDragging = false;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("InputHandler: Main Camera не найдена! Убедитесь, что камера имеет тег 'MainCamera'.");
        }
    }

    void Update()
    {
        if (board == null || board.IsProcessing) return;

        if (GetTouchDown())
        {
            HandleTouchStart();
        }

        if (GetTouchUp() && isDragging)
        {
            HandleTouchEnd();
        }
    }

    #region Обработка касаний

    // Обработка начала касания
    void HandleTouchStart()
    {
        Vector2 worldPos = GetTouchWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit.collider != null)
        {
            selectedGem = hit.collider.GetComponent<Gem>();
            if (selectedGem != null)
            {
                startTouchPos = worldPos;
                isDragging = true;
            }
        }
    }

    // Обработка окончания касания
    void HandleTouchEnd()
    {
        if (selectedGem == null)
        {
            ResetInput();
            return;
        }

        Vector2 endTouchPos = GetTouchWorldPosition();
        Vector2 swipe = endTouchPos - startTouchPos;

        if (swipe.magnitude >= minSwipeDistance)
        {
            TrySwipe(swipe);
        }

        ResetInput();
    }

    // Попытка выполнить свайп
    void TrySwipe(Vector2 swipe)
    {
        // Определяем направление
        Vector2Int direction = GetSwipeDirection(swipe);
        
        int targetX = selectedGem.x + direction.x;
        int targetY = selectedGem.y + direction.y;

        if (!IsValidPosition(targetX, targetY)) return;

        Gem targetGem = board.gems[targetX, targetY];
        if (targetGem != null)
        {
            board.TrySwapGems(selectedGem, targetGem);
        }
    }

    // Сбрасывает состояние ввода
    void ResetInput()
    {
        selectedGem = null;
        isDragging = false;
    }

    #endregion

    #region Вспомогательные методы

    // Определяет направление свайпа
    Vector2Int GetSwipeDirection(Vector2 swipe)
    {
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            return new Vector2Int(swipe.x > 0 ? 1 : -1, 0);
        }
        else
        {
            return new Vector2Int(0, swipe.y > 0 ? 1 : -1);
        }
    }

    // Проверяет валидность позиции
    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < board.width && y >= 0 && y < board.height;
    }

    #endregion

    #region Абстракция ввода (легко заменить для мобильных)

    // Возвращает true при начале касания
    bool GetTouchDown()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        
        return false;
    }

    // Возвращает true при окончании касания
    bool GetTouchUp()
    {
        if (Input.GetMouseButtonUp(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) return true;
        
        return false;
    }

    // Возвращает позицию касания в мировых координатах
    Vector2 GetTouchWorldPosition()
    {
        if (mainCamera == null) return Vector2.zero;
        
        Vector3 screenPos;
        
        if (Input.touchCount > 0)
        {
            screenPos = Input.GetTouch(0).position;
        }
        else
        {
            screenPos = Input.mousePosition;
        }
        
        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    #endregion
}
