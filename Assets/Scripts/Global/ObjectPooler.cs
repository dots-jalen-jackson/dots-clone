using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ObjectPooler : MonoBehaviour
{
#pragma warning disable 0649
    /// <summary>
    /// Stores the object that we are spawning in the pool
    /// </summary>
    [SerializeField]
    private GameObject _objectPrefab;
    
    public GameObject ObjectPrefab => _objectPrefab;

    /// <summary>
    /// Number of objects in the pool
    /// </summary>
    private int _objectPoolSize;

    public int ObjectPoolSize
    {
        get => _objectPoolSize;
        set => _objectPoolSize = value;
    }
    
#pragma warning restore 0649

    /// <summary>
    /// Stores the objects in the pooler that aren't being used in the game
    /// </summary>
    private List<GameObject> _freeList;
    
    /// <summary>
    /// Stores the objects in the pooler that are being used in the game
    /// </summary>
    private List<GameObject> _usedList;
    
    void Awake()
    {
        _freeList = new List<GameObject>();
        _usedList = new List<GameObject>();
    }
    
    /// <summary>
    /// Generates new game object w/ objectPrefab then adds it to the object pool
    /// </summary>
    void InstantiateNewObject()
    {
        GameObject obj = Instantiate<GameObject>(_objectPrefab, transform);
        obj.SetActive(false);
        _freeList.Add(obj);
    }
    
    /// <summary>
    /// Generates multiple objects based on the number of objects in the pool
    /// </summary>
    public void GenerateObjects()
    {
        for (int i = 0; i < ObjectPoolSize; i++)
        {
            InstantiateNewObject();
        }
    }

    /// <summary>
    /// Gets object from the object pool
    /// </summary>
    public GameObject GetPooledObject()
    {
        if (_freeList.Count == 0)
            return null;
        
        // Activate the last object in the pool
        GameObject obj = _freeList[_freeList.Count - 1];
        obj.SetActive(true);
        
        // Move it to the used list & take it out of the free list
        _freeList.RemoveAt(_freeList.Count - 1);
        _usedList.Add(obj);

        return obj;
    }

    /// <summary>
    /// Returns object to the object pool with default position and rotation
    /// </summary>
    public void ReturnPooledObject(GameObject obj)
    {
        Debug.Assert(_usedList.Contains(obj));
        
        // Deactivate & reset the last object in the pool
        obj.SetActive(false);
        obj.transform.position = _objectPrefab.transform.position;
        obj.transform.rotation = _objectPrefab.transform.rotation;
        
        // Move it to the free list & take it out of the used list
        _usedList.Remove(obj);
        _freeList.Add(obj);
    }

}