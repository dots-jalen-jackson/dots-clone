using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;

    private RectTransform _rectTransform;

    private Dot[,] _dots;
    private Stack<Dot> _prevDots;
    private bool[,] _edges;
    private bool[] _visited;

    private Dot _formedSquareDot;

    public bool IsSquareFormed { get; private set; }

    public int BoardWidth => _boardWidth;

    public int BoardHeight => _boardHeight;
    
    public int BoardSpacing => DotsGenerator.Instance.DotSize * 2;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        int numDots = DotsGenerator.Instance.NumDots;
        
        _dots = new Dot[_boardWidth, _boardHeight];
        _edges = new bool[numDots * numDots, numDots * numDots];
        _visited = new bool[numDots];
        _prevDots = new Stack<Dot>();

        CenterBoard();
        DotsBoardUpdater.Instance.StartPopulatingBoard();
    }

    private void CenterBoard()
    {
        int dotSize = DotsGenerator.Instance.DotSize;
        
        float centerSpacing = (float) dotSize / 2;

        float centerX = -centerSpacing * _boardWidth;
        float centerY = centerSpacing * _boardHeight;
        _rectTransform.anchoredPosition = new Vector2(centerX, centerY);
    }

    public bool IsBoardFilled()
    {
        for (int col = 0; col < _boardWidth; col++)
        {
            for (int row = 0; row < _boardHeight; row++)
            {
                if (_dots[col, row] == null)
                    return false;

                if (!_dots[col, row].IsAt(col, row))
                    return false;
            }
        }

        return true;
    }

    public int GetRowAtPosition(Vector2 position)
    {
        int numDots = DotsGenerator.Instance.NumDots;
        
        float y = position.y;
        return ((int) (BoardSpacing - y) / numDots);
    }

    public int GetColAtPosition(Vector2 position)
    {
        int numDots = DotsGenerator.Instance.NumDots;
        
        float x = position.x;
        return ((int) (x + BoardSpacing) / numDots);
    }
    
    public float GetXAt(int col)
    {
        return (BoardSpacing * col) - BoardSpacing;
    }

    public float GetYAt(int row)
    {
        return BoardSpacing - (BoardSpacing * row);
    }

    public void PlaceDotAt(int col, int row, Dot dot)
    {
        _dots[col, row] = dot;
    }

    public void SetDotVisited(Dot dot, bool isVisited)
    {
        int dotIndex = GetIndex(dot);
        _visited[dotIndex] = isVisited;
    }

    public void ShiftDotDown(int col, int currentRow, int targetRow)
    {
        _dots[col, targetRow] = _dots[col, currentRow];
        _dots[col, currentRow] = null;
    }

    public Dot GetDotAt(int col, int row)
    {
        return _dots[col, row];
    }

    public bool ContainsDotIn(int col, int row)
    {
        return _dots[col, row] != null;
    }
    
    public bool IsAllDotsOnRowAtTarget(int row)
    {
        for (int col = 0; col < _boardWidth; col++)
        {
            if (_dots[col, row].Row != row)
                return false;
        }

        return true;
    }

    public bool IsDotVisited(Dot dot)
    {
        int dotIndex = GetIndex(dot);
        return _visited[dotIndex];
    }
    
    public void UnvisitAllDots()
    {
        int numDots = DotsGenerator.Instance.NumDots;
        for (int i = 0; i < numDots; i++)
        {
            _visited[i] = false;
        }
    }
    
    public void AddEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = true;
        _edges[dstIndex, srcIndex] = true;

        _prevDots.Push(src);

        if (!IsSquareFormed)
        {
            if (CountEdgesAt(dst) > 1)
            {
                _formedSquareDot = dst;
                IsSquareFormed = true;

                List<Dot> potentialDotsRemoved = GetDotsToRemove(dst);
                DotsBoardUpdater.Instance.StartHighlightingDots(potentialDotsRemoved);
            }
        }
    }

    public void RemoveEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = false;
        _edges[dstIndex, srcIndex] = false;

        if (IsSquareFormed)
        {
            if (_formedSquareDot == src)
            {
                if (CountEdgesAt(src) <= 1)
                    IsSquareFormed = false;
            }
            else if (_formedSquareDot == dst)
            {
                List<Dot> potentialDotsRemoved = GetDotsToRemove(dst);
                DotsBoardUpdater.Instance.StartHighlightingDots(potentialDotsRemoved);
            }
        }

        _prevDots.Pop();
    }
    
    public void ClearEdges()
    {
        _prevDots.Clear();
        _formedSquareDot = null;
        IsSquareFormed = false;

        int numDots = DotsGenerator.Instance.NumDots;
        
        for (int i = 0; i < numDots * numDots; i++)
        {
            for (int j = 0; j < numDots * numDots; j++)
                _edges[i, j] = false;
        }
    }

    public bool ContainsEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        return _edges[srcIndex, dstIndex] && _edges[dstIndex, srcIndex];
    }

    public bool IsDotPreviousSource(Dot dot)
    {
        return dot == _prevDots.Peek();
    }

    public bool IsPossibleDotConnections()
    {
        Queue<Dot> q = new Queue<Dot>();
        q.Enqueue(_dots[0, 0]);

        while (q.Count > 0)
        {
            Dot cur = q.Dequeue();
            int curIndex = GetIndex(cur);
            _visited[curIndex] = true;

            List<Dot> sameDotsAroundCur = GetSameColoredDotsAround(cur);
            if (sameDotsAroundCur.Count > 0)
            {
                UnvisitAllDots();
                return true;
            }

            List<Dot> allDotsAroundCur = GetDotsAround(cur);
            foreach (Dot dot in allDotsAroundCur)
            {
                int dotIndex = GetIndex(dot);
                if (_visited[dotIndex])
                    continue;

                q.Enqueue(_dots[dot.Col, dot.Row]);
            }
        }
        
        UnvisitAllDots();
        return false;
    }

    public List<Dot> GetSameColoredDotsAround(Dot dot)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color != dot.Color));

        return dots;
    }
    
    public List<Dot> GetSameColoredDotsAround(Dot dot, Color color)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color != color));

        return dots;
    }
    
    public List<Dot> GetDifferentColoredDotsAround(Dot dot)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color == dot.Color));

        return dots;
    }

    public List<Dot> GetDotsToRemove(Dot src)
    {
        List<Dot> line = GetDotsOnLineFrom(src);
        if (!IsSquareFormed)
            return line;

        List<Dot> dotsWithSrcColor = GetDotsWithColor(src.Color);
        List<Dot> dotsInSquare = GetDotsInSquare(line);

        List<Dot> dotsToRemove = dotsWithSrcColor.Union<Dot>(dotsInSquare).Distinct().ToList();
        return dotsToRemove;
    }

    private List<Dot> GetDotsWithColor(Color color)
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

    private List<Dot> GetDotsOnLineFrom(Dot src)
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

        if (dots.Count <= 1)
            dots.Clear();

        UnvisitAllDots();

        return dots;
    }

    private List<Dot> GetDotsInSquare(List<Dot> square)
    {
        List<Dot> dotsInSquare = new List<Dot>();
        List<Dot> cornersOnSquare = GetCornerDots(square);

        int minRow = Int32.MaxValue;
        int minCol = Int32.MaxValue;
        int maxRow = Int32.MinValue;
        int maxCol = Int32.MinValue;

        foreach (Dot corner in cornersOnSquare)
        {
            minRow = Mathf.Min(minRow, corner.Row);
            maxRow = Mathf.Max(maxRow, corner.Row);
            minCol = Mathf.Min(minCol, corner.Col);
            maxCol = Mathf.Max(maxCol, corner.Col);
        }

        for (int col = 0; col < _boardWidth; col++)
        {
            for (int row = 0; row < _boardHeight; row++)
            {
                if (row <= minRow || row >= maxRow || col <= minCol || col >= maxCol)
                    continue;

                Dot dot = _dots[col, row];
                dotsInSquare.Add(dot);
            }
        }

        return dotsInSquare;
    }

    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }
    
    private int CountEdgesAt(Dot src)
    {
        int numEdges = 0;

        List<Dot> dotsAroundSrc = GetDotsAround(src);
        foreach (Dot dst in dotsAroundSrc)
        {
            if (ContainsEdge(src, dst))
                numEdges++;
        }

        return numEdges;
    }
    
    private bool IsDotCorner(Dot src)
    {
        if (CountEdgesAt(src) < 2)
            return false;

        int numDots = DotsGenerator.Instance.NumDots;

        int srcIndex = GetIndex(src);
        int leftSrcIndex = srcIndex - 1;
        int rightSrcIndex = srcIndex + 1;
        int upSrcIndex = srcIndex - _boardWidth;
        int downSrcIndex = srcIndex + _boardWidth;

        bool isTopLeftCorner = false;
        bool isTopRightCorner = false;
        bool isBotLeftCorner = false;
        bool isBotRightCorner = false;
        
        bool isLeftSrcIndexInBounds = leftSrcIndex >= 0;
        bool isRightSrcIndexInBounds = rightSrcIndex < numDots * numDots;
        bool isUpSrcIndexInBounds = upSrcIndex >= 0;
        bool isDownSrcIndexInBounds = downSrcIndex < numDots * numDots;

        if (isDownSrcIndexInBounds)
        {
            if (isLeftSrcIndexInBounds)
                isTopRightCorner = _edges[srcIndex, downSrcIndex] && _edges[srcIndex, leftSrcIndex];
            
            if (isRightSrcIndexInBounds)
                isTopLeftCorner = _edges[srcIndex, downSrcIndex] && _edges[srcIndex, rightSrcIndex];
        }

        if (isUpSrcIndexInBounds)
        {
            if (isLeftSrcIndexInBounds)
                isBotRightCorner = _edges[srcIndex, upSrcIndex] && _edges[srcIndex, leftSrcIndex];
            
            if (isRightSrcIndexInBounds)
                isBotLeftCorner = _edges[srcIndex, upSrcIndex] && _edges[srcIndex, rightSrcIndex];
        }

        return isTopLeftCorner || isTopRightCorner || isBotLeftCorner || isBotRightCorner;
    }
    
    private List<Dot> GetDotsAround(Dot dot)
    {
        List<Dot> neighbors = new List<Dot>();
        
        if (dot.Row - 1 >= 0)
        {
            Dot upDot = _dots[dot.Col, dot.Row - 1];

            if (upDot != null)
                neighbors.Add(upDot);
        }
        
        if (dot.Row + 1 < _boardHeight)
        {
            Dot downDot = _dots[dot.Col, dot.Row + 1];

            if (downDot != null)
                neighbors.Add(downDot);
        }

        if (dot.Col - 1 >= 0)
        {
            Dot leftDot = _dots[dot.Col - 1, dot.Row];

            if (leftDot != null)
                neighbors.Add(leftDot);
        }
        
        if (dot.Col + 1 < _boardWidth)
        {
            Dot rightDot = _dots[dot.Col + 1, dot.Row];

            if (rightDot != null)
                neighbors.Add(rightDot);
        }

        return neighbors;
    }
    
    private List<Dot> GetCornerDots(List<Dot> square)
    {
        List<Dot> cornerDots = new List<Dot>();

        foreach (Dot dot in square)
        {
            if (IsDotCorner(dot))
                cornerDots.Add(dot);
        }

        return cornerDots;
    }
}
