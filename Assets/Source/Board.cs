using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Размер поля")]
    public int width = 8;
    public int height = 8;

    [Header("Настройки отображения")]
    public float tileSpacing = 1.1f;

    [Header("Префабы гемов")]
    public GameObject[] gemPrefabs;
    
    [Header("Компоненты")]
    public MatchDetector matchDetector;

    public Gem[,] gems { get; private set; }
    public bool IsProcessing { get; private set; } = false;

    public event System.Action<int> OnGemsDestroyed;
    public event System.Action OnMoveCompleted;

    #region Инициализация

    void Awake()
    {
        if (matchDetector == null)
            matchDetector = GetComponent<MatchDetector>();
    }

    void Start()
    {
        gems = new Gem[width, height];
        GenerateBoard();
    }

    // Генерирует начальное поле без совпадений
    void GenerateBoard()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateGemAt(x, y, false);
            }
        }
    }

    #endregion

    #region Создание гемов

    // Создаёт гем на указанной позиции
    void CreateGemAt(int x, int y, bool spawnAbove)
    {
        int gemType = GetSafeGemType(x, y);
        
        GameObject gemObj = Instantiate(gemPrefabs[gemType], transform);
        
        Vector3 spawnPos = spawnAbove 
            ? GetWorldPosition(x, height + 1) 
            : GetWorldPosition(x, y);
        gemObj.transform.localPosition = spawnPos;
        
        Gem gem = gemObj.GetComponent<Gem>();
        gem.Initialize(gemType, x, y);
        gems[x, y] = gem;
    }

    // Возвращает тип гема, который не создаст совпадение
    int GetSafeGemType(int x, int y)
    {
        List<int> availableTypes = new List<int>();
        
        for (int i = 0; i < gemPrefabs.Length; i++)
        {
            if (!matchDetector.WouldCreateMatch(gems, width, height, x, y, i))
            {
                availableTypes.Add(i);
            }
        }
        
        if (availableTypes.Count == 0)
        {
            return Random.Range(0, gemPrefabs.Length);
        }
        
        return availableTypes[Random.Range(0, availableTypes.Count)];
    }

    #endregion

    #region Позиционирование

    // Конвертирует координаты сетки в мировые координаты
    public Vector3 GetWorldPosition(int x, int y)
    {
        float offsetX = (width - 1) * tileSpacing / 2f;
        float offsetY = (height - 1) * tileSpacing / 2f;
        return new Vector3(x * tileSpacing - offsetX, y * tileSpacing - offsetY, 0f);
    }

    // Проверяет, являются ли два гема соседними
    public bool AreAdjacent(Gem a, Gem b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    #endregion

    #region Обмен гемов

    // Попытка обменять два гема местами
    public void TrySwapGems(Gem a, Gem b)
    {
        if (IsProcessing) return;
        if (!AreAdjacent(a, b)) return;
        
        StartCoroutine(SwapSequence(a, b));
    }

    IEnumerator SwapSequence(Gem a, Gem b)
    {
        IsProcessing = true;
        
        SwapInArray(a, b);
        yield return StartCoroutine(AnimateSwap(a, b));
        HashSet<Gem> matches = matchDetector.FindAllMatches(gems, width, height);
        
        if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessCascade());
            OnMoveCompleted?.Invoke();
        }
        else
        {
            SwapInArray(a, b);
            yield return StartCoroutine(AnimateSwap(a, b));
        }
        
        IsProcessing = false;
    }

    // Меняет два гема местами в массиве
    void SwapInArray(Gem a, Gem b)
    {
        int ax = a.x, ay = a.y;
        int bx = b.x, by = b.y;
        
        gems[ax, ay] = b;
        gems[bx, by] = a;
        a.SetPosition(bx, by);
        b.SetPosition(ax, ay);
    }

    #endregion

    #region Обработка совпадений

    // Обрабатывает каскад: удаление -> падение -> новые совпадения
    IEnumerator ProcessCascade()
    {
        HashSet<Gem> matches = matchDetector.FindAllMatches(gems, width, height);
        
        while (matches.Count > 0)
        {
            int destroyedCount = DestroyGems(matches);
            OnGemsDestroyed?.Invoke(destroyedCount);
            
            yield return new WaitForSeconds(0.15f);
            
            yield return StartCoroutine(CollapseAndFill());
            
            yield return new WaitForSeconds(0.1f);
            
            matches = matchDetector.FindAllMatches(gems, width, height);
        }
    }

    // Удаляет гемы из множества
    int DestroyGems(HashSet<Gem> gemsToDestroy)
    {
        int count = 0;
        foreach (Gem gem in gemsToDestroy)
        {
            if (gems[gem.x, gem.y] == gem)
            {
                gems[gem.x, gem.y] = null;
                count++;
            }
            Destroy(gem.gameObject);
        }
        return count;
    }

    #endregion

    #region Падение и заполнение

    // Обрушивает столбцы и создаёт новые гемы
    IEnumerator CollapseAndFill()
    {
        CollapseColumns();
        yield return StartCoroutine(AnimateAllToPosition());
        FillEmptySpaces();
        yield return StartCoroutine(AnimateAllToPosition());
    }

    // Сдвигает гемы вниз, заполняя пустоты
    void CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int writeIndex = 0;
            
            for (int y = 0; y < height; y++)
            {
                if (gems[x, y] != null)
                {
                    if (y != writeIndex)
                    {
                        gems[x, writeIndex] = gems[x, y];
                        gems[x, writeIndex].SetPosition(x, writeIndex);
                        gems[x, y] = null;
                    }
                    writeIndex++;
                }
            }
        }
    }

    // Заполняет пустые ячейки новыми гемами
    void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyCount = 0;
            for (int y = 0; y < height; y++)
            {
                if (gems[x, y] == null)
                {
                    CreateGemAt(x, y, true);
                    
                    gems[x, y].transform.localPosition = GetWorldPosition(x, height + emptyCount);
                    emptyCount++;
                }
            }
        }
    }

    #endregion

    #region Анимации

    // Анимация обмена двух гемов
    IEnumerator AnimateSwap(Gem a, Gem b)
    {
        Vector3 posA = GetWorldPosition(a.x, a.y);
        Vector3 posB = GetWorldPosition(b.x, b.y);
        Vector3 startA = a.transform.localPosition;
        Vector3 startB = b.transform.localPosition;
        
        float duration = 0.15f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            a.transform.localPosition = Vector3.Lerp(startA, posA, t);
            b.transform.localPosition = Vector3.Lerp(startB, posB, t);
            
            yield return null;
        }
        
        a.transform.localPosition = posA;
        b.transform.localPosition = posB;
    }

    // Анимирует все гемы к их целевым позициям
    IEnumerator AnimateAllToPosition()
    {
        List<Gem> gemsToMove = new List<Gem>();
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> endPositions = new List<Vector3>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gems[x, y] != null)
                {
                    Vector3 target = GetWorldPosition(x, y);
                    Vector3 current = gems[x, y].transform.localPosition;
                    
                    if (Vector3.Distance(current, target) > 0.01f)
                    {
                        gemsToMove.Add(gems[x, y]);
                        startPositions.Add(current);
                        endPositions.Add(target);
                    }
                }
            }
        }
        
        if (gemsToMove.Count == 0) yield break;
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            for (int i = 0; i < gemsToMove.Count; i++)
            {
                if (gemsToMove[i] != null)
                {
                    gemsToMove[i].transform.localPosition = Vector3.Lerp(startPositions[i], endPositions[i], t);
                }
            }
            
            yield return null;
        }
        
        // Финальные позиции
        for (int i = 0; i < gemsToMove.Count; i++)
        {
            if (gemsToMove[i] != null)
            {
                gemsToMove[i].transform.localPosition = endPositions[i];
            }
        }
    }

    #endregion

    #region Публичные методы

    // Перезапуск доски
    public void ResetBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gems[x, y] != null)
                {
                    Destroy(gems[x, y].gameObject);
                    gems[x, y] = null;
                }
            }
        }
        
        GenerateBoard();
    }

    #endregion
}



