using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Dot : EventTrigger
{
    public int Row { get; set; }
    public int Col { get; set; }

    private Stack<Dot> _prevDots;

    private void Start()
    {
        _prevDots = new Stack<Dot>();
    }

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
        
        bool isLastDotNeighbor = this.IsAround(lastDotPointed);
        if (!isLastDotNeighbor) 
            return;
        
        bool isEdgeExists = DotsBoard.Instance.ContainsEdge(lastDotPointed, this);
        if (isEdgeExists)
            return;
        
        bool isEdgeExistsInOppositeDirection = DotsBoard.Instance.ContainsEdge(this, lastDotPointed);
        if (isEdgeExistsInOppositeDirection)
        {
            bool isOnPreviousDot = this == lastDotPointed._prevDots.Peek();
            if (!isOnPreviousDot)
                return;

            DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
            DotsBoard.Instance.RemoveEdge(this, lastDotPointed);
            DotsBoard.Instance.RemoveEdge(lastDotPointed, this);

            lastDotPointed._prevDots.Pop();
        }
        else
        {
            DotsLineRenderer.Instance.ConnectDots(lastDotPointed, this);
            DotsBoard.Instance.AddEdge(lastDotPointed, this);
            
            _prevDots.Push(lastDotPointed);
        }
            
        eventData.pointerDrag = gameObject;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.ClearLine();
        DotsBoard.Instance.ClearEdges();
        DotsBoard.Instance.UnvisitAllDots();
    }

    private bool IsAround(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetDotsAround(dot);
        return surroundingDots.Contains(this);
    }
}
