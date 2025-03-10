using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BlockManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _spawnPositions;
    [SerializeField] private GameObject[] _blockPrefabs;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private List<Block> _currentBlocks = new();

    private bool _isStartSpawn = false;

    void OnEnable()
    {
        EventBus.Subscribe<OnGridUpdatedEvent>(CheckAllBlockStateEvent_Grid);
        EventBus.Subscribe<OnBlockUnableToDrag>(CheckAllBlockStateEvent_Block);
        EventBus.Subscribe<OnGameStartEvent>(SpawnBlocks);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<OnGridUpdatedEvent>(CheckAllBlockStateEvent_Grid);
        EventBus.Unsubscribe<OnBlockUnableToDrag>(CheckAllBlockStateEvent_Block);
        EventBus.Unsubscribe<OnGameStartEvent>(SpawnBlocks);
    }

    private void Update()
    {
        if (_currentBlocks.Count == 0 && _isStartSpawn)
        {
            _currentBlocks.Clear();
            ReSpawnBlocks();
        }
    }

    private void SpawnBlocks(OnGameStartEvent _) => ReSpawnBlocks();

    private void ReSpawnBlocks()
    {
        _isStartSpawn = true;
        foreach (Transform spawnPos in _spawnPositions)
        {
            int randomIndex = Random.Range(0, _blockPrefabs.Length);
            GameObject blockObj = Instantiate(_blockPrefabs[randomIndex], spawnPos.position, Quaternion.identity);
            blockObj.SetActive(true);

            if (blockObj.TryGetComponent<Collider2D>(out var col))
            {
                Vector2 colliderCenter = col.bounds.center;
                Vector2 pivotPos = blockObj.transform.position;
                Vector2 offset = colliderCenter - pivotPos;
                blockObj.transform.position = (Vector2)spawnPos.position - offset;
            }

            if (blockObj.TryGetComponent<Block>(out var block))
            {
                block.PlaySpawnAnimationSequential();
                DOVirtual.DelayedCall(0.5f, () => {
                    block.CheckSelfState();
                });
                _currentBlocks.Add(block);
            }
        }
    }

    private void StopSpawnBlocks()
    {
        _isStartSpawn = false;
        foreach (Block block in _currentBlocks)
        {
            Destroy(block.gameObject);
        }
        _currentBlocks.Clear();
    }

    private void CheckAllBlockStateEvent_Grid(OnGridUpdatedEvent _) => CheckAllBlockState();
    private void CheckAllBlockStateEvent_Block(OnBlockUnableToDrag _) => CheckAllBlockState();

    private void CheckAllBlockState()
    {
        if (_currentBlocks.Count == 0) return;

        foreach (Block block in _currentBlocks)
        {
            if (block.IsAllowToDrag()) return;
        }

        Invoke(nameof(StopSpawnBlocks), 1.5f);
        EventBus.Publish(new OnGameOverEvent { });
    }

    public void RemoveBlockFromList(Block block)
    {
        _currentBlocks.Remove(block);
    }
}
