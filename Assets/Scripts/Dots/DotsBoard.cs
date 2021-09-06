using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;
    
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

        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                _dots[i, j] = _dotsPooler.GetPooledObject().GetComponent<Dot>();
                _dots[i, j].name = $"Dot {j}, {i}";
                _dots[i, j].Row = j;
                _dots[i, j].Col = i;
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

    public List<Dot> GetDotsAround(Dot dot)
    {
        List<Dot> neighbors = new List<Dot>();
        
        if (dot.Row - 1 >= 0)
        {
            Dot upDot = _dots[dot.Col, dot.Row - 1];
            neighbors.Add(upDot);
        }
        
        if (dot.Row + 1 < _boardHeight)
        {
            Dot downDot = _dots[dot.Col, dot.Row + 1];
            neighbors.Add(downDot);
        }

        if (dot.Col - 1 >= 0)
        {
            Dot leftDot = _dots[dot.Col - 1, dot.Row];
            neighbors.Add(leftDot);
        }
        
        if (dot.Col + 1 < _boardWidth)
        {
            Dot rightDot = _dots[dot.Col + 1, dot.Row];
            neighbors.Add(rightDot);
        }

        return neighbors;
    }
    

    public int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }
}
