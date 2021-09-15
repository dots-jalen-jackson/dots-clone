using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotsBoardUpdater : Singleton<DotsBoardUpdater>
{
    [SerializeField] private float _dotPopulateSpeed = 6.0f;
    [SerializeField] private float _dotDropSpeed = 9.0f;

    private float _dotSpawnPositionY;

    public void StartPopulatingBoard()
    {
        StartCoroutine(PopulateBoard());
    }
    
    public void StartHighlightingDots(List<Dot> dots)
    {
        foreach (Dot dot in dots)
        {
            dot.Highlight();
        }
    }
    
    public void StartRemoveDot(Dot src)
    {
        src.Shrink(() =>
        {
            int row = src.Row;
            int col = src.Col;

            DotsGenerator.Instance.ReturnDotToPool(col, row);
            
            StartDroppingDotsDownInCol(col);
        });
    }

    public void StartRemovingDots(List<Dot> dots)
    {
        if (dots.Count <= 0)
            return;

        DotsBoard.Instance.ClearEdges();

        foreach (Dot dot in dots)
        {
            dot.StopAllCoroutines();
            dot.Reset();
        }
        
        StartShrinkingDots(dots);
    }
    
    private void StartDroppingDotsDownInCol(int col)
    {
        StartCoroutine(DropDotsDown(col));
    }

    private void StartShrinkingDots(List<Dot> dots)
    {
        StartCoroutine(ShrinkDots(dots, StartDroppingDotsDown));
    }
    
    private void StartDroppingDotsDown(HashSet<int> cols)
    {
        foreach (int col in cols)
            StartDroppingDotsDownInCol(col);
    }
    
    private IEnumerator PopulateBoard()
    {
        int boardWidth = DotsBoard.Instance.BoardWidth;
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardSpacing = DotsGenerator.Instance.DotSpacing;
        
        _dotSpawnPositionY = boardSpacing * 4.0f;

        int row = boardHeight - 1;
        while (row >= 0)
        {
            for (int col = 0; col < boardWidth; col++)
            {
                float x = DotsBoard.Instance.GetXAt(col);
                float y = DotsBoard.Instance.GetYAt(row);
                float topY = DotsBoard.Instance.GetYAt(boardHeight);
                
                Vector2 startPosition = new Vector2(x, (topY - y) + _dotSpawnPositionY * (boardHeight / 2.0f));
                Vector2 targetPosition = new Vector2(x, y);

                Dot dot = DotsGenerator.Instance.InitializeDotAtPosition(col, row, startPosition);
                StartCoroutine(dot.MoveTo(targetPosition, _dotPopulateSpeed));
            }

            yield return new WaitUntil(() => DotsBoard.Instance.IsAllDotsOnRowAtTarget(row));
            
            row--;
        }

        DotsInputHandler.Instance.IsInputEnabled = true;
    }
    
    private IEnumerator ShrinkDots(List<Dot> dots, Action<HashSet<int>> onShrinkCompleted = null)
    {
        int removedDotsCount = 0;
        HashSet<int> cols = new HashSet<int>();

        foreach (Dot dot in dots)
        {
            dot.Shrink(() =>
            {
                int row = dot.Row;
                int col = dot.Col;

                DotsGenerator.Instance.ReturnDotToPool(col, row);

                cols.Add(col);

                removedDotsCount++;
            });
        }

        yield return new WaitUntil(() => removedDotsCount == dots.Count);

        onShrinkCompleted?.Invoke(cols);
    }
    
    private IEnumerator DropDotsDown(int col)
    {
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int startRow = boardHeight - 1;

        int currentRow = startRow;
        while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            currentRow--;

        int dotShiftCount = 0;
        while (currentRow >= 0 && !DotsBoard.Instance.ContainsDotIn(col, currentRow))
        {
            currentRow--;
            dotShiftCount++;
        }
        
        int dotSpacing = DotsGenerator.Instance.DotSpacing;
        
        do
        {
            while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            {
                Dot currentDot = DotsBoard.Instance.GetDotAt(col, currentRow);
                
                
                float endY = currentDot.Position.y - (dotSpacing * dotShiftCount);
                if (endY < DotsBoard.Instance.GetYAt(boardHeight - 1))
                    endY = DotsBoard.Instance.GetYAt(boardHeight - 1);
                
                Vector2 currentDotEndPosition = new Vector2(currentDot.Position.x, endY);

                yield return currentDot.MoveTo(currentDotEndPosition, _dotDropSpeed);

                DotsBoard.Instance.ShiftDotDown(col, currentRow, currentDot.Row);

                currentRow--;
            }
            
            while (currentRow >= 0 && !DotsBoard.Instance.ContainsDotIn(col, currentRow))
            {
                currentRow--;
                dotShiftCount++;
            }
            
        } while (currentRow >= 0);

        currentRow = startRow;
        while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            currentRow--;

        while (currentRow >= 0 && !DotsBoard.Instance.ContainsDotIn(col, currentRow))
        {
            float x = DotsBoard.Instance.GetXAt(col);
            Vector2 startPosition = new Vector2(x, _dotSpawnPositionY);
            
            yield return GenerateDotAtPosition(col, currentRow, startPosition, _dotDropSpeed);
            currentRow--;
        }
    }
    
    private IEnumerator GenerateDotAtPosition(int col, int row, Vector2 startPosition, float moveSpeed)
    {
        Dot newDot = DotsGenerator.Instance.InitializeDotAtPosition(col, row, startPosition);
        if (newDot == null)
            yield break;

        DotsBoard.Instance.PlaceDotAt(col, row, newDot);

        Vector2 targetPosition = new Vector2(newDot.Position.x, DotsBoard.Instance.GetYAt(row));

        yield return newDot.MoveTo(targetPosition, moveSpeed);
    }
}
