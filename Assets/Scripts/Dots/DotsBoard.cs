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
    
    public bool IsSquareFormed { get; set; }
    
    private ObjectPooler _dotsPooler;
    private Dot [,] _dots;
    private Dictionary<int, List<int>> _edges;
    private bool[] _visited;

    private int NumDots => _dotsPooler.ObjectPoolSize;

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
        _edges = new Dictionary<int, List<int>>();
        _visited = new bool[NumDots];
        
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

    public void ClearEdges()
    {
        _edges.Clear();
    }

    public void UnvisitAllDots()
    {
        for (int i = 0; i < NumDots; i++)
        {
            _visited[i] = false;
        }
    }
    
    public void AddEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        List<int> dstsFromSrc;
        if (_edges.TryGetValue(srcIndex, out List<int> dsts))
            dstsFromSrc = dsts;
        else
            dstsFromSrc = new List<int>();

        dstsFromSrc.Add(dstIndex);
        _edges[srcIndex] = dstsFromSrc;
    }

    public void RemoveEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);
        
        if (_edges.TryGetValue(srcIndex, out List<int> dsts))
        {
            dsts.Remove(dstIndex);
            _edges[srcIndex] = dsts;
        }
    }

    public bool ContainsEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        if (_edges.TryGetValue(srcIndex, out List<int> dsts))
            return dsts.Contains(dstIndex);

        return false;
    }

    public int CountEdgesAt(Dot src)
    {
        int srcIndex = GetIndex(src);

        if (_edges.TryGetValue(srcIndex, out List<int> dsts))
            return dsts.Count;

        return 0;
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

    public List<Dot> GetDotsOnLine(Dot src)
    {
        List<Dot> dots = new List<Dot>();
        
        Stack<Dot> s = new Stack<Dot>();
        s.Push(src);

        string message = string.Empty;

        while (s.Count > 0)
        {
            Dot cur = s.Pop();
            dots.Add(cur);
            
            int curIndex = GetIndex(cur);
            _visited[curIndex] = true;

            foreach (Dot dst in cur.PreviousDots)
            {
                int dstIndex = GetIndex(dst);
                if (_visited[dstIndex])
                    continue;

                s.Push(dst);
            }
        }

        return dots;
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
    
    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }

    private Color GenerateRandomColor()
    {
        Color color = _dotColors[Random.Range(0, _dotColors.Length)];
        return color;
    }
}
