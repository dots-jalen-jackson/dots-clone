using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

public class Dot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler
{
    /// <summary>
    /// The image that stays static at all atimes
    /// </summary>
    [SerializeField]
    private Image _dotImage;

    /// <summary>
    /// The image that will be animated when the dot is highlighted
    /// </summary>
    [SerializeField] 
    private Image _dotSelectedImage;
    
    /// <summary>
    /// How fast the dot gets removed
    /// </summary>
    [SerializeField]
    private float _dotRemoveScaleMulitplier;
    
    /// <summary>
    /// Cache the information on the dot's rect transform
    /// </summary>
    private RectTransform _rectTransform;

    /// <summary>
    /// Cache the information on the color of the dot
    /// </summary>
    private Color _color;

    /// <summary>
    /// The width of the rect transform
    /// </summary>
    private float ColliderSize => _rectTransform.rect.width;

    /// <summary>
    /// The row that the dots is placed on the board
    /// </summary>
    public int Row { get; set; }
    
    /// <summary>
    /// The column that the dots is placed on the board
    /// </summary>
    public int Col { get; set; }

    public Color Color => _color;

    /// <summary>
    /// The dot's location inside of the canvas
    /// </summary>
    public Vector2 Position
    {
        get => _rectTransform.anchoredPosition;
        set => _rectTransform.anchoredPosition = value;
    }

    /// <summary>
    /// The width of the dot's image
    /// </summary>
    public float Size => _dotImage.GetComponent<RectTransform>().rect.width;

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Resets the scales of the rect transform and dot selected image to one
    /// Set the color of the dot to a random color
    /// </summary>
    public void Reset()
    {
        _rectTransform.localScale = Vector3.one;
        _dotSelectedImage.rectTransform.localScale = Vector3.one;
        
        SetRandomColor();
    }

    /// <summary>
    /// Enabling the dot resets it
    /// </summary>
    private void OnEnable()
    {
        Reset();
    }

    /// <summary>
    /// Execute OnDotClicked when the pointer is down on this dot
    /// </summary>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotClicked(this, eventData);
    }

    /// <summary>
    /// Execute OnDotBeginLine when the pointer starts dragging
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotBeginLine(this);
    }

    /// <summary>
    /// Execute OnDotUpdatingLine when the pointer is currently dragging
    /// </summary>
    public virtual void OnDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotUpdatingLine(this, eventData);
    }

    /// <summary>
    /// Execute OnDotLineUpdated when the pointer enters a new dot while dragging
    /// </summary>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotLineUpdated(this, eventData);
    }

    /// <summary>
    /// Execute OnDotEndLine when the pointer stops dragging
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        DotsInputHandler.Instance.OnDotEndLine(this);
    }

    /// <summary>
    /// Start the coroutine that decreases the scale to 0
    /// </summary>
    /// <param name="onShrinkCompleted">The function that will execute once the shrinking is done</param>
    public void Shrink(Action onShrinkCompleted)
    {
        StopAllCoroutines();
        StartCoroutine(OnDotRemoved(onShrinkCompleted));
    }

    /// <summary>
    /// Start the coroutine that increases the scale and fades in & out the dot's selected image
    /// </summary>
    public void Highlight()
    {
        StopAllCoroutines();
        StartCoroutine(OnDotSelected());
    }
    
    /// <summary>
    /// Checks if this dot is around another dot with the same color
    /// </summary>
    /// <param name="dot">The dot that we will get the neighbors from</param>
    public bool IsAroundSameColoredDot(Dot dot)
    {
        List<Dot> surroundingDots = DotsBoard.Instance.GetSameColoredDotsAround(dot);
        return surroundingDots.Contains(this);
    }

    /// <summary>
    /// Checks if a dot is at a column and row
    /// </summary>
    /// <param name="col">The column being checked</param>
    /// <param name="row">The row begin checked</param>
    /// <returns></returns>
    public bool IsAt(int col, int row)
    {
        return Col == col && Row == row;
    }
    
    /// <summary>
    /// Animates the dot going from the current position to end position at a speed
    /// </summary>
    /// <param name="endPosition">The position the dot is going to</param>
    /// <param name="moveSpeed">How fast the movement will be</param>
    /// <returns>The dot will be at the end position with its row and column updated at that position</returns>
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
    
    /// <summary>
    /// Animates the dot's selected image scaling down to 0 and fading out
    /// </summary>
    /// <returns>The dot's selected image will finish animating</returns>
    private IEnumerator OnDotSelected()
    {
        Color dotSelectedStartColor = new Color(Color.r, Color.g, Color.b, 1.0f);
        Vector2 dotSelectedStartScale = Vector2.one;

        // Calculate the speed of the scaling animation by dividing the size of the collider and dot's image
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

    /// <summary>
    /// Animates the scale decreasing to 0
    /// </summary>
    /// <param name="onShrinkCompleted">The function that will execute once the dot stops shrinking</param>
    /// <returns>The Dot will be shrunk & onShrinkCompleted function will be called</returns>
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
    
    /// <summary>
    /// Sets the dot's image & selected image's color to a random color from the color palette
    /// </summary>
    private void SetRandomColor()
    {
        _color = DotsGenerator.Instance.ColorPalette.GetRandomColor();
        
        _dotImage.color = Color;
        _dotSelectedImage.color = new Color(Color.r, Color.g, Color.b, 0.0f);
    }
}
