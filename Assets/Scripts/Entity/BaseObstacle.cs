using DG.Tweening;
using UnityEngine;

// Shared foundation for all obstacle types.
// Handles: event wiring, round movement, spawn animation, collider reset.
// Subclasses decide what happens when the deadline is reached and how to release.
public abstract class BaseObstacle : MonoBehaviour, IPoolable
{
    [SerializeField] protected Collider2D _collider;

    private ObjectPool<BaseObstacle> _sharedPool;
    public void SetSharedPool(ObjectPool<BaseObstacle> pool) => _sharedPool = pool;
    protected void ReleaseShared() => _sharedPool?.Release(this);

    protected virtual void OnEnable()
    {
        EventManager.OnRoundStart += HandleRoundStart;
    }

    protected virtual void OnDisable()
    {
        EventManager.OnRoundStart -= HandleRoundStart;
    }

    // Call from subclass Init after setting up type-specific data
    protected void PlaySpawnAnimation(Vector3 finalPosition)
    {
        transform.position = finalPosition + Vector3.up * 1f;
        transform.localScale = Vector3.zero;
        transform.DOMove(finalPosition, 0.35f).SetEase(Ease.OutQuad);
        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    // Trampoline so subclasses can override round-start behavior via virtual dispatch
    private void HandleRoundStart(int round) => OnRoundStart(round);

    protected virtual void OnRoundStart(int round)
    {
        transform.DOKill();
        transform.DOMoveY(transform.position.y - 1f, 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(CheckDeadLine);
    }


    private void CheckDeadLine()
    {
        if (transform.position.y <= GameManager.Instance.DeadLine.position.y)
            OnReachedDeadLine();
    }

    // Each subclass decides: trigger game over, silently release, etc.
    protected abstract void OnReachedDeadLine();

    // Each subclass calls its typed pool
    protected abstract void Release();

    public virtual void OnSpawn()
    {
        _collider.enabled = true;
        transform.localScale = Vector3.zero;
    }

    public virtual void OnDespawn()
    {
        transform.DOKill();
        _collider.enabled = false;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }
}
