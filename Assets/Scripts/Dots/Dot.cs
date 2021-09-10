using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

public class Dot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    private Image _dotImage;

    [SerializeField] 
    private Image _dotSelectedImage;

    [SerializeField, Range(1f, 10f)] 
    private float _dotRemoveScaleMulitplier = 5f;
    
    private RectTransform _rectTransform;

    private float ColliderSize => _rectTransform.rect.width;

    public int Row => DotsBoard.Instance.GetRowAtPosition(Position);
    public int Col => DotsBoard.Instance.GetColAtPosition(Position);

    public Color Color
    {
        get => _dotImage.color;
        set => _dotImage.color = value;
    }

    public Vector2 Position
    {
        get => _rectTransform.anchoredPosition;
        set => _rectTransform.anchoredPosition = value;
    }

    public float Size => _dotImage.GetComponent<RectTransform>().rect.width;

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Reset()
    {
        _rectTransform.localScale = Vector3.one;
        
        _dotSelectedImage.color = new Color(Color.r, Color.g, Color.b, 0.0f);
        _dotSelectedImage.rectTransform.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        Reset();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(OnDotSelected());
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.AddDotToLine(this);
        DotsLineRenderer.Instance.SetLineColor(Color);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
            DotsLineRenderer.Instance.SetCurrentPosition(eventData.position);
        else
            DotsLineRenderer.Instance.SetCurrentPosition(transform.position);
        
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
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
        
        StopAllCoroutines();
        StartCoroutine(OnDotSelected());
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        List<Dot> dotsToRemove;
        if (!DotsBoard.Instance.IsSquareFormed())
        {
            dotsToRemove = DotsBoard.Instance.GetDotsOnLineFrom(this);
        }
        else
        {
            dotsToRemove = DotsBoard.Instance.GetDotsWithColor(Color);

            List<Dot> square = DotsBoard.Instance.GetDotsOnLineFrom(this);
            List<Dot> dotsInSquare = DotsBoard.Instance.GetDotsInSquare(square);

            dotsToRemove.AddRange(dotsInSquare);
            dotsToRemove = dotsToRemove.Distinct().ToList();
        }

        foreach (Dot dot in dotsToRemove)
        {
            dot.StopAllCoroutines();
            dot.Reset();
        }

        DotsBoard.Instance.ResetBoard();
        DotsLineRenderer.Instance.ClearLine();

        if (dotsToRemove.Count <= 0)
            return;
        
        DotsBoard.Instance.RemoveDots(dotsToRemove);
    }

    public void Shrink(Action onShrinkCompleted)
    {
        StopAllCoroutines();
        StartCoroutine(OnDotRemoved(onShrinkCompleted));
    }

    private IEnumerator OnDotSelected()
    {
        Color dotSelectedStartColor = new Color(Color.r, Color.g, Color.b, 1.0f);
        Vector2 dotSelectedStartScale = Vector2.one;

        float scaleSpeed = ColliderSize / Size;
        float scaleMultiplier = scaleSpeed + 1;
        
        Color dotSelectedEndColor = new Color(Color.r, Color.g, Color.b, 0.0f);
        Vector2 dotSelectedEndScale = dotSelectedStartScale * scaleMultiplier;

        float t = 0.0f;
        while (t < 1.0f)
        {
            _dotSelectedImage.color = Color.Lerp(dotSelectedStartColor, dotSelectedEndColor, t);
            _dotSelectedImage.rectTransform.localScale = Vector2.Lerp(dotSelectedStartScale, dotSelectedEndScale, t);

            t += Time.deltaTime * scaleSpeed;
            yield return null;
        }
    }

    private IEnumerator OnDotRemoved(Action onShrinkCompleted)
    {
        Vector2 dotRemovedStartScale = Vector2.one;
        Vector2 dotRemovedEndScale = Vector2.zero;

        float t = 0.0f;
        while (t < 1.0f)
        {
            _rectTransform.localScale = Vector2.Lerp(dotRemovedStartScale, dotRemovedEndScale, t);

            t += Time.deltaTime * _dotRemoveScaleMulitplier;
            yield return null;
        }
        
        onShrinkCompleted?.Invoke();
    }

    private bool IsAroundSameColoredDot(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetSameColoredDotsAround(dot);
        return surroundingDots.Contains(this);
    }
}
