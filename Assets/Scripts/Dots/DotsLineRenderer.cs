﻿using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DotsLineRenderer : Singleton<DotsLineRenderer>
{
    [SerializeField] 
    private DotsHUDLineRenderer _topHUDLineRenderer;
    
    [SerializeField] 
    private DotsHUDLineRenderer _bottomHUDLineRenderer;
    
    private LineRenderer _lineRenderer;

    private int _currentIndex;

    private Camera _mainCamera;

    public bool IsLine => _lineRenderer.positionCount > 1;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        
        _currentIndex = 0;
        
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 1;
    }
    
    public void SetLineColor(Color color)
    {
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;

        _topHUDLineRenderer.SetColor(color);
        _bottomHUDLineRenderer.SetColor(color);
    }

    public void ClearLine()
    {
        _currentIndex = 0;
        
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(_currentIndex, Vector3.zero);
        
        _topHUDLineRenderer.ClearLine();
        _bottomHUDLineRenderer.ClearLine();
    }

    public void AddDotToLine(Dot dot)
    {
        UpdateCurrentPosition(dot.transform.position);
    }

    public void ConnectDots(Dot beginDot, Dot endDot)
    {
        Vector2 beginPosition = beginDot.transform.position;
        Vector2 endPosition = endDot.transform.position;
        
        StrengthenConnectionBetweenDots(beginPosition, endPosition);
        AddDotToLine(endDot);

        if (!DotsBoard.Instance.IsSquareFormed())
        {
            _topHUDLineRenderer.IncreaseLineAtTop();
            _bottomHUDLineRenderer.IncreaseLineAtBottom();
        }
        else
        {
            _topHUDLineRenderer.MakeTopHalfOfSquare();
            _bottomHUDLineRenderer.MakeBottomHalfOfSquare();
        }
    }
    
    public void RemoveLastConnectedDotInLine()
    {
        _lineRenderer.positionCount -= 3;
        _currentIndex -= 3;
        
        _topHUDLineRenderer.DecreaseLineAtTop();
        _bottomHUDLineRenderer.DecreaseLineAtBottom();
    }

    public void SetCurrentPosition(Vector2 position)
    {
        Vector2 curPosition = _mainCamera.ScreenToWorldPoint(position);
        _lineRenderer.SetPosition(_currentIndex, curPosition);
    }
    
    private void UpdateCurrentPosition(Vector2 position)
    {
        _lineRenderer.positionCount++;
        
        Vector2 curPosition = _mainCamera.ScreenToWorldPoint(position);
        _lineRenderer.SetPosition(_currentIndex++, curPosition);
    }
    
    private void StrengthenConnectionBetweenDots(Vector2 dotBeginPosition, Vector2 dotEndPosition)
    {
        Vector2 leftMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.25f);
        UpdateCurrentPosition(leftMidPosition);

        Vector2 rightMidPosition = Vector2.Lerp(dotBeginPosition, dotEndPosition, 0.75f);
        UpdateCurrentPosition(rightMidPosition);
    }
}
