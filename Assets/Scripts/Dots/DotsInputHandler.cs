using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class DotsInputHandler : Singleton<DotsInputHandler>
{
    public bool IsInputEnabled { get; set; }

    public void OnDotClicked(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                dot.Highlight();
                break;
            case PointerEventData.InputButton.Right:
                
                if (DotsLineRenderer.Instance.IsLine)
                    return;
                
                DotsBoard.Instance.RemoveDot(dot);
                break;
        }
    }

    public void OnDotBeginLine(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        DotsLineRenderer.Instance.AddDotToLine(dot);
        DotsLineRenderer.Instance.SetLineColor(dot.Color);
    }

    public void OnDotUpdatingLine(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        if (eventData.pointerEnter != dot.gameObject)
            DotsLineRenderer.Instance.SetCurrentPosition(eventData.position);
        else
            DotsLineRenderer.Instance.SetCurrentPosition(dot.transform.position);
    }

    public void OnDotLineUpdated(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        if (!eventData.dragging)
            return;
        
        Dot lastDotPointed = eventData.pointerDrag.GetComponent<Dot>();
        
        bool isLastDotNeighbor = dot.IsAroundSameColoredDot(lastDotPointed);
        if (!isLastDotNeighbor) 
            return;
        
        bool isEdgeExists = DotsBoard.Instance.ContainsEdge(lastDotPointed, dot);
        if (isEdgeExists)
        {
            bool isBackAtPreviousDot = DotsBoard.Instance.IsDotPreviousSource(dot);
            if (!isBackAtPreviousDot)
                return;
            
            DotsBoard.Instance.RemoveEdge(lastDotPointed, dot);
            DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
        }
        else
        {
            DotsBoard.Instance.AddEdge(lastDotPointed, dot);
            DotsLineRenderer.Instance.ConnectDots(lastDotPointed, dot);
        }
        
        eventData.pointerDrag = dot.gameObject;
        
        if (DotsLineRenderer.Instance.IsLine)
            dot.Highlight();
    }

    public void OnDotEndLine(Dot dot)
    {
        if (!IsInputEnabled)
            return;
        
        DotsLineRenderer.Instance.ClearLines();
        DotsBoard.Instance.RemoveDots(dot);
    }
}
