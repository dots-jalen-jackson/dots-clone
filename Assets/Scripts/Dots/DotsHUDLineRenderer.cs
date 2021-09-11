using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DotsHUDLineRenderer : MonoBehaviour
{
    [SerializeField] 
    private float _increaseScaleMultiplier = 5.0f;
    
    private LineRenderer _lineRenderer;

    private Camera _mainCamera;

    private int _timesIncreasedLine = 0;
    
    private float TopOfLine => _mainCamera.ViewportToWorldPoint(Vector3.up).y;

    private float LeftCorner => _mainCamera.ViewportToWorldPoint(Vector3.up).x;
    
    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _mainCamera = Camera.main;
    }

    public void SetColor(Color color)
    {
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
    }
    public void IncreaseLineAtTop(float by = 1.0f)
    {
        _lineRenderer.positionCount = 2;
        
        Vector2 topLeftPosition = new Vector2(_lineRenderer.GetPosition(0).x + -_increaseScaleMultiplier * by, TopOfLine);
        Vector2 topRightPosition = new Vector2(_lineRenderer.GetPosition(1).x + _increaseScaleMultiplier * by, TopOfLine);
        
        _lineRenderer.SetPosition(0, topLeftPosition);
        _lineRenderer.SetPosition(1, topRightPosition);

        _timesIncreasedLine++;
    }

    public void IncreaseLineAtBottom(float by = 1.0f)
    {
        _lineRenderer.positionCount = 2;
        
        Vector2 botLeftPosition = new Vector2(_lineRenderer.GetPosition(0).x + -_increaseScaleMultiplier * by, -TopOfLine);
        Vector2 botRightPosition = new Vector2(_lineRenderer.GetPosition(1).x + _increaseScaleMultiplier * by, -TopOfLine);
        
        _lineRenderer.SetPosition(0, botLeftPosition);
        _lineRenderer.SetPosition(1, botRightPosition);

        _timesIncreasedLine++;
    }
    
    public void DecreaseLineAtTop(float by = 1.0f)
    {
        if (_lineRenderer.positionCount == 4)
        {
            ClearLine(false);
            IncreaseLineAtTop(_timesIncreasedLine + 1);
        }
        
        _lineRenderer.positionCount = 2;

        Vector2 topLeftPosition = new Vector2(_lineRenderer.GetPosition(0).x - -_increaseScaleMultiplier * by, TopOfLine);
        Vector2 topRightPosition = new Vector2(_lineRenderer.GetPosition(1).x - _increaseScaleMultiplier * by, TopOfLine);
        
        _lineRenderer.SetPosition(0, topLeftPosition);
        _lineRenderer.SetPosition(1, topRightPosition);

        _timesIncreasedLine--;
    }

    public void DecreaseLineAtBottom(float by = 1.0f)
    {
        if (_lineRenderer.positionCount == 4)
        {
            ClearLine(false);
            IncreaseLineAtBottom(_timesIncreasedLine + 1);
        }
        
        _lineRenderer.positionCount = 2;

        Vector2 botLeftPosition = new Vector2(_lineRenderer.GetPosition(0).x - -_increaseScaleMultiplier * by, -TopOfLine);
        Vector2 botRightPosition = new Vector2(_lineRenderer.GetPosition(1).x - _increaseScaleMultiplier * by, -TopOfLine);
        
        _lineRenderer.SetPosition(0, botLeftPosition);
        _lineRenderer.SetPosition(1, botRightPosition);

        _timesIncreasedLine--;
    }

    public void MakeTopHalfOfSquare()
    {
        if (_lineRenderer.positionCount == 4)
            return;
        
        _lineRenderer.positionCount = 4;
        
        _lineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _lineRenderer.SetPosition(1, new Vector2(LeftCorner, TopOfLine));
        _lineRenderer.SetPosition(2, new Vector2(-LeftCorner, TopOfLine));
        _lineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
    }
    
    public void MakeBottomHalfOfSquare()
    {
        if (_lineRenderer.positionCount == 4)
            return;
        
        _lineRenderer.positionCount = 4;
        
        _lineRenderer.SetPosition(0, new Vector2(LeftCorner, 0));
        _lineRenderer.SetPosition(1, new Vector2(LeftCorner, -TopOfLine));
        _lineRenderer.SetPosition(2, new Vector2(-LeftCorner, -TopOfLine));
        _lineRenderer.SetPosition(3, new Vector2(-LeftCorner, 0));
    }

    public void ClearLine(bool clearTimesIncreased = true)
    {
        if (clearTimesIncreased)
            _timesIncreasedLine = 0;
        
        for (int i = 0; i < _lineRenderer.positionCount; i++)
            _lineRenderer.SetPosition(i, Vector3.zero);
    }
}
