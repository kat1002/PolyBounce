using DG.Tweening;
using UnityEngine;

// Special obstacle with a trigger collider (ball passes through).
// Shoots along its local X axis (transform.right / -transform.right).
// At 0° rotation  → horizontal ray (left & right).
// At 90° rotation → vertical ray   (up & down).
// The spawner controls the rotation; everything else is direction-agnostic.
public class ShooterObstacle : BaseObstacle, ISpecial
{
    [SerializeField] private int _pointValue;

    [Header("Visuals")]
    [SerializeField] private Transform _arrowTransform;
    [SerializeField] private LineRenderer _line; // 3 points: rightEnd, center, leftEnd

    private bool _wasHitThisRound;
    private Tween _arrowTween;
    private Sequence _lineTween;

    public int PointValue => _pointValue;

    // ISpecial — rotation is randomized internally
    public void Init(Vector3 position) => Init(position, Random.value < 0.5f ? 0f : 90f);

    public void Init(Vector3 position, float rotationZ)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        PlaySpawnAnimation(position);
        // Store so despawn can cancel it before it fires
        _arrowTween = DOVirtual.DelayedCall(0.4f, StartArrowBounce);
    }

    private void StartArrowBounce()
    {
        if (_arrowTransform == null) return;
        _arrowTween?.Kill();
        _arrowTransform.localScale = Vector3.one;
        _arrowTween = _arrowTransform
            .DOScale(Vector3.one * 1.15f, 0.7f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // ISpecial
    public void TriggerEffect()
    {
        _wasHitThisRound = true; // mark for end-of-round destroy, regardless of hit count

        _arrowTween?.Kill();
        if (_arrowTransform != null)
        {
            _arrowTransform.DOKill();
            _arrowTransform.localScale = Vector3.one;
            _arrowTween = _arrowTransform.DOPunchScale(Vector3.one * 0.6f, 0.3f, 6, 0.4f);
        }

        SoundManager.Instance?.PlayShooterFired();
        FireRay();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
            TriggerEffect();
    }

    private void FireRay()
    {
        Vector2 origin   = transform.position;
        Vector2 rightDir = transform.right;
        Vector2 leftDir  = -transform.right;

        float distRight = DamageObstaclesAlongRay(origin, rightDir);
        float distLeft  = DamageObstaclesAlongRay(origin, leftDir);
        AnimateRayLine(origin, rightDir, distRight, leftDir, distLeft);
    }

    // Deals 1 damage to every Obstacle along the ray until a non-obstacle is hit.
    // Returns the distance to that boundary so the LineRenderer knows where to stop.
    private float DamageObstaclesAlongRay(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        float boundary = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            if (hit.collider.CompareTag("Ball")) continue; // pass through balls

            if (!hit.collider.CompareTag("Obstacle"))
            {
                boundary = hit.distance;
                break;
            }

            hit.collider.GetComponent<Obstacle>()?.ReceiveDamage(1);
        }
        return boundary;
    }

    // Single LineRenderer with 3 points: rightEnd → center → leftEnd.
    // Both arms grow outward from center simultaneously, then fade together.
    private void AnimateRayLine(Vector2 origin, Vector2 rightDir, float distRight,
                                               Vector2 leftDir,  float distLeft)
    {
        if (_line == null) return;

        // Cap infinity to a safe visual distance (scene is bounded by walls anyway)
        distRight = Mathf.Min(distRight, 50f);
        distLeft  = Mathf.Min(distLeft,  50f);

        Vector3 center   = origin;
        Vector3 rightEnd = (Vector2)origin + rightDir * distRight;
        Vector3 leftEnd  = (Vector2)origin + leftDir  * distLeft;

        _line.positionCount = 3;
        _line.SetPosition(0, center);
        _line.SetPosition(1, center);
        _line.SetPosition(2, center);
        SetLineAlpha(1f);
        _line.enabled = true;

        // Kill any in-progress ray animation before starting a new one
        _lineTween?.Kill();
        _lineTween = DOTween.Sequence();
        _lineTween.Append(
            DOVirtual.Float(0f, 1f, 0.15f, t =>
            {
                _line.SetPosition(0, Vector3.Lerp(center, rightEnd, t));
                _line.SetPosition(2, Vector3.Lerp(center, leftEnd,  t));
            }).SetEase(Ease.OutQuad)
        );
        _lineTween.AppendInterval(0.08f);
        _lineTween.Append(DOVirtual.Float(1f, 0f, 0.35f, a => SetLineAlpha(a)));
        _lineTween.OnComplete(() => _line.enabled = false);
    }

    private void SetLineAlpha(float alpha)
    {
        Color s = _line.startColor;
        Color e = _line.endColor;
        _line.startColor = new Color(s.r, s.g, s.b, alpha);
        _line.endColor   = new Color(e.r, e.g, e.b, alpha);
    }

    protected override void OnRoundStart(int round)
    {
        if (_wasHitThisRound)
        {
            _wasHitThisRound = false;
            _arrowTween?.Kill();
            _lineTween?.Kill();
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Release());
        }
        else
        {
            _arrowTween?.Kill();
            if (_arrowTransform != null)
                _arrowTransform.localScale = Vector3.one;
            base.OnRoundStart(round);
            _arrowTween = DOVirtual.DelayedCall(0.55f, StartArrowBounce);
        }
    }

    protected override void OnReachedDeadLine() =>
        GameManager.Instance.SetState(GameState.GameOver);

    protected override void Release() => ReleaseShared();

    public override void OnSpawn()
    {
        base.OnSpawn();
        _wasHitThisRound = false;
        if (_line != null) _line.enabled = false;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        _arrowTween?.Kill();
        _arrowTween = null;
        _lineTween?.Kill();
        _lineTween = null;
        _wasHitThisRound = false;
        if (_line != null) _line.enabled = false;
        if (_arrowTransform != null)
            _arrowTransform.localScale = Vector3.one;
    }
}
