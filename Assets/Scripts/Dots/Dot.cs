using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dot : EventTrigger
{
    public int Row { get; set; }
    public int Col { get; set; }

    public Color Color
    {
        get => GetComponent<Image>().color;
        set => GetComponent<Image>().color = value;
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
        
        bool isLastDotNeighbor = this.IsAround(lastDotPointed);
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
            dotsToRemove = DotsBoard.Instance.GetDotsInLineFrom(this);
        }
        else
        {
            dotsToRemove = DotsBoard.Instance.GetDotsWithColor(Color);
        }
        
        DotsBoard.Instance.ResetBoard();
        DotsLineRenderer.Instance.ClearLine();

        if (dotsToRemove.Count <= 0)
            return;
        
        //TODO Remove dots in list
    }

    private bool IsAround(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetSameColoredDotsAround(dot);
        return surroundingDots.Contains(this);
    }
}
