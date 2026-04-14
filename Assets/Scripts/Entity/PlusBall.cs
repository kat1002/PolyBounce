using UnityEngine;

public class PlusBall : BaseObstacle, ISpecial
{
    [SerializeField] private FloatingText _floatingTextPrefab;
    [SerializeField] private int _pointValue = 100;

    public int PointValue => _pointValue;

    public void Init(Vector3 position)
    {
        PlaySpawnAnimation(position);
    }

    // ISpecial
    public void TriggerEffect()
    {
        GameManager.Instance.AddBall(1);
        SoundManager.Instance?.PlayPlusBallCollected();

        if (_floatingTextPrefab != null)
        {
            FloatingText ft = Instantiate(_floatingTextPrefab);
            ft.Play("+1", transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            TriggerEffect();
            Release();
        }
    }

    protected override void OnReachedDeadLine()
    {
        Release(); // no game over — just disappear
    }

    protected override void Release() => ReleaseShared();
}
