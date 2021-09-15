using UnityEngine;

[DefaultExecutionOrder(0)]
public class DotsGenerator : Singleton<DotsGenerator>
{
    /// <summary>
    /// Set what colors the dot will be in the inspector
    /// </summary>
    [SerializeField] 
    private DotsColorPalette _dotsColorPalette;
    
    public DotsColorPalette ColorPalette => _dotsColorPalette;
    
    /// <summary>
    /// The pool the dots will spawn from
    /// </summary>
    private ObjectPooler _dotsPooler;

    /// <summary>
    /// The size of the dot's image
    /// </summary>
    private int _dotSize;

    public int DotSize => _dotSize;
    
    /// <summary>
    /// The number of dots in the object pool
    /// </summary>
    public int NumDots => _dotsPooler.ObjectPoolSize;

    /// <summary>
    /// Generate & compute the size of the dots
    /// </summary>
    public void Start()
    {
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardWidth = DotsBoard.Instance.BoardWidth;
        
        _dotsPooler = GetComponent<ObjectPooler>();
        _dotsPooler.ObjectPoolSize = boardWidth * boardHeight;
        _dotsPooler.GenerateObjects();

        GameObject dotObject = _dotsPooler.ObjectPrefab;
        Dot dot = dotObject.GetComponent<Dot>();
        _dotSize = (int) dot.Size;
    }
    
    /// <summary>
    /// Creates a new dot at the start position
    /// </summary>
    /// <param name="col">The column the dot will be on in the board</param>
    /// <param name="row">The row the dot will be on in the board</param>
    /// <param name="startPosition">The position the dot will start on</param>
    /// <returns>A new dot on the board at col & row at the start position</returns>
    public Dot InitializeDotAtPosition(int col, int row, Vector2 startPosition)
    {
        Dot dot = CreateDot(col, row);
        if (dot == null)
            return null;
        
        dot.Position = startPosition;
        dot.Col = DotsBoard.Instance.GetColAtPosition(startPosition);
        dot.Row = DotsBoard.Instance.GetRowAtPosition(startPosition);;

        return dot;
    }

    /// <summary>
    /// Brings the dot at the column and row back into the object pool
    /// </summary>
    /// <param name="col">The column the dot is on in the board</param>
    /// <param name="row">The row the dot is on in the board</param>
    public void ReturnDotToPool(int col, int row)
    {
        Dot dot = DotsBoard.Instance.GetDotAt(col, row);
        if (dot == null)
            return;
        
        _dotsPooler.ReturnPooledObject(dot.gameObject);
        DotsBoard.Instance.PlaceDotAt(col, row, null);
    }

    /// <summary>
    /// Gets the dot out of the pool and places it in the board at col and row
    /// </summary>
    /// <param name="col">The column the dot will be on in the board</param>
    /// <param name="row">The row the dot will be on in the board</param>
    /// <returns>A new dot on the board at col & row</returns>
    private Dot CreateDot(int col, int row)
    {
        int boardHeight = DotsBoard.Instance.BoardHeight;
        int boardWidth = DotsBoard.Instance.BoardWidth;
        
        if (row < 0 || row >= boardHeight || col < 0 || col >= boardWidth)
            return null;

        GameObject dotObject = _dotsPooler.GetPooledObject();
        if (dotObject == null)
            return null;

        Dot dot = dotObject.GetComponent<Dot>();
        DotsBoard.Instance.PlaceDotAt(col, row, dot);

        return DotsBoard.Instance.GetDotAt(col, row);
    }
}
