using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DotsLineRenderer : Singleton<DotsLineRenderer>
{
    // Retrieve the line renderer components of the top HUD, bottom HUD, & dots line
    [SerializeField] 
    private LineRenderer _topHUDLineRenderer;

    [SerializeField] 
    private LineRenderer _bottomHUDLineRenderer;
    
    private LineRenderer _dotsLineRenderer;

    /// <summary>
    /// Stores the last index of the line
    /// </summary>
    private int _currentIndex;

    /// <summary>
    /// Stores the number of dots in a line
    /// </summary>
    private int _numDots;

    /// <summary>
    /// Cache the main camera for getting the top & left corner of the screen
    /// </summary>
    private Camera _mainCamera;

    /// <summary>
    /// Returns the left corner of the screen at x
    /// Setting it to negative returns the right corner of the screen at x
    /// </summary>
    private float LeftCorner => _mainCamera.ViewportToWorldPoint(Vector3.up).x;
    
    /// <summary>
    /// Returns the top of the screen at y
    /// Setting it to negative returns the bottom of the screen at y
    /// </summary>
    private float TopOfScreen => _mainCamera.ViewportToWorldPoint(Vector3.up).y;

    /// <summary>
    /// A line exists as long as there is a dot added in the line
    /// </summary>
    public bool IsLine => _dotsLineRenderer.positionCount > 1;
    
    /// <summary>
    /// Initialize the main camera, current index, number of dots, and the dots line renderer
    /// </summary>
    void Start()
    {
        _mainCamera = Camera.main;
        
        _currentIndex = 0;
        _numDots = 0;
        
        _dotsLineRenderer = GetComponent<LineRenderer>();
        _dotsLineRenderer.positionCount = 1;
    }
    
    /// <summary>
    /// Sets the colors for the top HUD line, bottom HUD line, & the dots line
    /// </summary>
    /// <param name="color"></param>
    public void SetLineColor(Color color)
    {
        _dotsLineRenderer.startColor = color;
        _dotsLineRenderer.endColor = color;

        _topHUDLineRenderer.startColor = color;        
        _topHUDLineRenderer.endColor = color;

        _bottomHUDLineRenderer.startColor = color;
        _bottomHUDLineRenderer.endColor = color;
    }

    /// <summary>
    /// Removes the lines being currently updated
    /// </summary>
    public void ClearLines()
    {
        // Reset the current index & number of dots to 0
        _currentIndex = 0;
        _numDots = 0;
        
        // Reset the number of positions in the dot line to 0
        _dotsLineRenderer.positionCount = 1;
        _dotsLineRenderer.SetPosition(_currentIndex, Vector3.zero);
        
        // Clears the top & bottom HUD lines
        ClearTopLine();
        ClearBottomLine();
    }

    /// <summary>
    /// Adds a new position to the dots line on this dot's position.
    /// Increments the number of dots variable in the line
    /// </summary>
    /// <param name="dot">The dot being added in the line</param>
    public void AddDotToLine(Dot dot)
    {
        UpdateCurrentPosition(dot.transform.position);
        _numDots++;
    }

    /// <summary>
    /// Adds the endDot to the line from the beginDot
    /// Strengthens the connection between these two dots to prevent stretching
    /// Updates the top & bottom HUD lines in response
    /// </summary>
    /// <param name="beginDot">The last dot that was connected</param>
    /// <param name="endDot">The new dot that is being connected in this function</param>
    public void ConnectDots(Dot beginDot, Dot endDot)
    {
        Vector2 beginPosition = beginDot.transform.position;
        Vector2 endPosition = endDot.transform.position;
        
        StrengthenConnectionBetweenDots(beginPosition, endPosition);
        AddDotToLine(endDot);
        
        if (!DotsBoard.Instance.IsSquareFormed)
        {
            UpdateTopHUDLine();
            UpdateBottomHUDLine();
        }
        else
            MakeSquareInHUD();
    }
    
    /// <summary>
    /// Removes the last connection made out of the line
    /// </summary>
    public void RemoveLastConnectedDotInLine()
    {
        // Decrement the current index & dots line's position count by 3 since the strengthen function adds 2 positions
        _dotsLineRenderer.positionCount -= 3;
        _currentIndex -= 3;
        
        // Decrement the number of dots by 1 since we're removing a dot out of the line
        _numDots--;
        
        // Keep the square formation the top & bottom HUD lines make if a square still exists in the line
        if (DotsBoard.Instance.IsSquareFormed)
            return;
        
        // Otherwise, update the top & bottom HUD lines in response
        UpdateTopHUDLine();
        UpdateBottomHUDLine();
    }

    /// <summary>
    /// Sets the position of dots line at the current index
    /// </summary>
    /// <param name="position">Position being set</param>
    public void SetCurrentPosition(Vector2 position)
    {
        _dotsLineRenderer.SetPosition(_currentIndex, position);
    }
    
    /// <summary>
    /// Increments the dots line's position count and sets the position of the new currentIndex
    /// </summary>
    /// <param name="position">Position being set</param>
    private void UpdateCurrentPosition(Vector2 position)
    {
        _dotsLineRenderer.positionCount++;
        _dotsLineRenderer.SetPosition(_currentIndex++, position);
    }
    
    /// <summary>
    /// Adds 2 positions to the dots line to prevent stretching in between the dotBeginPosition & dotEndPosition
    /// </summary>
    /// <param name="dotBeginPosition">The position the first dot is on</param>
    /// <param name="dotEndPosition">The position the second dot is on</param>
    private void StrengthenConnectionBetweenDots(Vector2 dotBeginPosition, Vector2 dotEndPosition)
    {
        // Add position 25% from the begin and end position
        Vector2 leftMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.25f);
        UpdateCurrentPosition(leftMidPosition);

        // Add position 75% from the begin and end position
        Vector2 rightMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.75f);
        UpdateCurrentPosition(rightMidPosition);
    }
    
    #region Dots HUD Line Renderer
    
    /// <summary>
    /// Increase or decrease the top HUD line depending on the number of dots
    /// </summary>
    private void UpdateTopHUDLine()
    {
        // Get the top positions of the screen from the center, left, and right sides
        Vector2 topCenter = new Vector2(0, TopOfScreen);
        
        Vector2 topLeftSide = new Vector2(LeftCorner, TopOfScreen);
        Vector2 topRightSide = new Vector2(-LeftCorner, TopOfScreen);

        // Calculate how far it takes to reach the top's sides based on the number of dots and the width of the line
        float topT = (_numDots - 1) * (1f / _topHUDLineRenderer.startWidth);
        Vector2 topHUDLineLeftSide = Vector2.Lerp(topCenter, topLeftSide, topT);
        Vector2 topHUDLineRightSide = Vector2.Lerp(topCenter, topRightSide, topT);

        // When this value reaches the left and right side, increases the line's position to 4
        _topHUDLineRenderer.positionCount = topT <= 1.0f ? 2 : 4;
        
        switch (_topHUDLineRenderer.positionCount)
        {
            case 2:
                // Set the positions in 0 & 1 to the Vector2s that we calculated above
                _topHUDLineRenderer.SetPosition(0, topHUDLineLeftSide);
                _topHUDLineRenderer.SetPosition(1, topHUDLineRightSide);
                break;
            case 4:
                // Set the positions in 1 & 2 to the Vector2s that we calculated above
                _topHUDLineRenderer.SetPosition(1, topLeftSide);
                _topHUDLineRenderer.SetPosition(2, topRightSide);
                
                // Get the mid positions for the left & right sides
                Vector2 midCenterLeft = new Vector2(LeftCorner, 0);
                Vector2 midCenterRight = new Vector2(-LeftCorner, 0);

                // Calculate how far it takes to get to reach the middle from the top left & top right sides
                float midT = (_numDots * (1f / _topHUDLineRenderer.startWidth)) - 1f;
                Vector2 topHUDLineCenterLeftSide = Vector2.Lerp(topLeftSide, midCenterLeft, midT);
                Vector2 topHUDLineCenterRightSide = Vector2.Lerp(topRightSide, midCenterRight, midT);
                
                // Set the positions in 0 and 3 to the Vector2s above
                _topHUDLineRenderer.SetPosition(0, topHUDLineCenterLeftSide);
                _topHUDLineRenderer.SetPosition(3, topHUDLineCenterRightSide);
                
                break;
        }
    }

    /// <summary>
    /// Increases or decreases the top HUD line depending on the number of dots
    /// </summary>
    private void UpdateBottomHUDLine()
    {
        // Get the bottom positions of the screen from the center, left, and right sides
        Vector2 botCenter = new Vector2(0, -TopOfScreen);
        
        Vector2 botLeftSide = new Vector2(LeftCorner, -TopOfScreen);
        Vector2 botRightSide = new Vector2(-LeftCorner, -TopOfScreen);

        // Calculate how far it takes to reach the bottom's sides based on the number of dots & the width of the line
        float botT = (_numDots - 1) * (1f / _bottomHUDLineRenderer.startWidth);
        Vector2 botHUDLineLeftSide = Vector2.Lerp(botCenter, botLeftSide, botT);
        Vector2 botHUDLineRightSide = Vector2.Lerp(botCenter, botRightSide, botT);

        // When this value reaches the left and right side, increases the line's position to 4
        _bottomHUDLineRenderer.positionCount = botT <= 1.0f ? 2 : 4;

        switch (_bottomHUDLineRenderer.positionCount)
        {
            case 2:
                // Set the positions in 0 & 1 to the Vector2s that we calculated above
                _bottomHUDLineRenderer.SetPosition(0, botHUDLineLeftSide);
                _bottomHUDLineRenderer.SetPosition(1, botHUDLineRightSide);
                break;
            case 4: 
                // Set the positions in 1 & 2 to the Vector2s that we calculated above
                _bottomHUDLineRenderer.SetPosition(1, botLeftSide);
                _bottomHUDLineRenderer.SetPosition(2, botRightSide);
                
                // Get the mid positions for the left & right sides
                Vector2 midCenterLeft = new Vector2(LeftCorner, 0);
                Vector2 midCenterRight = new Vector2(-LeftCorner, 0);

                // Calculate how far it takes to get to reach the middle from the bottom left & top right sides
                float midT = (_numDots * (1f / _bottomHUDLineRenderer.startWidth)) - 1f;
                Vector2 botHUDLineCenterLeftSide = Vector2.Lerp(botLeftSide, midCenterLeft, midT);
                Vector2 botHUDLineCenterRightSide = Vector2.Lerp(botRightSide, midCenterRight, midT);
                
                // Set the positions in 0 and 3 to the Vector2s above
                _bottomHUDLineRenderer.SetPosition(0, botHUDLineCenterLeftSide);
                _bottomHUDLineRenderer.SetPosition(3, botHUDLineCenterRightSide);
                
                break;
        }
    }

    /// <summary>
    /// Uses the top HUD & bottom HUD line renderers to form a square
    /// </summary>
    private void MakeSquareInHUD()
    {
        //Don't reset the square when its already been set
        if (_topHUDLineRenderer.positionCount == 4 && _bottomHUDLineRenderer.positionCount == 4)
            return;
        
        // Create the top half of the square from the middle left & right to the top left & right of the screen
        _topHUDLineRenderer.positionCount = 4;
        _topHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _topHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, TopOfScreen));
        _topHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, TopOfScreen));
        _topHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
        
        // Create the top half of the square from the middle left & right to the bottom left & right of the screen
        _bottomHUDLineRenderer.positionCount = 4;
        _bottomHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _bottomHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, -TopOfScreen));
        _bottomHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, -TopOfScreen));
        _bottomHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
    }

    /// <summary>
    /// Removes all of the positions in the top HUD line & resets its position count to 2
    /// </summary>
    private void ClearTopLine()
    {
        for (int i = 0; i < _topHUDLineRenderer.positionCount; i++)
            _topHUDLineRenderer.SetPosition(i, Vector3.zero);

        _topHUDLineRenderer.positionCount = 2;
    }
    
    /// <summary>
    /// Removes all of the positions in the bottom HUD line & resets its position count to 2
    /// </summary>
    private void ClearBottomLine()
    {
        for (int i = 0; i < _bottomHUDLineRenderer.positionCount; i++)
            _bottomHUDLineRenderer.SetPosition(i, Vector3.zero);

        _bottomHUDLineRenderer.positionCount = 2;
    }
    
    #endregion
}
