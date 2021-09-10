using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

public class Dot : EventTrigger
{
    private RectTransform _rectTransform;
    private Image _image;

    public int Row => DotsBoard.Instance.GetRowAtPosition(Position);
    public int Col => DotsBoard.Instance.GetColAtPosition(Position);

    public Color Color
    {
        get => _image.color;
        set => _image.color = value;
    }

    public Vector2 Position
    {
        get => _rectTransform.anchoredPosition;
        set => _rectTransform.anchoredPosition = value;
    }

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.AddDotToLine(this);
        DotsLineRenderer.Instance.SetLineColor(Color);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
            DotsLineRenderer.Instance.SetCurrentPosition(eventData.position);
        else
            DotsLineRenderer.Instance.SetCurrentPosition(transform.position);
        
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!eventData.dragging)
            return;
        
        Dot lastDotPointed = eventData.pointerDrag.GetComponent<Dot>();
        
        bool isLastDotNeighbor = this.IsAroundSameColoredDot(lastDotPointed);
        if (!isLastDotNeighbor) 
            return;
        
        bool isEdgeExists = DotsBoard.Instance.ContainsEdge(lastDotPointed, this);
        if (isEdgeExists)
        {
            bool isBackAtPreviousDot = DotsBoard.Instance.IsDotPreviousSource(this);
            if (!isBackAtPreviousDot)
                return;
            
            DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
            DotsBoard.Instance.RemoveEdge(lastDotPointed, this);
        }
        else
        {
            DotsLineRenderer.Instance.ConnectDots(lastDotPointed, this);
            DotsBoard.Instance.AddEdge(lastDotPointed, this);
        }
        
        eventData.pointerDrag = gameObject;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        List<Dot> dotsToRemove;
        if (!DotsBoard.Instance.IsSquareFormed())
        {
            dotsToRemove = DotsBoard.Instance.GetDotsOnLineFrom(this);
        }
        else
        {
            dotsToRemove = DotsBoard.Instance.GetDotsWithColor(Color);

            List<Dot> square = DotsBoard.Instance.GetDotsOnLineFrom(this).Distinct().ToList();
            List<Dot> dotsInSquare = DotsBoard.Instance.GetDotsInSquare(square);

            dotsToRemove.AddRange(dotsInSquare);
            dotsToRemove = dotsToRemove.Distinct().ToList();
        }
        
        DotsBoard.Instance.ResetBoard();
        DotsLineRenderer.Instance.ClearLine();

        if (dotsToRemove.Count <= 0)
            return;
        
        //TODO Remove dots in list
        DotsBoard.Instance.RemoveDots(dotsToRemove);
    }

    private bool IsAroundSameColoredDot(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetSameColoredDotsAround(dot);
        return surroundingDots.Contains(this);
    }
}
