using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _bestScoreText;

    private int _totalScore = 0;
    private int _bestScore = 0;

    private const string BEST_SCORE_KEY = "BestScore";

    public int TotalScore => _totalScore;
    public int BestScore => _bestScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        UpdateBestScoreUI();
    }

    public void AddScore(int scoreToAdd)
    {
        _totalScore += scoreToAdd;
        UpdateScoreUI();
        CheckAndSaveBestScore();
    }

    private void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = _totalScore.ToString();
            _scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f);
        }
    }

    private void UpdateBestScoreUI()
    {
        if (_bestScoreText != null)
        {
            _bestScoreText.text = _bestScore.ToString();
        }
    }

    private void CheckAndSaveBestScore()
    {
        if (_totalScore > _bestScore)
        {
            _bestScore = _totalScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
            PlayerPrefs.Save();
            UpdateBestScoreUI();
        }
    }

    public void ResetScore()
    {
        _totalScore = 0;
        UpdateScoreUI();
    }
}