using DG.Tweening;
using UnityEngine;

// Special obstacle with a trigger collider — ball passes through it.
// Redirects the ball to a random direction and awards points when hit.
// Releases at the start of the next round after being hit (same pattern as ShooterObstacle).
public class BumperObstacle : BaseObstacle, ISpecial
{
    [SerializeField] private int _pointValue = 50;

    private bool _wasHitThisRound;
    private Tween _punchTween;

    public int PointValue => _pointValue;

    public void Init(Vector3 position)
    {
        PlaySpawnAnimation(position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;

        Ball ball = other.GetComponent<Ball>();
        if (ball == null) return;

        ball.Redirect(RandomDirection());
        TriggerEffect();
    }

    // ISpecial
    public void TriggerEffect()
    {
        _wasHitThisRound = true;

        EventManager.InvokeObstacleDestroyed(_pointValue);
        SoundManager.Instance?.PlayBallHitObstacle();

        _punchTween?.Kill();
        _punchTween = transform.DOPunchScale(Vector3.one * 0.5f, 0.25f, 6, 0.4f);
    }

    protected override void OnRoundStart(int round)
    {
        if (_wasHitThisRound)
        {
            _wasHitThisRound = false;
            _punchTween?.Kill();
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Release());
        }
        else
        {
            base.OnRoundStart(round);
        }
    }

    // Random direction biased away from horizontal/vertical to avoid ball getting stuck.
    private Vector2 RandomDirection()
    {
        float quadrant   = Mathf.Floor(Random.Range(0f, 4f)) * 90f;
        float inQuad     = Mathf.Lerp(20f, 70f, Random.value);
        float finalAngle = quadrant + inQuad;
        float rad        = finalAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    protected override void OnReachedDeadLine() =>
        GameManager.Instance.SetState(GameState.GameOver);

    protected override void Release() => ReleaseShared();

    public override void OnSpawn()
    {
        base.OnSpawn();
        _wasHitThisRound = false;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        _punchTween?.Kill();
        _punchTween = null;
        _wasHitThisRound = false;
    }
}
