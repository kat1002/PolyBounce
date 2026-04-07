using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<Obstacle> _obstaclePrefabs;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _obstacleParent;

    [SerializeField] private int _obstaclesPerRound = 5;
    [SerializeField] private int _columns = 7;
    [SerializeField] private float _cellSize = 1f;

    [Header("PlusBall")]
    [SerializeField] private PlusBall _plusBallPrefab;
    [SerializeField] private Transform _plusBallParent;
    [SerializeField] [Range(0f, 1f)] private float _plusBallSpawnRate = 0.3f;

    [Header("ShooterObstacle")]
    [SerializeField] private ShooterObstacle _shooterObstaclePrefab;
    [SerializeField] private Transform _shooterObstacleParent;
    [SerializeField] [Range(0f, 1f)] private float _shooterObstacleSpawnRate = 0.25f;

    private List<ObjectPool<Obstacle>> _obstaclePools;
    private ObjectPool<PlusBall> _plusBallPool;
    private ObjectPool<ShooterObstacle> _shooterObstaclePool;

    private void Awake()
    {
        _obstaclePools = new List<ObjectPool<Obstacle>>();
        foreach (var prefab in _obstaclePrefabs)
            _obstaclePools.Add(new ObjectPool<Obstacle>(prefab, 10, _obstacleParent));

        if (_plusBallPrefab != null)
            _plusBallPool = new ObjectPool<PlusBall>(_plusBallPrefab, 5, _plusBallParent);

        if (_shooterObstaclePrefab != null)
            _shooterObstaclePool = new ObjectPool<ShooterObstacle>(_shooterObstaclePrefab, 5, _shooterObstacleParent);
    }

    private void OnEnable()
    {
        EventManager.OnGameStateChanged += OnStateChanged;
        EventManager.OnRoundStart += SpawnObstacle;
    }

    private void OnDisable()
    {
        EventManager.OnGameStateChanged -= OnStateChanged;
        EventManager.OnRoundStart -= SpawnObstacle;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.PostRound) StartCoroutine(DoPostRound());
    }

    private IEnumerator DoPostRound()
    {
        GameManager.Instance.HandleNewRound();
        yield return new WaitForSeconds(0.6f);
        if (GameManager.Instance.CurrentState != GameState.GameOver)
            GameManager.Instance.SetState(GameState.StartRound);
    }

    private void SpawnObstacle(int round)
    {
        if (_obstaclePools.Count == 0) return;

        // Build and shuffle column indices
        List<int> columns = new List<int>(_columns);
        for (int i = 0; i < _columns; i++) columns.Add(i);

        for (int i = columns.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (columns[i], columns[j]) = (columns[j], columns[i]);
        }

        float gridLeft = _spawnPoint.position.x - (_columns * _cellSize) / 2f;
        int obstacleCount = Mathf.Min(_obstaclesPerRound, _columns);

        // Spawn obstacles in the first N shuffled columns
        for (int i = 0; i < obstacleCount; i++)
        {
            float x = gridLeft + (columns[i] + 0.5f) * _cellSize;
            Vector3 spawnPos = new Vector3(x, _spawnPoint.position.y, _spawnPoint.position.z);

            int randomIndex = Random.Range(0, _obstaclePools.Count);
            Obstacle obstacle = _obstaclePools[randomIndex].Get();
            obstacle.SetPool(_obstaclePools[randomIndex]);
            obstacle.Init(round, spawnPos);
        }

        // Try to spawn specials in remaining columns — track taken slots to avoid overlap
        int takenSpecialColumn = TrySpawnPlusBall(columns, obstacleCount, gridLeft);
        TrySpawnShooterObstacle(columns, obstacleCount, gridLeft, takenSpecialColumn);
    }

    // Returns the shuffled-list index that was picked, or -1 if nothing spawned.
    private int TrySpawnPlusBall(List<int> columns, int usedCount, float gridLeft)
    {
        if (_plusBallPool == null) return -1;
        if (Random.value > _plusBallSpawnRate) return -1;

        int remaining = _columns - usedCount;
        if (remaining <= 0) return -1;

        int pick = Random.Range(usedCount, _columns);
        float x = gridLeft + (columns[pick] + 0.5f) * _cellSize;
        Vector3 spawnPos = new Vector3(x, _spawnPoint.position.y, _spawnPoint.position.z);

        PlusBall plusBall = _plusBallPool.Get();
        plusBall.SetPool(_plusBallPool);
        plusBall.Init(spawnPos);
        return pick;
    }

    // excludedPick: shuffled-list index already taken by another special (-1 = none).
    private void TrySpawnShooterObstacle(List<int> columns, int usedCount, float gridLeft, int excludedPick)
    {
        if (_shooterObstaclePool == null) return;
        if (Random.value > _shooterObstacleSpawnRate) return;

        // Build candidate indices from remaining columns, skipping the excluded one
        List<int> candidates = new List<int>();
        for (int i = usedCount; i < _columns; i++)
            if (i != excludedPick) candidates.Add(i);

        if (candidates.Count == 0) return;

        int pick = candidates[Random.Range(0, candidates.Count)];
        float x = gridLeft + (columns[pick] + 0.5f) * _cellSize;
        Vector3 spawnPos = new Vector3(x, _spawnPoint.position.y, _spawnPoint.position.z);

        // 0° = horizontal shooter (left/right), 90° = vertical shooter (up/down)
        float rotation = Random.value < 0.5f ? 0f : 90f;

        ShooterObstacle shooter = _shooterObstaclePool.Get();
        shooter.SetPool(_shooterObstaclePool);
        shooter.Init(spawnPos, rotation);
    }
}
