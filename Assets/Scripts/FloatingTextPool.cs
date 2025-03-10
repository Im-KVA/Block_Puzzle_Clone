using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    [Header("Floating Text Prefabs")]
    [SerializeField] private GameObject[] _floatingTextPrefabs;

    private readonly Dictionary<int, Queue<GameObject>> _pool = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        for (int i = 0; i < _floatingTextPrefabs.Length; i++)
        {
            _pool[i] = new Queue<GameObject>();
            for (int j = 0; j < 3; j++)
            {
                GameObject go = Instantiate(_floatingTextPrefabs[i], transform);
                go.SetActive(false);
                _pool[i].Enqueue(go);
            }
        }
    }

    public GameObject GetFloatingText(int typeIndex)
    {
        if (_pool.ContainsKey(typeIndex) && _pool[typeIndex].Count > 0)
        {
            GameObject go = _pool[typeIndex].Dequeue();
            go.SetActive(true);
            return go;
        }
        else
        {
            GameObject go = Instantiate(_floatingTextPrefabs[typeIndex], transform);
            return go;
        }
    }

    public void ReturnFloatingText(int typeIndex, GameObject floatingText)
    {
        floatingText.SetActive(false);
        if (!_pool.ContainsKey(typeIndex))
        {
            _pool[typeIndex] = new Queue<GameObject>();
        }
        _pool[typeIndex].Enqueue(floatingText);
    }
}
