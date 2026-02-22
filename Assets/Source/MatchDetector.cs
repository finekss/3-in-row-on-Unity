using UnityEngine;
using System.Collections.Generic;

public class MatchDetector : MonoBehaviour
{
    // Находит все совпадения (3+ в ряд) на доске
    public HashSet<Gem> FindAllMatches(Gem[,] gems, int width, int height)
    {
        HashSet<Gem> allMatches = new HashSet<Gem>();
        
        FindHorizontalMatches(gems, width, height, allMatches);
        FindVerticalMatches(gems, width, height, allMatches);
        
        return allMatches;
    }

    // Поиск горизонтальных совпадений
    private void FindHorizontalMatches(Gem[,] gems, int width, int height, HashSet<Gem> matches)
    {
        for (int y = 0; y < height; y++)
        {
            int matchStart = 0;
            int matchType = -1;
            
            for (int x = 0; x < width; x++)
            {
                Gem gem = gems[x, y];
                
                if (gem == null)
                {
                    AddMatchIfValid(gems, matchStart, x - 1, y, true, matches);
                    matchStart = x + 1;
                    matchType = -1;
                    continue;
                }
                
                if (gem.gemType != matchType)
                {
                    AddMatchIfValid(gems, matchStart, x - 1, y, true, matches);
                    matchStart = x;
                    matchType = gem.gemType;
                }
            }
            
            AddMatchIfValid(gems, matchStart, width - 1, y, true, matches);
        }
    }

    // Поиск вертикальных совпадений
    private void FindVerticalMatches(Gem[,] gems, int width, int height, HashSet<Gem> matches)
    {
        for (int x = 0; x < width; x++)
        {
            int matchStart = 0;
            int matchType = -1;
            
            for (int y = 0; y < height; y++)
            {
                Gem gem = gems[x, y];
                
                if (gem == null)
                {
                    AddMatchIfValid(gems, matchStart, y - 1, x, false, matches);
                    matchStart = y + 1;
                    matchType = -1;
                    continue;
                }
                
                if (gem.gemType != matchType)
                {
                    AddMatchIfValid(gems, matchStart, y - 1, x, false, matches);
                    matchStart = y;
                    matchType = gem.gemType;
                }
            }
            
            AddMatchIfValid(gems, matchStart, height - 1, x, false, matches);
        }
    }

    // Добавляет гемы в matches, если серия >= 3
    private void AddMatchIfValid(Gem[,] gems, int start, int end, int fixedCoord, bool isHorizontal, HashSet<Gem> matches)
    {
        int length = end - start + 1;
        if (length < 3) return;
        
        for (int i = start; i <= end; i++)
        {
            Gem gem = isHorizontal ? gems[i, fixedCoord] : gems[fixedCoord, i];
            if (gem != null)
            {
                matches.Add(gem);
            }
        }
    }

    // Проверяет, создаст ли гем определённого типа совпадение на позиции
    public bool WouldCreateMatch(Gem[,] gems, int width, int height, int x, int y, int gemType)
    {
        if (x >= 2 && MatchType(gems, x - 1, y, gemType) && MatchType(gems, x - 2, y, gemType))
            return true;
        
        if (x >= 1 && x < width - 1 && MatchType(gems, x - 1, y, gemType) && MatchType(gems, x + 1, y, gemType))
            return true;
        
        if (x < width - 2 && MatchType(gems, x + 1, y, gemType) && MatchType(gems, x + 2, y, gemType))
            return true;
        
        if (y >= 2 && MatchType(gems, x, y - 1, gemType) && MatchType(gems, x, y - 2, gemType))
            return true;
        
        if (y >= 1 && y < height - 1 && MatchType(gems, x, y - 1, gemType) && MatchType(gems, x, y + 1, gemType))
            return true;
        
        if (y < height - 2 && MatchType(gems, x, y + 1, gemType) && MatchType(gems, x, y + 2, gemType))
            return true;
        
        return false;
    }

    // Проверяет, совпадает ли тип гема на позиции с заданным
    private bool MatchType(Gem[,] gems, int x, int y, int gemType)
    {
        if (gems[x, y] == null) return false;
        return gems[x, y].gemType == gemType;
    }
}
