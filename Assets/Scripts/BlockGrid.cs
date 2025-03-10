using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;

public class BlockGrid : MonoBehaviour
{
    [Header("Shadow Sprites")]
    [SerializeField] private Sprite[] _shadowSprites;

    [Header("VFX")]
    [SerializeField] private GameObject _explosionVFXPrefab;
    [SerializeField] private ParticleSystem _previewVFX;

    [Header("Flag")]
    public bool isPlaced = false;
    public bool isPreviewClear = false;
    public bool isPreview = false;

    private SpriteRenderer _spriteRenderer;
    private Sprite _placedSprite;
    private int _selectedSpriteIndex;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnGameOverEvent>(ResetBlockOnEnd);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnGameOverEvent>(ResetBlockOnEnd);
        DOTween.Kill(this);
    }

    public void PreviewUpdateSprite(Sprite previewSprite)
    {
        _spriteRenderer.sprite = previewSprite;

        if (isPreview) return;
        _previewVFX.Play();
        isPreview = true;
    }


    public void ResetPreviewSprite()
    {
        if (isPlaced)
        {
            _spriteRenderer.sprite = _placedSprite;
            _spriteRenderer.color = Color.white;

            if (!isPreview) return;
            _previewVFX.Stop();
            isPreview = false;
        }
        else
        {
            if (_shadowSprites.Length > 0)
            {
                _spriteRenderer.sprite = _shadowSprites[0];
            }
            _spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }

    public void TriggerClearEffect(float delay = 0f, Sprite[] explosionSet = null)
    {
        DOVirtual.DelayedCall(delay, () => {
            if (explosionSet != null)
            {
                GameObject explosionObj = Instantiate(_explosionVFXPrefab, transform.position, Quaternion.identity);
                if (explosionObj.TryGetComponent<ExplosionVFX>(out var explosionVFX))
                {
                    explosionVFX.Setup(explosionSet);
                }
                if (explosionObj.TryGetComponent<ParticleSystem>(out var ps))
                {
                    Destroy(explosionObj, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(transform.localScale * 1.2f, 0.2f));
            seq.Append(transform.DOScale(0f, 0.2f));
            seq.OnComplete(() => { ResetBlock(); });
        });
    }

    public void SetShadow(int spriteIndex, float alpha)
    {
        if (spriteIndex >= 0 && spriteIndex < _shadowSprites.Length)
        {
            _spriteRenderer.sprite = _shadowSprites[spriteIndex];
        }
        _spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, alpha);
    }

    public void PlaceBlock(Sprite blockSprite, int selectedSprite)
    {
        isPlaced = true;
        _placedSprite = blockSprite;
        _selectedSpriteIndex = selectedSprite;
        _spriteRenderer.sprite = blockSprite;
        _spriteRenderer.color = Color.white;
        _spriteRenderer.DOFade(1f, 0.2f);
    }

    private void ResetBlockOnEnd(OnGameOverEvent _)
    {
        ExplosionVFXManager explosionMgr  = FindObjectOfType<ExplosionVFXManager>();
        Sprite[] explosionSet = explosionMgr.GetExplosionSpritesForBlock(_selectedSpriteIndex);

        DOVirtual.DelayedCall(0.5f, () => {
            if (explosionSet != null)
            {
                GameObject explosionObj = Instantiate(_explosionVFXPrefab, transform.position, Quaternion.identity);
                if (explosionObj.TryGetComponent<ExplosionVFX>(out var explosionVFX))
                {
                    explosionVFX.Setup(explosionSet);
                }
                if (explosionObj.TryGetComponent<ParticleSystem>(out var ps))
                {
                    Destroy(explosionObj, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
            Sequence seq = DOTween.Sequence();
            seq.OnComplete(() => { ResetBlock(); });
        });
    }

    public void ResetBlock()
    {
        isPlaced = false;
        transform.localScale = Vector3.one * 0.5f;
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
        gameObject.SetActive(false);
    }

    public void ClearLogic()
    {
        isPlaced = false;
    }
}
