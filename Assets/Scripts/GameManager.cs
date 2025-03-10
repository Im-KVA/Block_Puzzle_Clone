using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private RectTransform _endPanel;
    [SerializeField] private RectTransform _startPanel;
    [SerializeField] private CanvasGroup _playPanel;
    [SerializeField] private TextMeshProUGUI _endScore;
    [SerializeField] private TextMeshProUGUI _endBestScore;
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector2 _onScreenPosition;
    [SerializeField] private Vector2 _offScreenPosition;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void OnEnable()
    {
        EventBus.Subscribe<OnGameOverEvent>(OnGameEnd);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<OnGameOverEvent>(OnGameEnd);
    }

    public void OnGameEnd(OnGameOverEvent _)
    {
        Debug.LogError("Game end");

        Invoke(nameof(ShowEndPanel), 1.5f);
    }

    public void ShowEndPanel()
    {
        if (_endPanel == null) return;

        _startPanel.gameObject.SetActive(false);
        _playPanel.gameObject.SetActive(false);

        _endScore.text = ScoreManager.Instance.TotalScore.ToString();
        _endBestScore.text = ScoreManager.Instance.BestScore.ToString();

        _endPanel.gameObject.SetActive(true);
        _endPanel.anchoredPosition = _offScreenPosition;
        _endPanel.DOAnchorPos(_onScreenPosition, _animationDuration).SetEase(Ease.OutBack);
    }

    public void ShowPlayPanel()
    {
        ScoreManager.Instance.ResetScore();

        if (_playPanel == null) return;

        EventBus.Publish(new OnGameStartEvent { });

        _startPanel.gameObject.SetActive(false);
        _endPanel.gameObject.SetActive(false);

        _playPanel.gameObject.SetActive(true);

        _playPanel.alpha = 0;
        _playPanel.DOFade(1f, _fadeDuration).SetEase(Ease.Linear);
    }
}