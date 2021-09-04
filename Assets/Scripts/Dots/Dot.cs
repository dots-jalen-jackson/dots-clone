using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dot : EventTrigger
{
    public int Row { get; set; }
    public int Col { get; set; }
    public Dot PreviousDot { get; set; }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.AddDotToLine(this);
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
        bool isPointedToNeighbor = this.IsNeighborOf(lastDotPointed);
        if (isPointedToNeighbor)
        {
            bool isBackAtLastDotPointed = this.IsBackTo(lastDotPointed);
            if (isBackAtLastDotPointed)
            {
                DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
                lastDotPointed.PreviousDot = null;
            }
            else
            {
                DotsLineRenderer.Instance.ConnectDotsInLine(lastDotPointed, this);
                PreviousDot = lastDotPointed;
            }
            
            eventData.pointerDrag = gameObject;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.ClearLine();
        DotsBoard.Instance.ResetPreviousDots();
    }

    private bool IsNeighborOf(Dot dot)
    {
        List<Dot> neighbors = DotsBoard.Instance.GetNeighbors(this);
        return neighbors.Contains(dot);
    }

    private bool IsBackTo(Dot dot)
    {
        return this == dot.PreviousDot;
    }
}
