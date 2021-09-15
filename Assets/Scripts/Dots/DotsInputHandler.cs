using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DotsInputHandler : Singleton<DotsInputHandler>
{
    /// <summary>
    /// Cache the main camera for getting the screen to world point position of the mouse cursor
    /// </summary>
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// Toggles input detection for dot event functions below
    /// </summary>
    public bool IsInputEnabled { get; set; }

    /// <summary>
    /// Clicking the left mouse button highlights the button
    /// Clicking the right mouse button when there is no line formed removes the dot
    /// </summary>
    /// <param name="dot">The dot being clicked on</param>
    /// <param name="eventData">Input detection</param>
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
                
                DotsBoardUpdater.Instance.StartRemoveDot(dot);
                break;
        }
    }

    /// <summary>
    /// Clicking on the dot then dragging the mouse out forms a new line starting on that dot
    /// </summary>
    /// <param name="dot">The dot being clicked on</param>
    public void OnDotBeginLine(Dot dot)
    {
        if (!IsInputEnabled)
            return;
        
        DotsLineRenderer.Instance.AddDotToLine(dot);
        DotsLineRenderer.Instance.SetLineColor(dot.Color);
    }

    /// <summary>
    /// Holding the mouse button updates the last position of the line
    /// </summary>
    /// <param name="dot">The last dot that was entered in while updating the line</param>
    /// <param name="eventData">Input detection</param>
    public void OnDotUpdatingLine(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        if (eventData.pointerEnter != dot.gameObject)
        {
            // Entering in a dot while holding it sets it to dot's position
            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(eventData.position);
            DotsLineRenderer.Instance.SetCurrentPosition(mousePosition);
        }
        else
        {
            // Not entering in a dot sets it to the mouse cursor's position
            DotsLineRenderer.Instance.SetCurrentPosition(dot.transform.position);
        }
    }

    /// <summary>
    /// Hovering over a new dot while dragging the mouse updates the line
    /// </summary>
    /// <param name="dot">The dot the mouse cursor on</param>
    /// <param name="eventData">Input information</param>
    public void OnDotLineUpdated(Dot dot, PointerEventData eventData)
    {
        if (!IsInputEnabled)
            return;
        
        if (!eventData.dragging)
            return;
        
        //Check if the last dot pointed has the same color and is a neighbor to the dot the mouse cursor is on
        Dot lastDotPointed = eventData.pointerDrag.GetComponent<Dot>();
        
        bool isLastDotNeighbor = dot.IsAroundSameColoredDot(lastDotPointed);
        if (!isLastDotNeighbor) 
            return;
        
        // Check if a connection between the last dot pointed & the dot the mouse cursor is on
        bool isEdgeExists = DotsBoard.Instance.ContainsEdge(lastDotPointed, dot);
        if (isEdgeExists)
        {
            // Remove the connection between these two dots if the mouse cursor is back where it previously was
            bool isBackAtPreviousDot = DotsBoard.Instance.IsDotPreviousSource(dot);
            if (!isBackAtPreviousDot)
                return;
            
            DotsBoard.Instance.RemoveEdge(lastDotPointed, dot);
            DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
        }
        else
        {
            // Add the newly discovered connection between these two dots
            DotsBoard.Instance.AddEdge(lastDotPointed, dot);
            DotsLineRenderer.Instance.ConnectDots(lastDotPointed, dot);
        }
        
        // Update the last dot pointed to the dot the mouse cursor is on
        eventData.pointerDrag = dot.gameObject;
        
        // Highlight the dot the mouse cursor is on if its in a line
        if (DotsLineRenderer.Instance.IsLine)
            dot.Highlight();
    }

    /// <summary>
    /// Clears the line then removes all of the dots in the line when we stop holding the mouse
    /// </summary>
    /// <param name="dot">The most recent dot that we connected with in the line</param>
    public void OnDotEndLine(Dot dot)
    {
        if (!IsInputEnabled)
            return;
        
        DotsLineRenderer.Instance.ClearLines();

        List<Dot> dotsToRemove = DotsBoard.Instance.GetDotsToRemove(dot);
        DotsBoardUpdater.Instance.StartRemovingDots(dotsToRemove);
    }
}
