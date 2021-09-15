using UnityEngine;

/// <summary>
/// Make sures that there is only one instance of the component T
/// </summary>
/// <typeparam name="T">The component that we went to only have one instance of in game</typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    /// <summary>
    /// Instantiate a game object with component T if it doesn't exists
    /// Get the only instance of component T if it does exists
    /// </summary>
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Destroys the only instance of component T
    /// </summary>
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}