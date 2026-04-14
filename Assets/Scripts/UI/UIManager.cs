using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Game Text")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private TextMeshProUGUI _ballText;

    [Header("Pause")]
    [SerializeField] private GameObject _pausePanel;

    [Header("GameOver")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _highscoreText;

    private int _score;
    private int _displayedScore;
    private Tween _scoreTween;
    private Tween _scorePunch;
    private Tween _ballPunch;

    private void OnEnable()
    {
        EventManager.OnGameStateChanged += OnStateChanged;
        EventManager.OnObstacleDestroyed += OnObstacleDestroyed;
        EventManager.OnRoundStart += OnRoundStart;
        EventManager.OnBallCountChanged += OnBallCountChanged;
    }

    private void OnDisable()
    {
        EventManager.OnGameStateChanged -= OnStateChanged;
        EventManager.OnObstacleDestroyed -= OnObstacleDestroyed;
        EventManager.OnRoundStart -= OnRoundStart;
        EventManager.OnBallCountChanged -= OnBallCountChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            _scoreTween?.Kill();
            _displayedScore = _score;

            bool newRecord = SaveManager.TrySaveHighscore(_score);

            if (_finalScoreText != null)
                _finalScoreText.text = $"SCORE: {_score}";

            if (_highscoreText != null)
            {
                int best = SaveManager.LoadHighscore();
                _highscoreText.text = newRecord
                    ? $"NEW BEST:  {best}"
                    : $"BEST:  {best}";
            }

            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(true);
        }

        if (state == GameState.StartRound)
            UpdateBallText(GameManager.Instance.BallCount);
    }

    private void OnObstacleDestroyed(int points)
    {
        _score += points;

        _scoreTween?.Kill();
        int from = _displayedScore;
        _scoreTween = DOVirtual.Int(from, _score, 0.5f, value =>
        {
            _displayedScore = value;
            _scoreText.text = value.ToString();
        })
        .SetEase(Ease.OutQuad)
        .OnComplete(() => _scoreTween = null);

        _scorePunch?.Kill();
        _scoreText.transform.localScale = Vector3.one;
        _scorePunch = _scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);
    }

    private void OnRoundStart(int round)
    {
        _roundText.text = $"Round {round}";
    }

    private void OnBallCountChanged(int count)
    {
        UpdateBallText(count);
        _ballPunch?.Kill();
        _ballText.transform.localScale = Vector3.one;
        _ballPunch = _ballText.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 5, 0.5f);
    }

    private void UpdateBallText(int count)
    {
        _ballText.text = $"Ball: x{count}";
    }

}
