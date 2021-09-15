using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DotsLineRenderer : Singleton<DotsLineRenderer>
{
    [SerializeField] 
    private LineRenderer _topHUDLineRenderer;

    [SerializeField] 
    private LineRenderer _bottomHUDLineRenderer;
    
    private LineRenderer _dotsLineRenderer;

    private int _currentIndex;

    private int _numDots;

    private Camera _mainCamera;

    private float LeftCorner => _mainCamera.ViewportToWorldPoint(Vector3.up).x;

    private float TopOfScreen => _mainCamera.ViewportToWorldPoint(Vector3.up).y;

    public bool IsLine => _dotsLineRenderer.positionCount > 1;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        
        _currentIndex = 0;
        _numDots = 0;
        
        _dotsLineRenderer = GetComponent<LineRenderer>();
        _dotsLineRenderer.positionCount = 1;
    }
    
    public void SetLineColor(Color color)
    {
        _dotsLineRenderer.startColor = color;
        _dotsLineRenderer.endColor = color;

        _topHUDLineRenderer.startColor = color;        
        _topHUDLineRenderer.endColor = color;

        _bottomHUDLineRenderer.startColor = color;
        _bottomHUDLineRenderer.endColor = color;
    }

    public void ClearLines()
    {
        _currentIndex = 0;
        _numDots = 0;
        
        _dotsLineRenderer.positionCount = 1;
        _dotsLineRenderer.SetPosition(_currentIndex, Vector3.zero);
        
        ClearTopLine();
        ClearBottomLine();
    }

    public void AddDotToLine(Dot dot)
    {
        UpdateCurrentPosition(dot.transform.position);
        _numDots++;
    }

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
    
    public void RemoveLastConnectedDotInLine()
    {
        _dotsLineRenderer.positionCount -= 3;
        _currentIndex -= 3;
        _numDots--;

        if (DotsBoard.Instance.IsSquareFormed)
            return;
        
        UpdateTopHUDLine();
        UpdateBottomHUDLine();
    }

    public void SetCurrentPosition(Vector2 position)
    {
        _dotsLineRenderer.SetPosition(_currentIndex, position);
    }
    
    private void UpdateCurrentPosition(Vector2 position)
    {
        _dotsLineRenderer.positionCount++;
        
        _dotsLineRenderer.SetPosition(_currentIndex++, position);
    }
    
    private void StrengthenConnectionBetweenDots(Vector2 dotBeginPosition, Vector2 dotEndPosition)
    {
        Vector2 leftMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.25f);
        UpdateCurrentPosition(leftMidPosition);

        Vector2 rightMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.75f);
        UpdateCurrentPosition(rightMidPosition);
    }
    
    #region Dots HUD Line Renderer
    private void UpdateTopHUDLine()
    {
        Vector2 topCenter = new Vector2(0, TopOfScreen);
        
        Vector2 topLeftSide = new Vector2(LeftCorner, TopOfScreen);
        Vector2 topRightSide = new Vector2(-LeftCorner, TopOfScreen);

        float topT = (_numDots - 1) * (1f / _topHUDLineRenderer.startWidth);

        Vector2 topHUDLineLeftSide = Vector2.Lerp(topCenter, topLeftSide, topT);
        Vector2 topHUDLineRightSide = Vector2.Lerp(topCenter, topRightSide, topT);

        _topHUDLineRenderer.positionCount = topT <= 1.0f ? 2 : 4;
        

        switch (_topHUDLineRenderer.positionCount)
        {
            case 2:
                _topHUDLineRenderer.SetPosition(0, topHUDLineLeftSide);
                _topHUDLineRenderer.SetPosition(1, topHUDLineRightSide);
                break;
            case 4:
                _topHUDLineRenderer.SetPosition(1, topLeftSide);
                _topHUDLineRenderer.SetPosition(2, topRightSide);
                
                Vector2 midCenterLeft = new Vector2(LeftCorner, 0);
                Vector2 midCenterRight = new Vector2(-LeftCorner, 0);

                float midT = (_numDots * (1f / _topHUDLineRenderer.startWidth)) - 1f;

                Vector2 topHUDLineCenterLeftSide = Vector2.Lerp(topLeftSide, midCenterLeft, midT);
                Vector2 topHUDLineCenterRightSide = Vector2.Lerp(topRightSide, midCenterRight, midT);
                
                _topHUDLineRenderer.SetPosition(0, topHUDLineCenterLeftSide);
                _topHUDLineRenderer.SetPosition(3, topHUDLineCenterRightSide);
                
                break;
        }
    }

    private void UpdateBottomHUDLine()
    {
        Vector2 botCenter = new Vector2(0, -TopOfScreen);
        
        Vector2 botLeftSide = new Vector2(LeftCorner, -TopOfScreen);
        Vector2 botRightSide = new Vector2(-LeftCorner, -TopOfScreen);

        float botT = (_numDots - 1) * (1f / _bottomHUDLineRenderer.startWidth);

        Vector2 botHUDLineLeftSide = Vector2.Lerp(botCenter, botLeftSide, botT);
        Vector2 botHUDLineRightSide = Vector2.Lerp(botCenter, botRightSide, botT);

        _bottomHUDLineRenderer.positionCount = botT <= 1.0f ? 2 : 4;

        switch (_bottomHUDLineRenderer.positionCount)
        {
            case 2:
                _bottomHUDLineRenderer.SetPosition(0, botHUDLineLeftSide);
                _bottomHUDLineRenderer.SetPosition(1, botHUDLineRightSide);
                break;
            case 4:
                _bottomHUDLineRenderer.SetPosition(1, botLeftSide);
                _bottomHUDLineRenderer.SetPosition(2, botRightSide);
                
                Vector2 midCenterLeft = new Vector2(LeftCorner, 0);
                Vector2 midCenterRight = new Vector2(-LeftCorner, 0);

                float midT = (_numDots * (1f / _bottomHUDLineRenderer.startWidth)) - 1f;

                Vector2 botHUDLineCenterLeftSide = Vector2.Lerp(botLeftSide, midCenterLeft, midT);
                Vector2 botHUDLineCenterRightSide = Vector2.Lerp(botRightSide, midCenterRight, midT);
                
                _bottomHUDLineRenderer.SetPosition(0, botHUDLineCenterLeftSide);
                _bottomHUDLineRenderer.SetPosition(3, botHUDLineCenterRightSide);
                
                break;
        }
    }

    private void MakeSquareInHUD()
    {
        if (_topHUDLineRenderer.positionCount == 4 && _bottomHUDLineRenderer.positionCount == 4)
            return;
        
        _topHUDLineRenderer.positionCount = 4;
        _topHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _topHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, TopOfScreen));
        _topHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, TopOfScreen));
        _topHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
        
        _bottomHUDLineRenderer.positionCount = 4;
        _bottomHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _bottomHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, -TopOfScreen));
        _bottomHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, -TopOfScreen));
        _bottomHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
    }

    private void ClearTopLine()
    {
        for (int i = 0; i < _topHUDLineRenderer.positionCount; i++)
            _topHUDLineRenderer.SetPosition(i, Vector3.zero);

        _topHUDLineRenderer.positionCount = 2;
    }
    
    private void ClearBottomLine()
    {
        for (int i = 0; i < _bottomHUDLineRenderer.positionCount; i++)
            _bottomHUDLineRenderer.SetPosition(i, Vector3.zero);

        _bottomHUDLineRenderer.positionCount = 2;
    }
    
    #endregion
}
