using UnityEngine;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _moveUpDistance = 0.3f;
    [SerializeField] private float _duration = 1.5f;
    [SerializeField] private float _rotationAngle = 5f;
    [SerializeField] private float _scalePeak = 1.05f;

    private Vector3 _initialScale;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialScale = transform.localScale;
    }

    public void PlayFloatingAnimation(System.Action onComplete = null)
    {
        transform.localScale = Vector3.zero;
        float randomRotation = Random.Range(-_rotationAngle, _rotationAngle);
        transform.rotation = Quaternion.Euler(0, 0, randomRotation);
        Color c = _spriteRenderer.color;
        c.a = 1f;
        _spriteRenderer.color = c;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(_initialScale * _scalePeak, _duration * 0.4f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(_initialScale, _duration * 0.4f).SetEase(Ease.InQuad));
        seq.Join(transform.DOMoveY(transform.position.y + _moveUpDistance, _duration).SetEase(Ease.OutQuad));
        seq.Join(_spriteRenderer.DOFade(0f, _duration).SetEase(Ease.Linear));
        seq.OnComplete(() => { onComplete?.Invoke(); });
    }
}
