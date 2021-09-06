using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Rendering.HybridV2;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;
    [SerializeField] private Color[] _dotColors;
    [SerializeField] private int _seed;
    
    private ObjectPooler _dotsPooler;
    private Dot [,] _dots;
    private Stack<Dot> _prevDots;
    private Stack<Dot> _formedSquareDots;
    private bool[,] _edges;
    private bool[] _visited;

    private int NumDots => _dotsPooler.ObjectPoolSize;

    public bool IsSquareFormed() => _formedSquareDots.Count > 0;

    private void Awake()
    {
        _dotsPooler = GetComponent<ObjectPooler>();
        _dotsPooler.ObjectPoolSize = _boardWidth * _boardHeight;
        _dotsPooler.GenerateObjects();
    }

    private void Start()
    {
        PopulateDots();
    }

    private void PopulateDots()
    {
        _dots = new Dot[_boardWidth, _boardHeight];
        _edges = new bool[NumDots * NumDots, NumDots * NumDots];
        _visited = new bool[NumDots];
        _prevDots = new Stack<Dot>();
        _formedSquareDots = new Stack<Dot>();
        
        #if UNITY_EDITOR
            Random.InitState(_seed);
        #endif

        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                _dots[i, j] = _dotsPooler.GetPooledObject().GetComponent<Dot>();
                _dots[i, j].name = $"Dot {j}, {i}";
                _dots[i, j].Row = j;
                _dots[i, j].Col = i;
                _dots[i, j].Color = GenerateRandomColor();
                _dots[i, j].gameObject.SetActive(true);
            }
        }
    }

    public void ResetBoard()
    {
        ClearEdges();
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
            
            if (upDot.Color == dot.Color)
                neighbors.Add(upDot);
        }
        
        if (dot.Row + 1 < _boardHeight)
        {
            Dot downDot = _dots[dot.Col, dot.Row + 1];
            
            if (downDot.Color == dot.Color)
                neighbors.Add(downDot);
        }

        if (dot.Col - 1 >= 0)
        {
            Dot leftDot = _dots[dot.Col - 1, dot.Row];
            
            if (leftDot.Color == dot.Color)
                neighbors.Add(leftDot);
        }
        
        if (dot.Col + 1 < _boardWidth)
        {
            Dot rightDot = _dots[dot.Col + 1, dot.Row];
            
            if (rightDot.Color == dot.Color)
                neighbors.Add(rightDot);
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

    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }

    private Color GenerateRandomColor()
    {
        Color color = _dotColors[Random.Range(0, _dotColors.Length)];
        return color;
    }
    
    private void ClearEdges()
    {
        _prevDots.Clear();
        _formedSquareDots.Clear();
        for (int i = 0; i < NumDots * NumDots; i++)
        {
            for (int j = 0; j < NumDots * NumDots; j++)
                _edges[i, j] = false;
        }
    }

    private void UnvisitAllDots()
    {
        for (int i = 0; i < NumDots; i++)
        {
            _visited[i] = false;
        }
    }
}
