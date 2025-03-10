using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Config grid")]
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private Vector2 _gridOrigin = Vector2.zero;

    [Header("Prefab & Pooling")]
    [SerializeField] private GameObject _blockPrefab;
    private GameObject[,] _gridBlocks;
    private readonly int _gridSize = 8;
    public int GridSize() => _gridSize;

    private void Awake()
    {
        _gridBlocks = new GameObject[_gridSize, _gridSize];
        SpawnGridBlocks();
    }

    private void SpawnGridBlocks()
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                Vector2 worldPos = new Vector2(_gridOrigin.x + x * _cellSize, _gridOrigin.y + y * _cellSize);
                GameObject block = Instantiate(_blockPrefab, worldPos, Quaternion.identity, transform);

                block.SetActive(false);
                _gridBlocks[x, y] = block;
            }
        }
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - _gridOrigin.x) / _cellSize);
        int y = Mathf.RoundToInt((worldPos.y - _gridOrigin.y) / _cellSize);
        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        float x = _gridOrigin.x + gridPos.x * _cellSize;
        float y = _gridOrigin.y + gridPos.y * _cellSize;
        return new Vector2(x, y);
    }

    public GameObject GetBlockAtGridPosition(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < _gridSize && gridPos.y >= 0 && gridPos.y < _gridSize)
        {
            return _gridBlocks[gridPos.x, gridPos.y];
        }

        return null;
    }

    public bool IsCellEmpty(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            return false;
        }

        GameObject gridBlock = GetBlockAtGridPosition(gridPos);
        if (gridBlock != null)
        {
            BlockGrid bg = gridBlock.GetComponent<BlockGrid>();
            if (bg != null)
            {
                return !bg.isPlaced;
            }
        }
        return false;
    }

    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize && pos.y >= 0 && pos.y < _gridSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                Vector2 worldPos = GridToWorld(new Vector2Int(x, y));
                Gizmos.DrawWireCube(worldPos, new Vector3(_cellSize, _cellSize, 0));
            }
        }
    }
}
