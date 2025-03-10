using UnityEngine;
using System.Collections.Generic;

public class GridClearManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private FloatingTextPool _floatingTextPool;
    [SerializeField] private int _gridSize = 8;
    [SerializeField] private int _baseScorePerLine = 10;
    [SerializeField] private List<int> _previewRows = new();
    [SerializeField] private List<int> _previewCols = new();

    private ExplosionVFXManager _explosionMgr;
    private HashSet<int> _lastPreviewRows = new HashSet<int>();
    private HashSet<int> _lastPreviewCols = new HashSet<int>();

    private void Awake()
    {
        _explosionMgr = FindObjectOfType<ExplosionVFXManager>();
    }

    public void PreviewCheckLines(Vector2Int[] potentialPositions, Sprite placedSprite)
    {
        HashSet<int> newRows = new HashSet<int>();
        HashSet<int> newCols = new HashSet<int>();
        foreach (Vector2Int pos in potentialPositions)
        {
            newRows.Add(pos.y);
            newCols.Add(pos.x);
        }

        if (newRows.SetEquals(_lastPreviewRows) && newCols.SetEquals(_lastPreviewCols))
        {
            return;
        }

        _lastPreviewRows = new HashSet<int>(newRows);
        _lastPreviewCols = new HashSet<int>(newCols);

        ResetPreviewLines();

        // Preview row
        foreach (int row in newRows)
        {
            bool isFull = true;
            for (int col = 0; col < _gridSize; col++)
            {
                Vector2Int cellPos = new Vector2Int(col, row);
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(cellPos);
                if (gridBlock == null)
                {
                    isFull = false;
                    break;
                }
                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                bool isFilled = (blockGrid != null && blockGrid.isPlaced) || System.Array.Exists(potentialPositions, p => p.Equals(cellPos));
                if (!isFilled)
                {
                    isFull = false;
                    break;
                }
            }
            if (!isFull)
                continue;

            _previewRows.Add(row);
            for (int col = 0; col < _gridSize; col++)
            {
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(new Vector2Int(col, row));
                if (gridBlock == null)
                    continue;
                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null)
                {
                    blockGrid.PreviewUpdateSprite(placedSprite);
                    blockGrid.isPreviewClear = true;
                }
            }
        }

        // Preview col
        foreach (int col in newCols)
        {
            bool isFull = true;
            for (int row = 0; row < _gridSize; row++)
            {
                Vector2Int cellPos = new Vector2Int(col, row);
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(cellPos);
                if (gridBlock == null)
                {
                    isFull = false;
                    break;
                }
                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                bool isFilled = (blockGrid != null && blockGrid.isPlaced) || System.Array.Exists(potentialPositions, p => p.Equals(cellPos));
                if (!isFilled)
                {
                    isFull = false;
                    break;
                }
            }
            if (!isFull)
                continue;

            _previewCols.Add(col);
            for (int row = 0; row < _gridSize; row++)
            {
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(new Vector2Int(col, row));
                if (gridBlock == null)
                    continue;
                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null)
                {
                    blockGrid.PreviewUpdateSprite(placedSprite);
                    blockGrid.isPreviewClear = true;
                }
            }
        }
    }

    public void ClearPreviewLines(Vector2Int[] potentialPositions, Sprite placedSprite, int selectedSpriteIndex)
    {
        PreviewCheckLines(potentialPositions, placedSprite);

        int linesCleared = _previewRows.Count + _previewCols.Count;
        if (linesCleared <= 0)
        {
            EventBus.Publish(new OnGridUpdatedEvent { });
            return;
        }

        Vector2 center = Vector2.zero;
        foreach (Vector2Int pos in potentialPositions)
        {
            center += new Vector2(pos.x, pos.y);
        }
        center /= potentialPositions.Length;

        // ---Update logic (clear)----------------------------------------------------------------------
        foreach (int row in _previewRows)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(new Vector2Int(col, row));
                if (gridBlock == null) continue;

                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null && blockGrid.isPreviewClear)
                {
                    blockGrid.ClearLogic();
                }
            }
        }

        foreach (int col in _previewCols)
        {
            for (int row = 0; row < _gridSize; row++)
            {
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(new Vector2Int(col, row));
                if (gridBlock == null) continue;

                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null && blockGrid.isPreviewClear)
                {
                    blockGrid.ClearLogic();
                }
            }
        }

        // ---Trigger clear effect--------------------------------------------------------------------------
        foreach (int row in _previewRows)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                Vector2Int cellPos = new Vector2Int(col, row);
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(cellPos);
                if (gridBlock == null) continue;

                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null && blockGrid.isPreviewClear)
                {
                    float delay = (Mathf.Abs(col - center.x) + Mathf.Abs(row - center.y)) * 0.05f;

                    Sprite[] explosionSet = _explosionMgr.GetExplosionSpritesForBlock(selectedSpriteIndex);
                    blockGrid.TriggerClearEffect(delay, explosionSet);
                }
            }
        }

        foreach (int col in _previewCols)
        {
            for (int row = 0; row < _gridSize; row++)
            {
                Vector2Int cellPos = new Vector2Int(col, row);
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(cellPos);
                if (gridBlock == null) continue;

                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null && blockGrid.isPreviewClear)
                {
                    float delay = (Mathf.Abs(col - center.x) + Mathf.Abs(row - center.y)) * 0.05f;

                    Sprite[] explosionSet = _explosionMgr.GetExplosionSpritesForBlock(selectedSpriteIndex);
                    blockGrid.TriggerClearEffect(delay, explosionSet);
                }
            }
        }

        //Score-------------------------------------------------------------------------------------------
        float multiplier = 1f;
        if (linesCleared == 1)
            multiplier = 3f;
        else if (linesCleared == 2)
            multiplier = 6f;
        else if (linesCleared == 3)
            multiplier = 8f;
        else if (linesCleared >= 4)
            multiplier = 10f;

        int scoreEarned = Mathf.RoundToInt(linesCleared * _baseScorePerLine * multiplier);

        ScoreManager.Instance.AddScore(scoreEarned);

        int floatingTextType = DetermineFloatingTextType(linesCleared);

        GameObject floatingTextObj = _floatingTextPool.GetFloatingText(floatingTextType);
        floatingTextObj.transform.position = _gridManager.GridToWorld(new Vector2Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y)));

        FloatingText ft = floatingTextObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.PlayFloatingAnimation(() => {
                _floatingTextPool.ReturnFloatingText(floatingTextType, floatingTextObj);
            });
        }

        Debug.Log($"Cleared {linesCleared} lines! Score earned: {scoreEarned}.");
        EventBus.Publish(new OnGridUpdatedEvent { });
    }

    public void ResetPreviewLines()
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                GameObject gridBlock = _gridManager.GetBlockAtGridPosition(new Vector2Int(x, y));
                if (gridBlock == null)
                {
                    continue;
                }
                BlockGrid blockGrid = gridBlock.GetComponent<BlockGrid>();
                if (blockGrid != null && blockGrid.isPreviewClear)
                {
                    blockGrid.ResetPreviewSprite();
                    blockGrid.isPreviewClear = false;
                }
            }
        }
        _previewRows.Clear();
        _previewCols.Clear();
    }

    private int DetermineFloatingTextType(int linesCleared)
    {
        if (linesCleared == 2)
            return 0;
        else if (linesCleared == 3)
            return 1;
        else if (linesCleared == 4)
            return 2;
        else if (linesCleared >= 5)
            return 3;

        return 0;
    }
}
