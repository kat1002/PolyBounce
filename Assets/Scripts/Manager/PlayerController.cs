using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player & Aiming")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _maxLineLength = 10f;
    [SerializeField] private int _maxBounces = 2;
    [SerializeField] private float _ballRadius = 0.2f;

    [Header("Ball")]
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private Transform _ballParent;
    private ObjectPool<Ball> _ballPool;

    private InputAction _clickAction;
    private bool _isAiming;

    private void Awake()
    {
        _ballPool    = new ObjectPool<Ball>(_ballPrefab, 10, _ballParent);
        _clickAction = _inputActions.FindActionMap("Player").FindAction("Click");
    }

    private void OnEnable()
    {
        _clickAction.Enable();
        _clickAction.started  += OnAimStart;
        _clickAction.canceled += OnShoot;
        EventManager.OnFirstBallLanded += SlideToPosition;
    }

    private void OnDisable()
    {
        _clickAction.started  -= OnAimStart;
        _clickAction.canceled -= OnShoot;
        _clickAction.Disable();
        EventManager.OnFirstBallLanded -= SlideToPosition;
    }

    private void OnAimStart(InputAction.CallbackContext context)
    {
        if (IsPointerOverUI()) return;
        var state = GameManager.Instance?.CurrentState;
        if (state != GameState.StartRound) return;
        _isAiming = true;
        if (_lineRenderer != null) _lineRenderer.enabled = true;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (!_isAiming) return;
        _isAiming = false;
        if (_lineRenderer != null) _lineRenderer.enabled = false;

        if (GameManager.Instance?.CurrentState != GameState.StartRound) return;

        Vector2 shootDirection = GetPointerDirection();
        if (shootDirection.y <= 0.1f) return;

        GameManager.Instance.SetState(GameState.InRound);
        GameManager.Instance.PrepareRound();
        StartCoroutine(LaunchBalls(shootDirection));
    }

    // Checks whether the current pointer is over any UI element.
    // Handles both mouse and touch (new Input System).
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Touch
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed)
                return EventSystem.current.IsPointerOverGameObject((int)touch.touchId.ReadValue());
        }

        // Mouse
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Update()
    {
        if (_isAiming) DrawPredictedPath();
    }

    private readonly List<Vector3> _pathPoints = new List<Vector3>(16);

    private void DrawPredictedPath()
    {
        Vector2 direction = GetPointerDirection();
        if (direction.y <= 0f) { _lineRenderer.positionCount = 0; return; }

        Vector2 currentPos = _shootPoint.position;
        float remainingDist = _maxLineLength;

        _pathPoints.Clear();
        _pathPoints.Add(currentPos);

        for (int i = 0; i < _maxBounces; i++)
        {
            RaycastHit2D hit = GetFirstSolidHit(currentPos, direction, remainingDist);

            if (hit.collider != null)
            {
                _pathPoints.Add(hit.centroid);
                remainingDist -= hit.distance;
                direction = Vector2.Reflect(direction, hit.normal);
                currentPos = hit.centroid + direction * 0.01f;

                if (i == _maxBounces - 1)
                    _pathPoints.Add(currentPos + direction * remainingDist);
            }
            else
            {
                _pathPoints.Add(currentPos + direction * remainingDist);
                break;
            }
        }

        _lineRenderer.positionCount = _pathPoints.Count;
        _lineRenderer.SetPositions(_pathPoints.ToArray());
    }

    private RaycastHit2D GetFirstSolidHit(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, _ballRadius, direction, distance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Ball")) continue;
            return hit;
        }
        return default;
    }

    private IEnumerator LaunchBalls(Vector2 direction)
    {
        for (int i = 0; i < GameManager.Instance.BallCount; i++)
        {
            Ball ball = _ballPool.Get();
            ball.SetPool(_ballPool);
            ball.InitBall(_shootPoint);
            ball.Launch(direction);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SlideToPosition(float x)
    {
        Vector3 target = new Vector3(x, _playerTransform.position.y, _playerTransform.position.z);
        _playerTransform.DOMove(target, 0.3f).SetEase(Ease.OutQuad);
    }

    private Vector2 GetPointerDirection()
    {
        if (_shootPoint == null) return Vector2.up;

        Pointer pointer = Pointer.current;
        if (pointer == null) return Vector2.up;

        Vector3 screenPos = pointer.position.ReadValue();
        screenPos.z = -Camera.main.transform.position.z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        Vector2 dir = (Vector2)worldPos - (Vector2)_shootPoint.position;
        if (dir.sqrMagnitude < 0.001f) return Vector2.up;
        return dir.normalized;
    }
}
