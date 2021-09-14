using System;
using System.Collections;
using System.Collections.Generic;
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
    
    [SerializeField]
    private float _dotRemoveScaleMulitplier;
    
    private RectTransform _rectTransform;

    private Color _color;

    private float ColliderSize => _rectTransform.rect.width;

    public int Row { get; set; }
    
    public int Col { get; set; }

    public Color Color => _color;

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

        _color = DotsGenerator.Instance.ColorPalette.GetRandomColor();
        
        _dotImage.color = Color;
        _dotSelectedImage.color = new Color(Color.r, Color.g, Color.b, 0.0f);
        _dotSelectedImage.rectTransform.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        Reset();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotClicked(this, eventData);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotBeginLine(this, eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotUpdatingLine(this, eventData);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotLineUpdated(this, eventData);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotEndLine(this);
    }

    public void Shrink(Action onShrinkCompleted)
    {
        StopAllCoroutines();
        StartCoroutine(OnDotRemoved(onShrinkCompleted));
    }

    public void Highlight()
    {
        StopAllCoroutines();
        StartCoroutine(OnDotSelected());
    }
    
    public bool IsAroundSameColoredDot(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetSameColoredDotsAround(dot);
        return surroundingDots.Contains(this);
    }
    
    public IEnumerator MoveTo(Vector2 endPosition, float moveSpeed)
    {
        Vector2 startPosition = Position;

        float t = 0.0f;
        while (t < 1.0f)
        {
            Position = Vector2.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * moveSpeed;
            yield return null;
        }

        Position = endPosition;
        Col = DotsBoard.Instance.GetColAtPosition(Position);
        Row = DotsBoard.Instance.GetRowAtPosition(Position);
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
}
