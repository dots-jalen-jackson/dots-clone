using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

[DefaultExecutionOrder(-1)]
public class ObjectPooler : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private GameObject _objectPrefab;

    [SerializeField]
    private bool _isExpandable = false;

    private int _objectPoolSize;

    public int ObjectPoolSize
    {
        get => _objectPoolSize;
        set => _objectPoolSize = value;
    }
    
#pragma warning restore 0649

    private List<GameObject> _freeList;
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
        {
            if (!_isExpandable)
                return null;
            else
                InstantiateNewObject();
        }

        GameObject obj = _freeList[_freeList.Count - 1];
        obj.SetActive(true);
        
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
        obj.SetActive(false);
        obj.transform.position = _objectPrefab.transform.position;
        obj.transform.rotation = _objectPrefab.transform.rotation;

        _usedList.Remove(obj);
        _freeList.Add(obj);
    }

}