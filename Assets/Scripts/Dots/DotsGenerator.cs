using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class DotsGenerator : Singleton<DotsGenerator>
{
    private ObjectPooler _dotsPooler;

    private int _dotSize;

    public int DotSize => _dotSize;

    public int DotSpacing => _dotSize * 2;
    
    public int NumDots => _dotsPooler.ObjectPoolSize;

    public void Start()
    {
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardWidth = DotsBoard.Instance.BoardWidth;
        
        _dotsPooler = GetComponent<ObjectPooler>();
        _dotsPooler.ObjectPoolSize = boardWidth * boardHeight;
        _dotsPooler.GenerateObjects();

        GameObject dotObject = _dotsPooler.ObjectPrefab;
        Dot dot = dotObject.GetComponent<Dot>();
        _dotSize = (int) dot.Size;
    }
    
    public Dot InitializeDotAtPosition(int col, int row, Vector2 startPosition)
    {
        Dot dot = CreateDot(col, row);
        if (dot == null)
            return null;
        
        dot.Position = startPosition;
        dot.Col = col;

        return dot;
    }

    public void ReturnDotToPool(int col, int row)
    {
        Dot dot = DotsBoard.Instance.GetDotAt(col, row);
        if (dot == null)
            return;
        
        _dotsPooler.ReturnPooledObject(dot.gameObject);
    }

    private Dot CreateDot(int col, int row)
    {
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardWidth = DotsBoard.Instance.BoardWidth;
        
        if (row < 0 || row >= boardHeight || col < 0 || col >= boardWidth)
            return null;

        GameObject dotObject = _dotsPooler.GetPooledObject();
        if (dotObject == null)
            return null;

        Dot dot = dotObject.GetComponent<Dot>();
        DotsBoard.Instance.PlaceDotAt(col, row, dot);
        dotObject.SetActive(true);

        return DotsBoard.Instance.GetDotAt(col, row);
    }
}
