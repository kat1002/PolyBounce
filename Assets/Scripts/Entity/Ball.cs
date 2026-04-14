using UnityEngine;

public class Ball : MonoBehaviour, IPoolable
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _wallBounceThreshold = 15;
    private ObjectPool<Ball> _pool;
    private Vector2 _lastVelocity;
    private int _wallBounceCount;
    private bool _reflectedThisStep;
    private bool _isLaunched;
    private bool _isPulled;

    private void FixedUpdate()
    {
        _reflectedThisStep = false;

        if (!_isLaunched) return;

        // When pulled, collider is disabled so OnTriggerEnter2D won't fire.
        // Manually check when the ball crosses the end line.
        if (_isPulled)
        {
            if (transform.position.y <= GameManager.Instance.EndLine.position.y)
            {
                GameManager.Instance.OnBallReturned(transform.position.x);
                _pool.Release(this);
            }
            return;
        }

        _lastVelocity = _rigidbody2D.linearVelocity;

        // Catch wedged/stopped balls — speed dropped despite being launched
        if (_rigidbody2D.linearVelocity.sqrMagnitude < _speed * _speed * 0.1f)
            PullToEndLine();
    }

    private void PullToEndLine()
    {
        _wallBounceCount = 0;
        _isPulled = true;
        _collider.enabled = false; // phase through walls and obstacles
        Vector2 target = new Vector2(transform.position.x, GameManager.Instance.EndLine.position.y);
        _rigidbody2D.linearVelocity = (target - (Vector2)transform.position).normalized * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collObject = collision.gameObject;

        if (collObject.CompareTag("Obstacle"))
        {
            _wallBounceCount = 0;
            Obstacle obs = collObject.GetComponent<Obstacle>();
            obs.ReceiveDamage(1);
            SoundManager.Instance?.PlayBallHitObstacle();
        }
        else
        {
            _wallBounceCount++;
            SoundManager.Instance?.PlayBallHitWall();
            if (_wallBounceCount >= _wallBounceThreshold)
            {
                PullToEndLine();
                return; // skip reflection — pull direction must not be overwritten
            }
        }

        // Only reflect once per physics step — prevents two colliders in the same step
        // (e.g. triangle + square corner) from setting opposing velocities that cancel out.
        if (_reflectedThisStep) return;
        if (_lastVelocity.sqrMagnitude < 0.001f) return;

        // Average all contact normals for a stable corner reflection.
        Vector2 avgNormal = Vector2.zero;
        for (int i = 0; i < collision.contactCount; i++)
            avgNormal += collision.GetContact(i).normal;
        if (avgNormal.sqrMagnitude < 0.001f) return;

        _reflectedThisStep = true;
        Vector2 direction = Vector2.Reflect(_lastVelocity.normalized, avgNormal.normalized);
        _rigidbody2D.linearVelocity = direction * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bottom"))
        {
            GameManager.Instance.OnBallReturned(transform.position.x);
            _pool.Release(this);
        }
    }

    public void InitBall(Transform shootPosition) {
        transform.position = shootPosition.position;
    }
    public void SetPool(ObjectPool<Ball> pool)
    {
        _pool = pool;
    }

    public void Launch(Vector2 direction)
    {
        _isLaunched = true;
        _rigidbody2D.linearVelocity = direction * _speed;
    }

    // Called by specials (e.g. BumperObstacle) to override the ball's current direction.
    public void Redirect(Vector2 direction)
    {
        _wallBounceCount = 0;
        _reflectedThisStep = true;
        _rigidbody2D.linearVelocity = direction.normalized * _speed;
    }

    public void OnSpawn()
    {
        _isLaunched = false;
        _isPulled = false;
        _collider.enabled = true;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _lastVelocity = Vector2.zero;
        _wallBounceCount = 0;
        _reflectedThisStep = false;
    }

    public void OnDespawn()
    {
        _isLaunched = false;
        _isPulled = false;
        _collider.enabled = true;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _lastVelocity = Vector2.zero;
        _wallBounceCount = 0;
        _reflectedThisStep = false;
    }
}
