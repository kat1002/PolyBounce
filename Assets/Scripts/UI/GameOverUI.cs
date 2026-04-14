using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _toMainMenuButton;

    private void Awake()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _toMainMenuButton.onClick.AddListener(OnToMainMenuClicked);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _toMainMenuButton.onClick.RemoveListener(OnToMainMenuClicked);
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("Game");
    }

    private void OnToMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("MainMenu");
    }
}
