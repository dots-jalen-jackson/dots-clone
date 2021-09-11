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

    [SerializeField] 
    private float _increaseHUDLineScaleMultiplier = 1.5f;
    
    private LineRenderer _dotsLineRenderer;

    private int _currentIndex;

    private int _numDots;

    private Stack<Vector4> _topHUDLinePositions;

    private Stack<Vector4> _bottomHUDLinePositions;

    private Camera _mainCamera;

    private float LeftCorner => _mainCamera.ViewportToWorldPoint(Vector3.up).x;

    private float TopHUDLine => _mainCamera.ViewportToWorldPoint(Vector3.up).y;

    public bool IsLine => _dotsLineRenderer.positionCount > 1;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        
        _currentIndex = 0;
        _numDots = 0;
        
        _dotsLineRenderer = GetComponent<LineRenderer>();
        _dotsLineRenderer.positionCount = 1;

        _topHUDLinePositions = new Stack<Vector4>();
        _bottomHUDLinePositions = new Stack<Vector4>();
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
            IncreaseTopHUDLine();
            IncreaseBottomHUDLine();
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
        
        DecreaseTopHUDLine();
        DecreaseBottomHUDLine();
    }

    public void SetCurrentPosition(Vector2 position)
    {
        Vector2 curPosition = _mainCamera.ScreenToWorldPoint(position);
        _dotsLineRenderer.SetPosition(_currentIndex, curPosition);
    }
    
    private void UpdateCurrentPosition(Vector2 position)
    {
        _dotsLineRenderer.positionCount++;
        
        Vector2 curPosition = _mainCamera.ScreenToWorldPoint(position);
        _dotsLineRenderer.SetPosition(_currentIndex++, curPosition);
    }
    
    private void StrengthenConnectionBetweenDots(Vector2 dotBeginPosition, Vector2 dotEndPosition)
    {
        Vector2 leftMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.25f);
        UpdateCurrentPosition(leftMidPosition);

        Vector2 rightMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.75f);
        UpdateCurrentPosition(rightMidPosition);
    }
    
    #region Dots HUD Line Renderer
    private void IncreaseTopHUDLine()
    {
        _topHUDLineRenderer.positionCount = 2;

        float increaseLineBy = _increaseHUDLineScaleMultiplier * _numDots;
        
        Vector2 topLeftPosition = new Vector2(_topHUDLineRenderer.GetPosition(0).x - increaseLineBy, TopHUDLine);
        Vector2 topRightPosition = new Vector2(_topHUDLineRenderer.GetPosition(1).x + increaseLineBy, TopHUDLine);
        
        _topHUDLineRenderer.SetPosition(0, topLeftPosition);
        _topHUDLineRenderer.SetPosition(1, topRightPosition);
        
        _topHUDLinePositions.Push(new Vector4(topLeftPosition.x, topLeftPosition.y, topRightPosition.x, topRightPosition.y));
    }

    private void IncreaseBottomHUDLine()
    {
        _topHUDLineRenderer.positionCount = 2;
        
        float increaseLineBy = _increaseHUDLineScaleMultiplier * _numDots;
        
        Vector2 botLeftPosition = new Vector2(_bottomHUDLineRenderer.GetPosition(0).x - increaseLineBy, -TopHUDLine);
        Vector2 botRightPosition = new Vector2(_bottomHUDLineRenderer.GetPosition(1).x + increaseLineBy, -TopHUDLine);
        
        _bottomHUDLineRenderer.SetPosition(0, botLeftPosition);
        _bottomHUDLineRenderer.SetPosition(1, botRightPosition);
        
        _bottomHUDLinePositions.Push(new Vector4(botLeftPosition.x, botLeftPosition.y, botRightPosition.x, botRightPosition.y));
    }
    
    private void DecreaseTopHUDLine()
    {
        if (_numDots <= 1)
        {
            ClearTopLine();
            return;
        }

        if (_topHUDLineRenderer.positionCount == 4)
            _topHUDLineRenderer.positionCount = 2;
        else
            _topHUDLinePositions.Pop();

        Vector4 lastTopLinePositions = _topHUDLinePositions.Peek();

        Vector2 topLeftPosition = new Vector2(lastTopLinePositions.x, lastTopLinePositions.y);
        Vector2 topRightPosition = new Vector2(lastTopLinePositions.z, lastTopLinePositions.w);
        
        _topHUDLineRenderer.SetPosition(0, topLeftPosition);
        _topHUDLineRenderer.SetPosition(1, topRightPosition);
    }

    private void DecreaseBottomHUDLine()
    {
        if (_numDots <= 1)
        {
            ClearBottomLine();
            return;
        }

        if (_bottomHUDLineRenderer.positionCount == 4)
            _bottomHUDLineRenderer.positionCount = 2;
        else
            _bottomHUDLinePositions.Pop();
        
        Vector4 lastBottomLinePositions = _bottomHUDLinePositions.Peek();
        
        Vector2 botLeftPosition = new Vector2(lastBottomLinePositions.x, lastBottomLinePositions.y);
        Vector2 botRightPosition = new Vector2(lastBottomLinePositions.z, lastBottomLinePositions.w);
        
        _bottomHUDLineRenderer.SetPosition(0, botLeftPosition);
        _bottomHUDLineRenderer.SetPosition(1, botRightPosition);
    }

    private void MakeSquareInHUD()
    {
        if (_topHUDLineRenderer.positionCount == 4 && _bottomHUDLineRenderer.positionCount == 4)
            return;
        
        _topHUDLineRenderer.positionCount = 4;
        _topHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _topHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, TopHUDLine));
        _topHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, TopHUDLine));
        _topHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
        
        _bottomHUDLineRenderer.positionCount = 4;
        _bottomHUDLineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _bottomHUDLineRenderer.SetPosition(1, new Vector2(LeftCorner, -TopHUDLine));
        _bottomHUDLineRenderer.SetPosition(2, new Vector2(-LeftCorner, -TopHUDLine));
        _bottomHUDLineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
    }

    private void ClearTopLine()
    {
        _topHUDLinePositions.Clear();
        
        for (int i = 0; i < _topHUDLineRenderer.positionCount; i++)
            _topHUDLineRenderer.SetPosition(i, Vector3.zero);

        _topHUDLineRenderer.positionCount = 2;
    }
    
    private void ClearBottomLine()
    {
        _bottomHUDLinePositions.Clear();
        
        for (int i = 0; i < _bottomHUDLineRenderer.positionCount; i++)
            _bottomHUDLineRenderer.SetPosition(i, Vector3.zero);

        _bottomHUDLineRenderer.positionCount = 2;
    }
    
    #endregion
}
