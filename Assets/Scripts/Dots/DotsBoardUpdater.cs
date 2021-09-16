using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DotsBoardUpdater : Singleton<DotsBoardUpdater>
{
    [SerializeField] private float _dotPopulateSpeed = 6.0f;
    [SerializeField] private float _dotDropSpeed = 9.0f;
    [SerializeField] private float _dotSwapSpeed = 9.0f;

    [SerializeField] private float _dotAutoShuffleDelay = 1.0f;

    /// <summary>
    /// Store the position's y component of the where the dot will spawn at
    /// </summary>
    private float _dotStartSpawnPositionY;

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
    
    /// <summary>
    /// Starts shrinking the dot.
    /// Then, takes it out of the object pool and begins the dropping dots animation
    /// </summary>
    /// <param name="src">The dot being removed</param>
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

    /// <summary>
    /// Clear edges then  begins the dot removal animation for each one
    /// </summary>
    /// <param name="dots">The list of dots that are to be removed</param>
    public void StartRemovingDots(List<Dot> dots)
    {
        if (dots.Count <= 0)
            return;
        
        // Disable input until animation is finished
        DotsInputHandler.Instance.IsInputEnabled = false;

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

    /// <summary>
    /// Begins the dot removal animation for each dot in the list.
    /// Ends with the dot dropping animation for all of the columns where the dots were removed
    /// </summary>
    /// <param name="dots"></param>
    private void StartShrinkingDots(List<Dot> dots)
    {
        StartCoroutine(ShrinkDots(dots, StartDroppingDotsDown));
    }
    
    /// <summary>
    /// For every column in the hashset, begin the dot dropping animation
    /// </summary>
    /// <param name="cols"></param>
    private void StartDroppingDotsDown(HashSet<int> cols)
    {
        foreach (int col in cols)
            StartDroppingDotsDownInCol(col);
    }
    
    /// <summary>
    /// Initialize the dots at the spawn position then animate them dropping in row by row
    /// </summary>
    /// <returns></returns>
    private IEnumerator PopulateBoard()
    {
        // Get the width, height, and spacing of the board
        int boardWidth = DotsBoard.Instance.BoardWidth;
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardSpacing = DotsBoard.Instance.BoardSpacing;
        
        // Calculate the starting spawn position's y by multiplying the boardSpacing by 4
        _dotStartSpawnPositionY = boardSpacing * 4.0f;

        // For every row...
        int row = boardHeight - 1;
        while (row >= 0)
        {
            for (int col = 0; col < boardWidth; col++)
            {
                // Calculate the start position for each dot on it
                float x = DotsBoard.Instance.GetXAt(col);
                float y = DotsBoard.Instance.GetYAt(row);
                float topY = DotsBoard.Instance.GetYAt(boardHeight);
                
                Vector2 startPosition = new Vector2(x, (topY - y) + _dotStartSpawnPositionY * (boardHeight / 2.0f));
                Vector2 targetPosition = new Vector2(x, y);
                
                // Initialize the dot at the start position then drop it down to the target position
                Dot dot = DotsGenerator.Instance.InitializeDotAtPosition(col, row, startPosition);
                StartCoroutine(dot.MoveTo(targetPosition, _dotPopulateSpeed));
            }

            
            // Move on to the next row until all of the dots are at the target row
            yield return new WaitUntil(() => DotsBoard.Instance.IsAllDotsOnRowAtTarget(row));
            row--;
        }
        
        // Auto shuffle board if there are still no possible connections
        yield return AutoShuffleBoard(_dotAutoShuffleDelay);

        // Enable the input
        DotsInputHandler.Instance.IsInputEnabled = true;
    }
    
    /// <summary>
    /// Run the shrink animation for each dot in the list
    /// </summary>
    /// <param name="dots">The dots that are going to be shrunk</param>
    /// <param name="onShrinkCompleted">The callaback once all of the dots are shrunk</param>
    /// <returns></returns>
    private IEnumerator ShrinkDots(List<Dot> dots, Action<HashSet<int>> onShrinkCompleted = null)
    {
        // Keep track of the number of the dots removed
        int removedDotsCount = 0;
        
        // Keep track of the columns that had dots removed in it
        HashSet<int> cols = new HashSet<int>();

        // Shrink each dot in the list
        foreach (Dot dot in dots)
        {
            dot.Shrink(() =>
            {
                int row = dot.Row;
                int col = dot.Col;

                // Return the dot after being shrunk into the pool
                DotsGenerator.Instance.ReturnDotToPool(col, row);

                // Add the dot's column to the column set
                cols.Add(col);

                // Increment the removed dots count
                removedDotsCount++;
            });
        }

        // Wait until all of the dots in the list are removed
        yield return new WaitUntil(() => removedDotsCount == dots.Count);

        // Execute the completed callback
        onShrinkCompleted?.Invoke(cols);
    }
    
    /// <summary>
    /// Runs the drop animation for all of the empty spaces in a column
    /// </summary>
    /// <param name="col">The column with the empty space</param>
    /// <returns></returns>
    private IEnumerator DropDotsDown(int col)
    {
        // Start from the bottom row
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int startRow = boardHeight - 1;

        // Traverse up through the row until it hits to the top or an empty space
        int currentRow = startRow;
        while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            currentRow--;

        // Traverse through the row until it hits to the top or a dot
        int dotShiftCount = 0;
        while (currentRow >= 0 && !DotsBoard.Instance.ContainsDotIn(col, currentRow))
        {
            currentRow--;
            
            // The more spaces we traverse, the more spaces we have to shift
            dotShiftCount++;
        }
        
        int boardSpacing = DotsBoard.Instance.BoardSpacing;
        
        // Shift all of the rows down until the only space left is the top row
        do
        {
            // Traverse up through the row until it hits to the top or an empty space
            while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            {
                // Each traversal, shift the current dot down to the empty space
                Dot currentDot = DotsBoard.Instance.GetDotAt(col, currentRow);
                
                float endY = currentDot.Position.y - (boardSpacing * dotShiftCount);
                if (endY < DotsBoard.Instance.GetYAt(boardHeight - 1))
                    endY = DotsBoard.Instance.GetYAt(boardHeight - 1);
                
                Vector2 currentDotEndPosition = new Vector2(currentDot.Position.x, endY);

                yield return currentDot.MoveTo(currentDotEndPosition, _dotDropSpeed);

                DotsBoard.Instance.ShiftDotDown(col, currentRow, currentDot.Row);

                currentRow--;
            }
            
            // Traverse up through the row until it hits to the top or a dot
            while (currentRow >= 0 && !DotsBoard.Instance.ContainsDotIn(col, currentRow))
            {
                currentRow--;
                
                // The more spaces we traverse, the more spaces we have to shift in the next iteration
                dotShiftCount++;
            }
            
        } while (currentRow >= 0);

        // Start back down to the bottom row
        currentRow = startRow;
        
        // Traverse up through the row until it hits to the top or an empty space
        while (currentRow >= 0 && DotsBoard.Instance.ContainsDotIn(col, currentRow))
            currentRow--;

        // Traverse through the row until it hits to the top
        while (currentRow >= 0)
        {
            // Each traversal, generate a new dot at the start position
            float x = DotsBoard.Instance.GetXAt(col);
            Vector2 startPosition = new Vector2(x, _dotStartSpawnPositionY);
            
            yield return GenerateDotAtPosition(col, currentRow, startPosition, _dotDropSpeed);
            currentRow--;
        }

        // Wait until all of the board's spaces are filled & the dots are at their target positions
        yield return new WaitUntil(() => DotsBoard.Instance.IsBoardFilled());

        // If the board has no possible connections, auto shuffle the board
        yield return AutoShuffleBoard(_dotAutoShuffleDelay);

        // Enable input
        DotsInputHandler.Instance.IsInputEnabled = true;
    }
    
    /// <summary>
    /// Animates a new dot being dropped onto its target position
    /// </summary>
    /// <param name="col">The column the new dot is in </param>
    /// <param name="row">The row the new dot is in</param>
    /// <param name="startPosition"> The position where the new dot will start at</param>
    /// <param name="moveSpeed"> How fast the new dot will drop to the target position</param>
    /// <returns></returns>
    private IEnumerator GenerateDotAtPosition(int col, int row, Vector2 startPosition, float moveSpeed)
    {
        // Create a new dot with the target row & column on a start position
        Dot newDot = DotsGenerator.Instance.InitializeDotAtPosition(col, row, startPosition);
        if (newDot == null)
            yield break;

        // Place the new dot in the board
        DotsBoard.Instance.PlaceDotAt(col, row, newDot);

        // Move the new dot to the target position
        Vector2 targetPosition = new Vector2(newDot.Position.x, DotsBoard.Instance.GetYAt(row));
        yield return newDot.MoveTo(targetPosition, moveSpeed);
    }
    
    /// <summary>
    /// If there are no possible connections, then call the shuffle board coroutine
    /// </summary>
    /// <param name="onShuffleCompletedDelay"></param>
    /// <returns>A reshuffled board with possible connections</returns>
    private IEnumerator AutoShuffleBoard(float onShuffleCompletedDelay)
    {
        while (!DotsBoard.Instance.IsPossibleDotConnections())
        {
            yield return ShuffleBoard();
            yield return new WaitForSeconds(onShuffleCompletedDelay);
        }
    }
    
    /// <summary>
    /// Animates the shuffling of randomly selected dots that leads to a board with possible connections
    /// </summary>
    private IEnumerator ShuffleBoard()
    {
        // Disable input
        DotsInputHandler.Instance.IsInputEnabled = false;
        
        // Keep track of the dots chosen to be swapped
        List<Tuple<Dot, Dot>> twoDotsToShuffleList = new List<Tuple<Dot, Dot>>();
        
        
        // Loop through the dots board matrix looking for two dots to swap
        int boardWidth = DotsBoard.Instance.BoardWidth;
        int boardHeight = DotsBoard.Instance.BoardHeight;
        for (int col = 0; col < boardWidth; col++)
        {
            for (int row = 0; row < boardHeight; row++)
            {
                // Assign the dot we are looking at to the current dot's variable, cur
                Dot cur = DotsBoard.Instance.GetDotAt(col, row);
                
                // Continue the for loop if the current dot has been visited already
                if (DotsBoard.Instance.IsDotVisited(cur))
                    continue;

                // Get all of the unvisited dots around the current dot with different colors
                List<Dot> dotsAroundCur = DotsBoard.Instance.GetDifferentColoredDotsAround(cur);
                dotsAroundCur.RemoveAll((dot) => DotsBoard.Instance.IsDotVisited(dot));
                
                // No values in the list means either the colors are the same or those dots have already been visited
                // Either way, continue the for loop
                if (dotsAroundCur.Count <= 0)
                    continue;

                // Get a random dot in the different colored dots list
                Dot diffColorDot = dotsAroundCur[Random.Range(0, dotsAroundCur.Count)];

                // Get all of the unvisited dots around the randomly selected dot with the same color as the current dot
                List<Dot> dotsAroundDiffColorDot = DotsBoard.Instance.GetSameColoredDotsAround(diffColorDot, cur.Color);
                dotsAroundDiffColorDot.RemoveAll((dot) => DotsBoard.Instance.IsDotVisited(dot));
                
                // No values in the list means either the colors are different or those dots have already been visited
                // Either way, continue the for loop
                if (dotsAroundDiffColorDot.Count <= 0)
                    continue;
                
                // Get a random dot in the same colored dots list
                Dot sameColorDot = dotsAroundDiffColorDot[Random.Range(0, dotsAroundDiffColorDot.Count)];

                // Add these two random dots into the shuffle list
                Tuple<Dot, Dot> twoDotsToShuffle = new Tuple<Dot, Dot>(diffColorDot, sameColorDot);
                twoDotsToShuffleList.Add(twoDotsToShuffle);
                
                // Mark these dots as visited so that they won't be checked again
                DotsBoard.Instance.SetDotVisited(diffColorDot, true);
                DotsBoard.Instance.SetDotVisited(sameColorDot, true);
            }
        }
        
        // Mark all of the dots as unvisited
        DotsBoard.Instance.UnvisitAllDots();

        // Loop through the two dots shuffle list and start swapping each of the two dots
        // Keep track of the number of times that we finish shuffling a pair of dots
        int numTimesShuffled = 0;
        foreach (Tuple<Dot, Dot> twoDots in twoDotsToShuffleList)
        {
            Dot dotOne = twoDots.Item1;
            Dot dotTwo = twoDots.Item2;

            StartCoroutine(SwapDots(dotOne, dotTwo, _dotSwapSpeed, () => numTimesShuffled++));
        }

        // Wait until all of the dots are swapped with the numTimesShuffled variable
        yield return new WaitUntil(() => numTimesShuffled == twoDotsToShuffleList.Count);
    }

    /// <summary>
    /// Animate the swapping of two dots by moving their positions from one to the other
    /// </summary>
    /// <param name="dotOne">The 1st being swapped</param>
    /// <param name="dotTwo">The 2nd being swapped</param>
    /// <param name="swapSpeed">How fast the dots are being swapped</param>
    /// <param name="onSwapCompleted">The callback that will be execute when the swap is completed</param>
    /// <returns>The 1st dot in the 2nd's dot position & the 2nd dot in the 1st dot's position</returns>
    private IEnumerator SwapDots(Dot dotOne, Dot dotTwo, float swapSpeed, Action onSwapCompleted)
    {
        // Cache each of the dot's positions
        Vector2 dotOnePosition = dotOne.Position;
        Vector2 dotTwoPosition = dotTwo.Position;

        // Cache each of the columns & rows
        int dotOneCol = dotOne.Col;
        int dotOneRow = dotOne.Row;
        
        int dotTwoCol = dotTwo.Col;
        int dotTwoRow = dotTwo.Row;

        // Move both the 1st dot to the 2nd's position & the 2nd's dot to the 1st position at the same time & speed
        StartCoroutine(dotOne.MoveTo(dotTwoPosition, swapSpeed));
        StartCoroutine(dotTwo.MoveTo(dotOnePosition, swapSpeed));

        // Wait until both dots are at their target position
        yield return new WaitUntil(() => dotOne.IsAt(dotTwoCol, dotTwoRow) && dotTwo.IsAt(dotOneCol, dotOneRow));
        
        // Update the dots board matrix by setting the new columns & rows of the 1st and 2nd dot
        DotsBoard.Instance.PlaceDotAt(dotOneCol, dotOneRow, dotTwo);
        DotsBoard.Instance.PlaceDotAt(dotTwoCol, dotTwoRow, dotOne);

        // The swap is now completed, execute this callback function if there it exists
        onSwapCompleted?.Invoke();
    }
}
