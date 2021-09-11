using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Rendering.HybridV2;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

[DefaultExecutionOrder(0)]
public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;
    [SerializeField] private Color[] _dotColors;

    private RectTransform _rectTransform;
    
    private ObjectPooler _dotsPooler;
    private Dot [,] _dots;
    private Stack<Dot> _prevDots;
    private bool[,] _edges;
    private bool[] _visited;

    private Dot _formedSquareDot;

    private Dictionary<Color, int> _dotColorsSpawnedCounts;
    private int _totalDotsCountSpawned;

    private int _dotSize;

    private int _numDots => _dotsPooler.ObjectPoolSize;

    private int _dotSpacing => _dotSize * 2;

    public bool IsSquareFormed { get; private set; }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        _dotsPooler = GetComponent<ObjectPooler>();
        _dotsPooler.ObjectPoolSize = _boardWidth * _boardHeight;
        _dotsPooler.GenerateObjects();

        GameObject dotObject = _dotsPooler.ObjectPrefab;
        Dot dot = dotObject.GetComponent<Dot>();
        _dotSize = (int) dot.Size;
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
        _dotColorsSpawnedCounts = new Dictionary<Color, int>();
        _totalDotsCountSpawned = 0;

        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                GenerateDot(i, j, GenerateRandomColor);
            }
        }
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
        
        if (!IsSquareFormed)
        {
            if (CountEdgesAt(dst) > 1)
            {
                _formedSquareDot = dst;
                IsSquareFormed = true;
                
                HighlightDots(GetDotsToRemove(dst));
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
                HighlightDots(GetDotsToRemove(dst));
            }
        }
        
        _prevDots.Pop();
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

    public void RemoveDot(Dot src)
    {
        src.Shrink(() =>
        {
            int row = src.Row;
            int col = src.Col;
            
            _dotsPooler.ReturnPooledObject(src.gameObject);
            _dots[col, row] = null;

            DropDotsDown(col);
        });
    }
    
    public void RemoveDots(Dot src)
    {
        List<Dot> dotsToRemove = GetDotsToRemove(src);
        if (dotsToRemove.Count <= 0)
            return;
        
        ClearEdges();
        
        foreach (Dot dot in dotsToRemove)
        {
            dot.StopAllCoroutines();
            dot.Reset();
        }
        
        IEnumerator shrinkDots = ShrinkDots(dotsToRemove, DropAllDotsDown);
        StartCoroutine(shrinkDots);
    }

    public List<Dot> GetSameColoredDotsAround(Dot dot)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color != dot.Color));

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

    private IEnumerator ShrinkDots(List<Dot> dots, Action<HashSet<int>> onShrinkCompleted)
    {
        int removedDotsCount = 0;
        HashSet<int> cols = new HashSet<int>();
        
        foreach (Dot dot in dots)
        {
            dot.Shrink(() =>
            {
                int row = dot.Row;
                int col = dot.Col;
            
                _dotsPooler.ReturnPooledObject(dot.gameObject);
                _dots[col, row] = null;
            
                cols.Add(col);

                removedDotsCount++;
            });
        }

        yield return new WaitUntil(() => removedDotsCount == dots.Count);
        
        onShrinkCompleted?.Invoke(cols);
    }

    private void DropAllDotsDown(HashSet<int> cols)
    {
        foreach (int col in cols)
            DropDotsDown(col);
    }

    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }

    private Dot GetDotAtIndex(int index)
    {
        int col = index / _boardWidth;
        int row = index % _boardWidth;

        return _dots[col, row];
    }

    private float GetXAt(int col)
    {
        return (_dotSpacing * col) - _dotSpacing;
    }

    private float GetYAt(int row)
    {
        return _dotSpacing - (_dotSpacing * row);
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

    private Color GenerateRandomColor()
    {
        Color color = _dotColors[Random.Range(0, _dotColors.Length)];
        return color;
    }

    private Color GenerateFirstColor()
    {
        return _dotColors[0];
    }
    
    private void GenerateDot(int col, int row, Func<Color> randomColorGenerator)
    {
        if (row < 0 || row >= _boardHeight || col < 0 || col >= _boardWidth)
            return;

        GameObject dotObject = _dotsPooler.GetPooledObject();
        if (dotObject == null)
            return;
        
        _dots[col, row] = dotObject.GetComponent<Dot>();
        _dots[col, row].Position = new Vector2(GetXAt(col), GetYAt(row));
        _dots[col, row].Color = randomColorGenerator.Invoke();
        _dots[col, row].gameObject.SetActive(true);

        Color dotColor = _dots[col, row].Color;

        if (_dotColorsSpawnedCounts.ContainsKey(dotColor))
            _dotColorsSpawnedCounts[dotColor]++;
        else
            _dotColorsSpawnedCounts[dotColor] = 0;
        
        _totalDotsCountSpawned++;
    }
    
    private void HighlightDots(List<Dot> dots)
    {
        foreach (Dot dot in dots)
        {
            dot.Highlight();
        }
    }
    
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
            GenerateDot(col, currentRow, GenerateRandomColorBasedOffDotSpawnCounts);
            currentRow++;
        }
    }

    private Color GenerateRandomColorBasedOffDotSpawnCounts()
    {
        Color color = GenerateRandomColor();
        
        foreach (Color dotColor in _dotColors)
        {
            int numDotsWithColorSpawned = _dotColorsSpawnedCounts[dotColor];
            float percentageDotsWithColorSpawned = (float)numDotsWithColorSpawned / _totalDotsCountSpawned;

            if (Random.Range(0.0f, 1.0f) < 1.0f - percentageDotsWithColorSpawned)
                color = dotColor;
        }


        return color;
    }
    
    private bool IsDotCorner(Dot src)
    {
        if (CountEdgesAt(src) < 2)
            return false;

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
        bool isRightSrcIndexInBounds = rightSrcIndex < _numDots * _numDots;
        bool isUpSrcIndexInBounds = upSrcIndex >= 0;
        bool isDownSrcIndexInBounds = downSrcIndex < _numDots * _numDots;

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
    
    private void ClearEdges()
    {
        _prevDots.Clear();
        _formedSquareDot = null;
        IsSquareFormed = false;
        
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
