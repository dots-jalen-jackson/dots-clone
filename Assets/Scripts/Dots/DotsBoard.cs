using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

[DefaultExecutionOrder(0)]
public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;
    [SerializeField] private Color[] _dotColors;
    [SerializeField] private int _seed;

    private RectTransform _rectTransform;
    
    private ObjectPooler _dotsPooler;
    private Dot [,] _dots;
    private Stack<Dot> _prevDots;
    private Stack<Dot> _formedSquareDots;
    private bool[,] _edges;
    private bool[] _visited;

    private int _dotSize;

    private int _numDots => _dotsPooler.ObjectPoolSize;

    private int _dotSpacing => _dotSize * 2;

    public bool IsSquareFormed() => _formedSquareDots.Count > 0;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        _dotsPooler = GetComponent<ObjectPooler>();
        _dotsPooler.ObjectPoolSize = _boardWidth * _boardHeight;
        _dotsPooler.GenerateObjects();

        GameObject dot = _dotsPooler.ObjectPrefab;
        RectTransform dotTransform = dot.GetComponent<RectTransform>();
        _dotSize = (int) dotTransform.rect.width;
    }

    private void Start()
    {
        CenterBoard();
        PopulateDots();
    }

    private void CenterBoard()
    {
        float centerSpacing = (float)_dotSize / 2;
        
        float centerX = -centerSpacing * _boardWidth;
        float centerY = centerSpacing * _boardHeight;
        _rectTransform.anchoredPosition = new Vector2(centerX, centerY);
    }

    private void PopulateDots()
    {
        _dots = new Dot[_boardWidth, _boardHeight];
        _edges = new bool[_numDots * _numDots, _numDots * _numDots];
        _visited = new bool[_numDots];
        _prevDots = new Stack<Dot>();
        _formedSquareDots = new Stack<Dot>();
        
        #if UNITY_EDITOR
            Random.InitState(_seed);
        #endif

        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                GenerateDot(i, j);
            }
        }
    }

    public void ResetBoard()
    {
        ClearEdges();
    }

    public int GetRowAtPosition(Vector2 position)
    {
        float y = position.y;
        return ((int) (_dotSpacing - y) / _numDots);
    }

    public int GetColAtPosition(Vector2 position)
    {
        float x = position.x;
        return ((int) (x + _dotSpacing) / _numDots);
    }
    
    public void AddEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = true;
        _edges[dstIndex, srcIndex] = true;

        _prevDots.Push(src);

        if (CountEdgesAt(dst) > 1)
            _formedSquareDots.Push(dst);
    }

    public void RemoveEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = false;
        _edges[dstIndex, srcIndex] = false;
        
        _prevDots.Pop();

        if (IsSquareFormed())
        {
            if (CountEdgesAt(src) <= 1)
                _formedSquareDots.Pop();
        }
    }

    public bool ContainsEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        return _edges[srcIndex, dstIndex] && _edges[dstIndex, srcIndex];
    }

    public int CountEdgesAt(Dot src)
    {
        int numEdges = 0;

        List<Dot> dotsAroundSrc = GetSameColoredDotsAround(src);
        foreach (Dot dst in dotsAroundSrc)
        {
            if (ContainsEdge(src, dst))
                numEdges++;
        }

        return numEdges;
    }

    public bool IsDotPreviousSource(Dot dot)
    {
        return dot == _prevDots.Peek();
    }

    public List<Dot> GetSameColoredDotsAround(Dot dot)
    {
        List<Dot> neighbors = new List<Dot>();
        
        if (dot.Row - 1 >= 0)
        {
            Dot upDot = _dots[dot.Col, dot.Row - 1];

            if (upDot != null)
            {
                if (upDot.Color == dot.Color)
                    neighbors.Add(upDot);
            }
        }
        
        if (dot.Row + 1 < _boardHeight)
        {
            Dot downDot = _dots[dot.Col, dot.Row + 1];

            if (downDot != null)
            {
                if (downDot.Color == dot.Color)
                    neighbors.Add(downDot);
            }
        }

        if (dot.Col - 1 >= 0)
        {
            Dot leftDot = _dots[dot.Col - 1, dot.Row];

            if (leftDot != null)
            {
                if (leftDot.Color == dot.Color)
                    neighbors.Add(leftDot);
            }
        }
        
        if (dot.Col + 1 < _boardWidth)
        {
            Dot rightDot = _dots[dot.Col + 1, dot.Row];

            if (rightDot != null)
            {
                if (rightDot.Color == dot.Color)
                    neighbors.Add(rightDot);
            }
        }

        return neighbors;
    }

    public List<Dot> GetDotsWithColor(Color color)
    {
        List<Dot> dots = new List<Dot>();
        
        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                Dot dot = _dots[i, j];

                if (dot == null)
                    continue;
                
                if (dot.Color == color)
                    dots.Add(dot);
            }
        }

        return dots;
    }

    public List<Dot> GetDotsInLineFrom(Dot src)
    {
        List<Dot> dots = new List<Dot>();
        
        Stack<Dot> stack = new Stack<Dot>();
        stack.Push(src);

        while (stack.Count > 0)
        {
            Dot cur = stack.Pop();
            dots.Add(cur);
            
            int curIndex = GetIndex(cur);
            _visited[curIndex] = true;

            List<Dot> dsts = GetSameColoredDotsAround(cur);
            foreach (Dot dst in dsts)
            {
                if (!ContainsEdge(cur, dst))
                    continue;
                
                int dstIndex = GetIndex(dst);
                if (_visited[dstIndex])
                    continue;
                
                stack.Push(dst);
            }
        }
        
        UnvisitAllDots();
        
        if (dots.Count <= 1)
            dots.Clear();
        
        return dots;
    }

    public void RemoveDots(List<Dot> dots)
    {
        HashSet<int> cols = new HashSet<int>();
        dots.ForEach((dot) =>
        {
            int row = dot.Row;
            int col = dot.Col;
            
            _dotsPooler.ReturnPooledObject(dot.gameObject);
            _dots[col, row] = null;

            cols.Add(col);
        });

        foreach (int col in cols)
            DropDotsDown(col);
    }

    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }

    private float GetXAt(int col)
    {
        return (_dotSpacing * col) - _dotSpacing;
    }

    private float GetYAt(int row)
    {
        return _dotSpacing - (_dotSpacing * row);
    }

    private Color GenerateRandomColor()
    {
        Color color = _dotColors[Random.Range(0, _dotColors.Length)];
        return color;
    }
    
    private void GenerateDot(int col, int row)
    {
        if (row < 0 || row >= _boardHeight || col < 0 || col >= _boardWidth)
            return;

        GameObject dotObject = _dotsPooler.GetPooledObject();
        if (dotObject == null)
            return;
        
        _dots[col, row] = dotObject.GetComponent<Dot>();
        _dots[col, row].Color = GenerateRandomColor();
        _dots[col, row].Position = new Vector2(GetXAt(col), GetYAt(row));
        _dots[col, row].gameObject.SetActive(true);
    }
    
    //TODO Generate dots after dropping them
    private void DropDotsDown(int col)
    {
        int startRow = _boardHeight - 1;

        int currentRow = startRow;
        while (currentRow >= 0 && _dots[col, currentRow] != null)
            currentRow--;

        int dotShiftCount = 0;
        while (currentRow >= 0 && _dots[col, currentRow] == null)
        {
            currentRow--;
            dotShiftCount++;
        }

        do
        {
            while (currentRow >= 0 && _dots[col, currentRow] != null)
            {
                Dot movedDot = _dots[col, currentRow];
                float movedY = movedDot.Position.y - (_dotSpacing * dotShiftCount);
                if (movedY < GetYAt(_boardHeight - 1))
                    movedY = GetYAt(_boardHeight - 1);

                movedDot.Position = new Vector2(movedDot.Position.x, movedY);

                _dots[col, movedDot.Row] = _dots[col, currentRow];
                _dots[col, currentRow] = null;

                currentRow--;
            }

            while (currentRow >= 0 && _dots[col, currentRow] == null)
            {
                currentRow--;
                dotShiftCount++;
            }
            
        } while (currentRow >= 0);

        currentRow = 0;
        while (currentRow < _boardHeight && _dots[col, currentRow] == null)
        {
            GenerateDot(col, currentRow);
            currentRow++;
        }
    }

    private int GetNumMissingDotsInCol(int col)
    {
        int count = 0;

        for (int row = 0; row < _boardHeight; row++)
        {
            Dot dot = _dots[col, row];
            if (dot != null)
                continue;

            count++;
        }

        return count;
    }
    
    private void ClearEdges()
    {
        _prevDots.Clear();
        _formedSquareDots.Clear();
        for (int i = 0; i < _numDots * _numDots; i++)
        {
            for (int j = 0; j < _numDots * _numDots; j++)
                _edges[i, j] = false;
        }
    }

    private void UnvisitAllDots()
    {
        for (int i = 0; i < _numDots; i++)
        {
            _visited[i] = false;
        }
    }
}
