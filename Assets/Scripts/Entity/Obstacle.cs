using DG.Tweening;
using TMPro;
using UnityEngine;

public class Obstacle : BaseObstacle
{
    [SerializeField] private int _health;
    [SerializeField] private int _point;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private ObjectPool<Obstacle> _pool;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Gradient _healthGradient;
    [SerializeField] private float _hardHealthThreshold = 100f;

    private MaterialPropertyBlock _mpb;
    private static readonly int HealthColorId = Shader.PropertyToID("_HealthColor");

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    public void SetPool(ObjectPool<Obstacle> pool) => _pool = pool;

    public void Init(int currentRound, Vector3 position)
    {
        _health = Random.Range(currentRound, currentRound * 3);
        _point  = _health;
        _healthText.text = _health.ToString();

        float[] rotations = { 0f, 90f, 180f, -90f };
        transform.rotation = Quaternion.Euler(0f, 0f, rotations[Random.Range(0, rotations.Length)]);
        _healthText.transform.rotation = Quaternion.identity;

        // Color is based on initial health amount — reflects difficulty, not damage taken.
        // Low health (easy) → green end of gradient. High health (hard) → red end.
        SetHealthColor(_health);
        PlaySpawnAnimation(position);
    }

    public void ReceiveDamage(int damage)
    {
        if (_pool == null) { Debug.LogError("[Obstacle] Pool not set."); return; }
        if (_health <= 0) return;

        _health -= damage;
        _healthText.text = _health.ToString();
        SetHealthColor(_health);

        if (_health <= 0)
        {
            _collider.enabled = false;
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    EventManager.InvokeObstacleDestroyed(_point);
                    Release();
                });
        }
    }

    private void SetHealthColor(int health)
    {
        if (_spriteRenderer == null || _healthGradient == null) return;
        float t = Mathf.Clamp01(health / _hardHealthThreshold);
        Color color = _healthGradient.Evaluate(t);
        _spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(HealthColorId, color);
        _spriteRenderer.SetPropertyBlock(_mpb);
    }

    protected override void OnReachedDeadLine()
    {
        GameManager.Instance.SetState(GameState.GameOver);
    }

    protected override void Release() => _pool.Release(this);

    public override void OnSpawn()
    {
        base.OnSpawn();
        transform.localScale = Vector3.zero;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        _health = 0;
        _healthText.text = "";

        // Reset to green for next use from pool
        if (_spriteRenderer != null && _healthGradient != null)
        {
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(HealthColorId, _healthGradient.Evaluate(0f));
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }
}
