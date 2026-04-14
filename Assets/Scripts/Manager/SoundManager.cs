using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }


    [Header("SFX Pool")]
    [SerializeField] private int _poolSize = 12;
    [SerializeField] [Range(0f, 0.3f)] private float _pitchVariation = 0.1f;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip _bgmGameplay;
    [SerializeField] private AudioClip _bgmGameOver;

    [Header("SFX Clips — Ball")]
    [SerializeField] private AudioClip _ballHitObstacle;
    [SerializeField] private AudioClip _ballHitWall;

    [Header("SFX Clips — Obstacle")]
    [SerializeField] private AudioClip _obstacleDestroyed;

    [Header("SFX Clips — Specials")]
    [SerializeField] private AudioClip _plusBallCollected;
    [SerializeField] private AudioClip _shooterFired;

    [Header("SFX Clips — Game")]
    [SerializeField] private AudioClip _roundStart;
    [SerializeField] private AudioClip _gameOver;

    // Separate AudioSource for BGM so it loops independently of the SFX pool
    private AudioSource _bgmSource;
    private AudioSource[] _sfxPool;

    private float _bgmVolume;
    private float _sfxVolume;

    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;
    public AudioClip BGMGameplay => _bgmGameplay;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM source
        _bgmSource           = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop      = true;
        _bgmSource.playOnAwake = false;

        // SFX pool
        _sfxPool = new AudioSource[_poolSize];
        for (int i = 0; i < _poolSize; i++)
        {
            _sfxPool[i]            = gameObject.AddComponent<AudioSource>();
            _sfxPool[i].playOnAwake = false;
        }

        _bgmVolume = SaveManager.LoadBGMVolume();
        _sfxVolume = SaveManager.LoadSFXVolume();
        _bgmSource.volume = _bgmVolume;
    }

    private void OnEnable()
    {
        EventManager.OnObstacleDestroyed += HandleObstacleDestroyed;
        EventManager.OnRoundStart        += HandleRoundStart;
        EventManager.OnGameStateChanged  += HandleStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnObstacleDestroyed -= HandleObstacleDestroyed;
        EventManager.OnRoundStart        -= HandleRoundStart;
        EventManager.OnGameStateChanged  -= HandleStateChanged;
    }

    private void HandleObstacleDestroyed(int points) => PlaySFX(_obstacleDestroyed);
    private void HandleRoundStart(int round)          => PlaySFX(_roundStart, 0.8f);
    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.GameOver) PlaySFX(_gameOver);
    }

    // ── BGM ───────────────────────────────────────────────────

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || _bgmSource.clip == clip) return;
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void StopBGM() => _bgmSource.Stop();

    public void SetBGMVolume(float volume)
    {
        _bgmVolume        = Mathf.Clamp01(volume);
        _bgmSource.volume = _bgmVolume;
        SaveManager.SaveBGMVolume(_bgmVolume);
    }

    // ── SFX ───────────────────────────────────────────────────

    public void PlayBallHitObstacle()   => PlaySFX(_ballHitObstacle,  1.0f, pitchRandom: true);
    public void PlayBallHitWall()       => PlaySFX(_ballHitWall,      0.6f, pitchRandom: true);
    public void PlayPlusBallCollected() => PlaySFX(_plusBallCollected, 1.0f);
    public void PlayShooterFired()      => PlaySFX(_shooterFired,      1.0f);

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        SaveManager.SaveSFXVolume(_sfxVolume);
    }

    private void PlaySFX(AudioClip clip, float volume = 1f, bool pitchRandom = false)
    {
        if (clip == null) return;
        AudioSource src = GetAvailableSource();
        src.clip   = clip;
        src.volume = volume * _sfxVolume;
        src.pitch  = pitchRandom
            ? 1f + Random.Range(-_pitchVariation, _pitchVariation)
            : 1f;
        src.Play();
    }

    // Finds a free AudioSource; steals the first one if all are busy.
    private AudioSource GetAvailableSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying) return src;
        return _sfxPool[0];
    }
}
