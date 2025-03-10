using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Block : MonoBehaviour
{
    [Header("Config Block")]
    [SerializeField] private Transform[] _subBlocks;
    [SerializeField] private Sprite[] _blockSprites;

    [Header("Other Settings - Debug only")]
    [SerializeField] private int _selectedSpriteIndex;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridClearManager _gridClearManager;
    [SerializeField] private BlockManager _blockManager;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Vector3 _initialPosition;
    [SerializeField] private Vector3 _initialScale;
    [SerializeField] private Vector3 _subInitialScales;
    [SerializeField] private bool _isDragging = false;
    [SerializeField] private bool _isAllowToDrag = true;
    [SerializeField] private bool _isSnappedToGrid = false;
    public bool IsAllowToDrag() => _isAllowToDrag;
    [SerializeField] private List<Vector2Int> _currentShadowPositions = new();

    void OnEnable() => EventBus.Subscribe<OnGridUpdatedEvent>(CheckSelfStateEvent);
    void OnDestroy()
    {
        EventBus.Unsubscribe<OnGridUpdatedEvent>(CheckSelfStateEvent);
        DOTween.Kill(this);
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
        _gridManager = FindObjectOfType<GridManager>();
        _gridClearManager = FindObjectOfType<GridClearManager>();
        _blockManager = FindObjectOfType<BlockManager>();

        if (_blockSprites != null && _blockSprites.Length > 0)
        {
            _selectedSpriteIndex = Random.Range(0, _blockSprites.Length);
            _selectedSprite = _blockSprites[_selectedSpriteIndex];
        }

        foreach (Transform sub in _subBlocks)
        {
            if (sub.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.sprite = _selectedSprite;
            }

            _subInitialScales = sub.localScale;
        }
    }

    private void OnMouseDown()
    {
        if (!_isAllowToDrag) return;

        _isDragging = true;
        _initialPosition = transform.position;
        _initialScale = transform.localScale;

        transform.localScale = Vector3.one;

        foreach (Transform sub in _subBlocks)
        {
            sub.localScale *= 0.9f;
        }
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;

        Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos + Vector3.up * 1f;

        UpdateShadow();

        GridClearManager gridClearManager = FindObjectOfType<GridClearManager>();
        if (gridClearManager != null)
        {
            gridClearManager.PreviewCheckLines(GetGridPositions(), _selectedSprite);
        }
    }

    private void OnMouseUp()
    {
        if (!_isAllowToDrag) return;
        _isDragging = false;

        if (IsPlacementValid())
        {
            SnapToGrid();
            Destroy(gameObject);
        }
        else
        {
            transform.position = _initialPosition;
            transform.localScale = _initialScale;
            foreach (Transform sub in _subBlocks)
            {
                sub.localScale = _subInitialScales;
            }
            ClearShadow();

            if (_gridClearManager != null)
            {
                _gridClearManager.PreviewCheckLines(GetGridPositions(), _selectedSprite);
                _gridClearManager.ResetPreviewLines();
            }
        }
    }

    private Vector2Int[] GetGridPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        float factor = 0.62f / 0.6f;
        foreach (Transform sub in _subBlocks)
        {
            Vector2 adjustedOffset = (Vector2)sub.localPosition * factor;
            Vector2 worldPos = (Vector2)transform.position + adjustedOffset;
            Vector2Int gridPos = _gridManager.WorldToGrid(worldPos);
            positions.Add(gridPos);
        }
        return positions.ToArray();
    }

    private void UpdateShadow()
    {
        ClearShadow();

        Vector2Int[] gridPositions = GetGridPositions();
        foreach (Vector2Int gridPos in gridPositions)
        {
            GameObject gridBlock = _gridManager.GetBlockAtGridPosition(gridPos);
            if (gridBlock == null)
            {
                continue;
            }
            BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
            if (blockGrid != null && !blockGrid.isPlaced)
            {
                gridBlock.SetActive(true);
                blockGrid.SetShadow(_selectedSpriteIndex, 0.5f);
                _currentShadowPositions.Add(gridPos);
            }
        }
    }

    private void ClearShadow()
    {
        foreach (Vector2Int pos in _currentShadowPositions)
        {
            GameObject gridBlock = _gridManager.GetBlockAtGridPosition(pos);
            if (gridBlock != null)
            {
                gridBlock.SetActive(false);
            }
        }
        _currentShadowPositions.Clear();
    }

    private bool IsPlacementValid()
    {
        Vector2Int[] positions = GetGridPositions();
        foreach (Vector2Int pos in positions)
        {
            GameObject gridBlock = _gridManager.GetBlockAtGridPosition(pos);
            if (gridBlock == null)
            {
                return false;
            }

            BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
            if (blockGrid != null && blockGrid.isPlaced)
            {
                return false;
            }
        }
        return true;
    }

    private void SnapToGrid()
    {
        _currentShadowPositions.Clear();
        _blockManager.RemoveBlockFromList(this);

        _isSnappedToGrid = true;

        Vector2Int[] positions = GetGridPositions();
        foreach (Vector2Int pos in positions)
        {
            GameObject gridBlock = _gridManager.GetBlockAtGridPosition(pos);
            if (gridBlock == null)
            {
                continue;
            }
            gridBlock.SetActive(true);

            if (gridBlock.TryGetComponent<BlockGrid>(out var blockGrid))
            {
                blockGrid.PlaceBlock(_selectedSprite, _selectedSpriteIndex);
                gridBlock.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 10, 1);
            }
        }
        _gridClearManager.ClearPreviewLines(GetGridPositions(), _selectedSprite, _selectedSpriteIndex);
    }

    public bool CanPlaceAt(Vector2Int origin)
    {
        Vector2 baseWorldPos = _gridManager.GridToWorld(origin);
        float factor = 0.62f / 0.6f;
        foreach (Transform sub in _subBlocks)
        {
            Vector2 adjustedOffset = (Vector2)sub.localPosition * factor;
            Vector2 worldPos = baseWorldPos + adjustedOffset;
            Vector2Int cellPos = _gridManager.WorldToGrid(worldPos);

            if (!_gridManager.IsCellEmpty(cellPos))
            {
                return false;
            }
        }
        return true;
    }

    public void CheckSelfStateEvent(OnGridUpdatedEvent _) => CheckSelfState();

    public void CheckSelfState()
    {
        if (_isSnappedToGrid) return;

        Debug.Log("CheckSelfState");
        _isAllowToDrag = AllowToDrag();

        if (_isAllowToDrag)
        {
            foreach (Transform sub in _subBlocks)
            {
                if (sub.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.color = Color.white;
                    sr.DOFade(1f, 0.5f);
                }
            }
        }
        else
        {
            foreach (Transform sub in _subBlocks)
            {
                if (sub.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                    sr.DOFade(0.5f, 0.5f);
                }
            }
            EventBus.Publish(new OnBlockUnableToDrag { });
        }
    }

    private bool AllowToDrag()
    {
        int gridSize = _gridManager.GridSize();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int testPos = new Vector2Int(x, y);

                if (CanPlaceAt(testPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void PlaySpawnAnimationSequential()
    {
        foreach (Transform sub in _subBlocks)
        {
            sub.localScale = Vector3.zero;
            if (sub.TryGetComponent<SpriteRenderer>(out var sr))
            {
                Color c = sr.color;
                c.a = 0;
                sr.color = c;
            }
        }

        Sequence seq = DOTween.Sequence();
        float tweenDuration = 0.05f;
        float intervalDelay = 0f;

        foreach (Transform sub in _subBlocks)
        {
            seq.Append(sub.DOScale(_subInitialScales * 1.1f, tweenDuration).SetEase(Ease.OutBack));
            seq.Append(sub.DOScale(_subInitialScales, tweenDuration));

            if (sub.TryGetComponent<SpriteRenderer>(out var sr))
            {
                seq.Join(sr.DOFade(1f, tweenDuration));
            }
            seq.AppendInterval(intervalDelay);
        }
    }


    //Debug only
    private void OnDrawGizmos()
    {
        if (_gridManager == null) return;

        Vector2Int[] positions = GetGridPositions();
        Gizmos.color = Color.red;

        foreach (Vector2Int pos in positions)
        {
            Vector2 worldPos = _gridManager.GridToWorld(pos);
            Gizmos.DrawWireCube(worldPos, Vector3.one * (0.6f * 0.9f));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_gridManager == null) return;

        int gridSize = 8;

        Gizmos.color = Color.green;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (CanPlaceAt(cell))
                {
                    Vector2 worldPos = _gridManager.GridToWorld(cell);
                    Gizmos.DrawWireSphere(worldPos, 0.3f);
                }
            }
        }
    }

}
