using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(0)]
public class DotsBoard : Singleton<DotsBoard>
{
    [SerializeField] private int _boardWidth = 6;
    [SerializeField] private int _boardHeight = 6;
    
    private ObjectPooler _dotsPooler;
    private Dot [,] _board;

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
        _board = new Dot[_boardWidth, _boardHeight];

        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                _board[i, j] = _dotsPooler.GetPooledObject().GetComponent<Dot>();
                _board[i, j].name = $"Dot {j}, {i}";
                _board[i, j].Row = j;
                _board[i, j].Col = i;
                _board[i, j].gameObject.SetActive(true);
            }
        }
    }

    public List<Dot> GetNeighbors(Dot dot)
    {
        List<Dot> neighbors = new List<Dot>();

        if (dot.Row - 1 >= 0)
        {
            Dot upDot = _board[dot.Col, dot.Row - 1];
            neighbors.Add(upDot);
        }

        if (dot.Row + 1 < _boardHeight)
        {
            Dot downDot = _board[dot.Col, dot.Row + 1];
            neighbors.Add(downDot);
        }

        if (dot.Col - 1 >= 0)
        {
            Dot leftDot = _board[dot.Col - 1, dot.Row];
            neighbors.Add(leftDot);
        }

        if (dot.Col + 1 < _boardWidth)
        {
            Dot rightDot = _board[dot.Col + 1, dot.Row];
            neighbors.Add(rightDot);
        }

        return neighbors;
    }

    public void ResetPreviousDots()
    {
        for (int i = 0; i < _boardWidth; i++)
        {
            for (int j = 0; j < _boardHeight; j++)
            {
                _board[i, j].PreviousDot = null;
            }
        }
    }
}
