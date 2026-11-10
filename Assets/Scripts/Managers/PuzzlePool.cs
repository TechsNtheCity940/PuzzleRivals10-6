using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for puzzle overlays. Keeps UI instances under the correct canvas
/// before activation and prevents duplicate returns from corrupting a pool queue.
/// </summary>
public class PuzzlePool : MonoBehaviour
{
    public static PuzzlePool Instance { get; private set; }

    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, string> _origin = new();
    private readonly HashSet<GameObject> _pooled = new();

    public static PuzzlePool EnsureInstance()
    {
        if (Instance != null) return Instance;
        var poolObject = new GameObject("PuzzlePool");
        return poolObject.AddComponent<PuzzlePool>();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Get a pooled instance or instantiate a new one.</summary>
    public GameObject Get(string key, GameObject prefab, Transform parent = null)
    {
        if (string.IsNullOrWhiteSpace(key) || prefab == null)
        {
            Debug.LogError("PuzzlePool.Get requires a valid key and prefab.");
            return null;
        }

        if (!_pools.TryGetValue(key, out var pool))
            pool = _pools[key] = new Queue<GameObject>();

        GameObject go = null;
        while (pool.Count > 0 && go == null)
        {
            go = pool.Dequeue();
            if (go != null) _pooled.Remove(go);
        }

        if (go == null)
            go = Instantiate(prefab, parent);

        // Reparent before activation so OnEnable/layout callbacks see the real UI
        // hierarchy, not the hidden pool root.
        if (parent != null) go.transform.SetParent(parent, false);
        _origin[go] = key;
        go.SetActive(true);
        return go;
    }

    /// <summary>Return an instance to its originating pool.</summary>
    public void Return(GameObject go)
    {
        if (go == null || _pooled.Contains(go)) return;

        if (!_origin.TryGetValue(go, out var key))
        {
            Destroy(go);
            return;
        }

        if (!_pools.TryGetValue(key, out var pool))
            pool = _pools[key] = new Queue<GameObject>();

        go.SetActive(false);
        go.transform.SetParent(transform, false);
        _pooled.Add(go);
        pool.Enqueue(go);
    }

    /// <summary>Pre-warm a pool with inactive instances.</summary>
    public void Warm(string key, GameObject prefab, int count)
    {
        if (string.IsNullOrWhiteSpace(key) || prefab == null || count <= 0) return;
        if (!_pools.TryGetValue(key, out var pool))
            pool = _pools[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            _origin[go] = key;
            _pooled.Add(go);
            pool.Enqueue(go);
        }
    }

    /// <summary>Destroy all inactive pooled instances and clear bookkeeping.</summary>
    public void Clear()
    {
        foreach (var pool in _pools.Values)
        {
            while (pool.Count > 0)
            {
                var go = pool.Dequeue();
                if (go != null) Destroy(go);
            }
        }

        _pools.Clear();
        _origin.Clear();
        _pooled.Clear();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
