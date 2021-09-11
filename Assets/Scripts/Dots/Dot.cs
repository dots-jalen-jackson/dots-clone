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

    public void Reset()
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
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                StartCoroutine(OnDotSelected());
                break;
            case PointerEventData.InputButton.Right:
                
                if (DotsLineRenderer.Instance.IsLine)
                    return;
                
                DotsBoard.Instance.RemoveDot(this);
                break;
        }
        
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
            
            DotsBoard.Instance.RemoveEdge(lastDotPointed, this);
            DotsLineRenderer.Instance.RemoveLastConnectedDotInLine();
        }
        else
        {
            DotsBoard.Instance.AddEdge(lastDotPointed, this);
            DotsLineRenderer.Instance.ConnectDots(lastDotPointed, this);
            
            if (DotsBoard.Instance.IsSquareFormed())
            {
                foreach (Dot dot in DotsBoard.Instance.GetDotsToRemove(this))
                {
                    dot.StopAllCoroutines();
                    dot.StartCoroutine(dot.OnDotSelected());
                }
            }
        }
        
        eventData.pointerDrag = gameObject;

        if (DotsBoard.Instance.IsSquareFormed()) 
            return;

        if (DotsLineRenderer.Instance.IsLine)
        {
            StopAllCoroutines();
            StartCoroutine(OnDotSelected());
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        DotsLineRenderer.Instance.ClearLines();
        DotsBoard.Instance.RemoveDots(this);
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
