using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class DotsBoard : Singleton<DotsBoard>
{
    /// <summary>
    /// The number of dots on the board horizontally
    /// </summary>
    [SerializeField] private int _boardWidth = 6;
    
    /// <summary>
    /// The number of dots on the board vertically
    /// </summary>
    [SerializeField] private int _boardHeight = 6;

    /// <summary>
    /// The matrix of dots in the board
    /// </summary>
    private Dot[,] _dots;
    
    /// <summary>
    /// The stack of the previous dots that were connected
    /// </summary>
    private Stack<Dot> _prevDots;
    
    /// <summary>
    /// Stores the connections between every two dots
    /// </summary>
    private bool[,] _edges;
    
    /// <summary>
    /// Stores the dots that have been marked as visited
    /// </summary>
    private bool[] _visited;

    /// <summary>
    /// Stores the dot that first formed a square
    /// </summary>
    private Dot _formedSquareDot;
    
    /// <summary>
    /// Stores how far the dots are spaced out by multiplying the dot size by 2
    /// </summary>
    public int BoardSpacing => DotsGenerator.Instance.DotSize * 2;

    public bool IsSquareFormed { get; private set; }

    public int BoardWidth => _boardWidth;

    public int BoardHeight => _boardHeight;
    
    private RectTransform _rectTransform;

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

    /// <summary>
    /// Centers the dots in the board
    /// </summary>
    private void CenterBoard()
    {
        // Calculate the center spacing by dividing the dot size in half
        int dotSize = DotsGenerator.Instance.DotSize;
        float centerSpacing = (float) dotSize / 2;

        // Create a Vector2 that aligns the board to the middle of the screen then set the anchored position to it
        float centerX = -centerSpacing * _boardWidth;
        float centerY = centerSpacing * _boardHeight;
        _rectTransform.anchoredPosition = new Vector2(centerX, centerY);
    }

    /// <summary>
    /// Checks if the board has no null variables and the dots are their target rows and columns
    /// </summary>
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
    
    /// <returns>Calculates the row using the y position</returns>
    public int GetRowAtPosition(Vector2 position)
    {
        int numDots = DotsGenerator.Instance.NumDots;
        
        float y = position.y;
        return ((int) (BoardSpacing - y) / numDots);
    }

    /// <returns>Calculates the column using the x position</returns>
    public int GetColAtPosition(Vector2 position)
    {
        int numDots = DotsGenerator.Instance.NumDots;
        
        float x = position.x;
        return ((int) (x + BoardSpacing) / numDots);
    }
    
    /// <returns>Calculates the x position at the column</returns>
    public float GetXAt(int col)
    {
        return (BoardSpacing * col) - BoardSpacing;
    }

    /// <returns>Calculates the y position at the row</returns>
    public float GetYAt(int row)
    {
        return BoardSpacing - (BoardSpacing * row);
    }

    /// <summary>
    /// Public variable that assigns a dot in the dots board matrix at indices col & row
    /// </summary>
    /// <param name="col">The column the dot will be placed in the board</param>
    /// <param name="row">The row the dot will be placed in the board</param>
    /// <param name="dot">The dot that is being placed</param>
    public void PlaceDotAt(int col, int row, Dot dot)
    {
        _dots[col, row] = dot;
    }

    /// <summary>
    /// Marks the dot as visited or not visited
    /// </summary>
    public void SetDotVisited(Dot dot, bool isVisited)
    {
        int dotIndex = GetIndex(dot);
        _visited[dotIndex] = isVisited;
    }

    /// <summary>
    /// Moves a dot down in column from current row to target row
    /// </summary>
    /// <param name="col">The column the dot is in the board</param>
    /// <param name="currentRow">The row the dot is currently in the board</param>
    /// <param name="targetRow">The row the dot is going to in the board</param>
    public void ShiftDotDown(int col, int currentRow, int targetRow)
    {
        _dots[col, targetRow] = _dots[col, currentRow];
        _dots[col, currentRow] = null;
    }

    
    /// <param name="col">The column the dot is in</param>
    /// <param name="row">The row the dot is in</param>
    /// <returns>The dot at indices col & row</returns>
    public Dot GetDotAt(int col, int row)
    {
        return _dots[col, row];
    }

    /// <summary>
    /// Checks if it is a dot is in the dots board matrix at indices col & row
    /// </summary>
    /// <param name="col">The column the dot is in</param>
    /// <param name="row">The row the dot is in</param>
    public bool ContainsDotIn(int col, int row)
    {
        return _dots[col, row] != null;
    }
    
    /// <summary>
    /// Checks if the dots targeted on the row is at the row
    /// </summary>
    /// <param name="row">The row being checked</param>
    /// <returns></returns>
    public bool IsAllDotsOnRowAtTarget(int row)
    {
        for (int col = 0; col < _boardWidth; col++)
        {
            if (_dots[col, row].Row != row)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a dot has been mark visited
    /// </summary>
    /// <param name="dot">The dot being checked</param>
    public bool IsDotVisited(Dot dot)
    {
        int dotIndex = GetIndex(dot);
        return _visited[dotIndex];
    }
    
    /// <summary>
    /// Marks all dots in the board as unvisited
    /// </summary>
    public void UnvisitAllDots()
    {
        int numDots = DotsGenerator.Instance.NumDots;
        for (int i = 0; i < numDots; i++)
        {
            _visited[i] = false;
        }
    }
    
    /// <summary>
    /// Marks connection between the two dots, src & st
    /// </summary>
    /// <param name="src">The source dot</param>
    /// <param name="dst">The destination dot</param>
    public void AddEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = true;
        _edges[dstIndex, srcIndex] = true;

        // Put the source dot in the previous dots stack
        _prevDots.Push(src);

        // Mark the dot if it has formed a square
        if (!IsSquareFormed)
        {
            if (CountEdgesAt(dst) > 1)
            {
                _formedSquareDot = dst;
                IsSquareFormed = true;

                // Highlight the dots that are going to be removed
                List<Dot> potentialDotsRemoved = GetDotsToRemove(dst);
                DotsBoardUpdater.Instance.StartHighlightingDots(potentialDotsRemoved);
            }
        }
    }

    /// <summary>
    /// Unmarks connection between the two dots, src & st
    /// </summary>
    /// <param name="src">The source dot</param>
    /// <param name="dst">The destination dot</param>
    public void RemoveEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        _edges[srcIndex, dstIndex] = false;
        _edges[dstIndex, srcIndex] = false;
        
        if (IsSquareFormed)
        {
            // Removing all connections in the formed square dot will turn the square into a line
            if (_formedSquareDot == src)
            {
                if (CountEdgesAt(src) <= 1)
                    IsSquareFormed = false;
            }
            
            // Removing connection from the formed square dot will re-highlight the dots that are going to be removed
            else if (_formedSquareDot == dst)
            {
                List<Dot> potentialDotsRemoved = GetDotsToRemove(dst);
                DotsBoardUpdater.Instance.StartHighlightingDots(potentialDotsRemoved);
            }
        }

        // Take the most recent previous dot out of the stack
        _prevDots.Pop();
    }
    
    /// <summary>
    /// Clears all of the marked connections between all of the dots
    /// </summary>
    public void ClearEdges()
    {
        // No edges means no previous dots, so clear it
        _prevDots.Clear();
        
        // No edges means no square has formed yet, so set formedSquareDot to null & IsSquareFormed to false
        _formedSquareDot = null;
        IsSquareFormed = false;

        int numDots = DotsGenerator.Instance.NumDots;
        for (int i = 0; i < numDots * numDots; i++)
        {
            for (int j = 0; j < numDots * numDots; j++)
                _edges[i, j] = false;
        }
    }

    /// <summary>
    /// Checks if there is a connection between dots src & dst
    /// </summary>
    /// <param name="src">The source dot</param>
    /// <param name="dst">The destination dot</param>
    /// <returns></returns>
    public bool ContainsEdge(Dot src, Dot dst)
    {
        int srcIndex = GetIndex(src);
        int dstIndex = GetIndex(dst);

        return _edges[srcIndex, dstIndex] && _edges[dstIndex, srcIndex];
    }

    /// <summary>
    /// Checks if dot is the most recent previous dot found in the stack
    /// </summary>
    /// <param name="dot">The dot that we are checking</param>
    public bool IsDotPreviousSource(Dot dot)
    {
        return dot == _prevDots.Peek();
    }

    /// <summary>
    /// Check for any connections that the player could find by
    /// breadth first searching through all of the dots until there is a dot that has same colored dots around it
    /// </summary>
    public bool IsPossibleDotConnections()
    {
        // Initialize queue and put the first dot in
        Queue<Dot> q = new Queue<Dot>();
        q.Enqueue(_dots[0, 0]);

        while (q.Count > 0)
        {
            // Mark dequeued dot as the current dot & visited
            Dot cur = q.Dequeue();
            int curIndex = GetIndex(cur);
            _visited[curIndex] = true;

            // Check if the current dot has any same colored dots around it
            List<Dot> sameDotsAroundCur = GetSameColoredDotsAround(cur);
            if (sameDotsAroundCur.Count > 0)
            {
                // There is a connection that the player can make, so end the breadth first search
                UnvisitAllDots();
                return true;
            }
            
            // Otherwise, enqueue any unvisited surrounding dots
            List<Dot> allDotsAroundCur = GetDotsAround(cur);
            foreach (Dot dot in allDotsAroundCur)
            {
                int dotIndex = GetIndex(dot);
                if (_visited[dotIndex])
                    continue;

                q.Enqueue(_dots[dot.Col, dot.Row]);
            }
        }
        
        // There was possible connections that the player could make, so the breadth first search has ended
        UnvisitAllDots();
        return false;
    }

    /// <summary>
    /// Gets the list of the dots around a source dot with the source dot's color
    /// </summary>
    /// <param name="dot"></param>
    public List<Dot> GetSameColoredDotsAround(Dot dot)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color != dot.Color));

        return dots;
    }
    
    /// <summary>
    /// Gets the list of the dots around a source dot with the same color
    /// </summary>
    /// <param name="dot">The dot that we are searching through for neighbors from</param>
    /// <param name="color">The color that we are searching for</param>
    public List<Dot> GetSameColoredDotsAround(Dot dot, Color color)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color != color));

        return dots;
    }
    
    /// <summary>
    /// Gets the list of the dots around a source dot with the different color
    /// </summary>
    /// <param name="dot">The dot that we are searching through for neighbors from</param>
    public List<Dot> GetDifferentColoredDotsAround(Dot dot)
    {
        List<Dot> dots = GetDotsAround(dot);
        dots.RemoveAll(((d) => d.Color == dot.Color));

        return dots;
    }

    /// <summary>
    /// Get the dots that are going to be removed once the player has stopped dragging
    /// </summary>
    /// <param name="src">The dot that is part of the line or square</param>
    /// <returns></returns>
    public List<Dot> GetDotsToRemove(Dot src)
    {
        // Return the line if the square hasn't been formed
        List<Dot> line = GetDotsOnLineFrom(src);
        if (!IsSquareFormed)
            return line;

        //Make a list of dots that have both dots with the same color & any other dots that are in the square
        List<Dot> dotsWithSrcColor = GetDotsWithColor(src.Color);
        List<Dot> dotsInSquare = GetDotsInSquare(line);
        List<Dot> dotsToRemove = dotsWithSrcColor.Union<Dot>(dotsInSquare).Distinct().ToList();
        
        // Return this unionized list
        return dotsToRemove;
    }

    /// <summary>
    /// Retrieves all of the dots with the same color as color
    /// </summary>
    /// <param name="color">The color being serached for</param>
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

    /// <summary>
    /// Retrieves the dots that are in the line.
    /// Depth-first search through the dots by traversing through edge to edge until there isn't anymore edges.
    /// </summary>
    /// <param name="src">The starting dot in the line</param>
    /// <returns></returns>
    private List<Dot> GetDotsOnLineFrom(Dot src)
    {
        List<Dot> dots = new List<Dot>();
        
        // Create a stack and push the src dot into it
        Stack<Dot> stack = new Stack<Dot>();
        stack.Push(src);

        while (stack.Count > 0)
        {
            // Pop the dot out of the stack, mark it as the current dot, and add it to the dots list
            Dot cur = stack.Pop();
            dots.Add(cur);
            
            // Mark the current dot as visited
            int curIndex = GetIndex(cur);
            _visited[curIndex] = true;

            // Get the same colored dots around the cur then loop through it
            List<Dot> dsts = GetSameColoredDotsAround(cur);
            foreach (Dot dst in dsts)
            {
                // Move on to the next dot if there is connection between the current dot and the dot being looked at
                if (!ContainsEdge(cur, dst))
                    continue;
                
                // Move on if the dot being looked at has already been visited
                int dstIndex = GetIndex(dst);
                if (_visited[dstIndex])
                    continue;
            
                // Push the dot being looked at into the stack
                stack.Push(dst);
            }
        }

        // A line that doesn't have only 0 or 1 dots in it, so clear it
        if (dots.Count <= 1)
            dots.Clear();

        // Return the dots list
        UnvisitAllDots();
        return dots;
    }

    /// <summary>
    /// Retrieves the list of dots that are inside of the square
    /// </summary>
    /// <param name="square"></param>
    private List<Dot> GetDotsInSquare(List<Dot> square)
    {
        List<Dot> dotsInSquare = new List<Dot>();
        
        // Get all of the corners of the square
        List<Dot> cornersOnSquare = GetCornerDots(square);

        // Find the minimum rows and columns out of each corner
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

        // Add the dots in the board matrix that between the minimum & maximum rows & columns
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

    /// <summary>
    /// Calculate the index of a dot in the board matrix
    /// </summary>
    /// <param name="dot">The dot being looked at</param>
    private int GetIndex(Dot dot)
    {
        return dot.Col * _boardWidth + dot.Row;
    }
    
    /// <summary>
    /// Count the number of connections a dot has
    /// </summary>
    /// <param name="src">The dot being looked at</param>
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
    
    /// <summary>
    /// Checks if a dot is a corner
    /// </summary>
    /// <param name="src">The dot being looked at</param>
    /// <returns></returns>
    private bool IsDotCorner(Dot src)
    {
        // Corners don't have less than 2 connections!
        if (CountEdgesAt(src) < 2)
            return false;

        int numDots = DotsGenerator.Instance.NumDots;

        // Get the indices of the dots around the src
        int srcIndex = GetIndex(src);
        int leftSrcIndex = srcIndex - 1;
        int rightSrcIndex = srcIndex + 1;
        int upSrcIndex = srcIndex - _boardWidth;
        int downSrcIndex = srcIndex + _boardWidth;
        
        // Set boolean variables that check if the indices are in the bounds of the edges matrix
        bool isLeftSrcIndexInBounds = leftSrcIndex >= 0;
        bool isRightSrcIndexInBounds = rightSrcIndex < numDots * numDots;
        bool isUpSrcIndexInBounds = upSrcIndex >= 0;
        bool isDownSrcIndexInBounds = downSrcIndex < numDots * numDots;

        // Set boolean variables that check if each respective two edges is a corner
        bool isTopLeftCorner = false;
        bool isTopRightCorner = false;
        bool isBotLeftCorner = false;
        bool isBotRightCorner = false;

        // The dot down of the source dot can make a corner with the left or right dot
        if (isDownSrcIndexInBounds)
        {
            if (isLeftSrcIndexInBounds)
                isTopRightCorner = _edges[srcIndex, downSrcIndex] && _edges[srcIndex, leftSrcIndex];
            
            if (isRightSrcIndexInBounds)
                isTopLeftCorner = _edges[srcIndex, downSrcIndex] && _edges[srcIndex, rightSrcIndex];
        }

        // The dot above the source dot can make a corner with the left or right dot
        if (isUpSrcIndexInBounds)
        {
            if (isLeftSrcIndexInBounds)
                isBotRightCorner = _edges[srcIndex, upSrcIndex] && _edges[srcIndex, leftSrcIndex];
            
            if (isRightSrcIndexInBounds)
                isBotLeftCorner = _edges[srcIndex, upSrcIndex] && _edges[srcIndex, rightSrcIndex];
        }
        
        return isTopLeftCorner || isTopRightCorner || isBotLeftCorner || isBotRightCorner;
    }
    
    /// <summary>
    /// Get all of the dots around a source dot.
    /// Check if the source dot's row or column is in bounds.
    /// If so, then add it to the neighbors list.
    /// </summary>
    /// <param name="src">The dot being searched for neighbors from</param>
    private List<Dot> GetDotsAround(Dot src)
    {
        List<Dot> neighbors = new List<Dot>();
        
       
        
        if (src.Row - 1 >= 0)
        {
            Dot upDot = _dots[src.Col, src.Row - 1];

            if (upDot != null)
                neighbors.Add(upDot);
        }
        
        if (src.Row + 1 < _boardHeight)
        {
            Dot downDot = _dots[src.Col, src.Row + 1];

            if (downDot != null)
                neighbors.Add(downDot);
        }

        if (src.Col - 1 >= 0)
        {
            Dot leftDot = _dots[src.Col - 1, src.Row];

            if (leftDot != null)
                neighbors.Add(leftDot);
        }
        
        if (src.Col + 1 < _boardWidth)
        {
            Dot rightDot = _dots[src.Col + 1, src.Row];

            if (rightDot != null)
                neighbors.Add(rightDot);
        }

        return neighbors;
    }
    
    /// <summary>
    /// Retrieves all of the dots in a square that are corners
    /// </summary>
    /// <param name="square">The square that we are checking for corners from</param>
    /// <returns></returns>
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
