using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("Позиция на доске")]
    public int x;
    public int y;
    
    [Header("Тип гема")]
    public int gemType;
    
    private SpriteRenderer spriteRenderer;
    
    public Sprite Sprite => spriteRenderer != null ? spriteRenderer.sprite : GetComponent<SpriteRenderer>().sprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Устанавливает логическую позицию гема на доске
    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    // Инициализирует гем с типом и позицией
    public void Initialize(int type, int posX, int posY)
    {
        gemType = type;
        x = posX;
        y = posY;
    }
}
