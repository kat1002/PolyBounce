using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Awake()
    {
        _settingsPanel.SetActive(false);
    }

    private void Start()
    {
        // Register in code — more reliable than inspector onValueChanged connections
        if (_bgmSlider != null) _bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    private void OnDestroy()
    {
        if (_bgmSlider != null) _bgmSlider.onValueChanged.RemoveListener(OnBGMSliderChanged);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    public void Open()
    {
        SyncSliders();
        _settingsPanel.SetActive(true);
        GameManager.Instance?.Pause();
    }

    public void Close()
    {
        _settingsPanel.SetActive(false);
        GameManager.Instance?.Resume();
    }

    public void Toggle()
    {
        if (_settingsPanel.activeSelf) Close();
        else Open();
    }

    private void SyncSliders()
    {
        if (SoundManager.Instance == null) return;
        // SetValueWithoutNotify so syncing doesn't trigger onValueChanged callbacks
        if (_bgmSlider != null) _bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BGMVolume);
        if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SFXVolume);
    }

    private void OnBGMSliderChanged(float value) => SoundManager.Instance?.SetBGMVolume(value);
    private void OnSFXSliderChanged(float value) => SoundManager.Instance?.SetSFXVolume(value);
}
