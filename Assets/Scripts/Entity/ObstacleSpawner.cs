using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpecialConfig
{
    public BaseObstacle prefab;
    [Range(0f, 1f)] public float spawnRate = 0.25f;
}

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<Obstacle> _obstaclePrefabs;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _obstacleParent;

    [SerializeField] private int _obstaclesPerRound = 5;
    [SerializeField] private int _columns = 7;
    [SerializeField] private float _cellSize = 1f;

    [Header("Specials")]
    [SerializeField] private List<SpecialConfig> _specials;

    private List<ObjectPool<Obstacle>> _obstaclePools;
    private Dictionary<SpecialConfig, ObjectPool<BaseObstacle>> _specialPools;

    private void Awake()
    {
        _obstaclePools = new List<ObjectPool<Obstacle>>();
        foreach (var prefab in _obstaclePrefabs)
            _obstaclePools.Add(new ObjectPool<Obstacle>(prefab, 10, _obstacleParent));

        _specialPools = new Dictionary<SpecialConfig, ObjectPool<BaseObstacle>>();
        foreach (var config in _specials)
            if (config.prefab != null)
                _specialPools[config] = new ObjectPool<BaseObstacle>(config.prefab, 5, _obstacleParent);
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
            int j = UnityEngine.Random.Range(0, i + 1);
            (columns[i], columns[j]) = (columns[j], columns[i]);
        }

        float gridLeft = _spawnPoint.position.x - (_columns * _cellSize) / 2f;
        int obstacleCount = Mathf.Min(_obstaclesPerRound, _columns);

        // Spawn obstacles in the first N shuffled columns
        for (int i = 0; i < obstacleCount; i++)
        {
            float x = gridLeft + (columns[i] + 0.5f) * _cellSize;
            Vector3 spawnPos = new Vector3(x, _spawnPoint.position.y, _spawnPoint.position.z);

            int randomIndex = UnityEngine.Random.Range(0, _obstaclePools.Count);
            Obstacle obstacle = _obstaclePools[randomIndex].Get();
            obstacle.SetPool(_obstaclePools[randomIndex]);
            obstacle.Init(round, spawnPos);
        }

        // Spawn specials in remaining columns — track taken slots to avoid overlap
        HashSet<int> takenSpecialSlots = new HashSet<int>();
        foreach (var config in _specials)
        {
            if (!_specialPools.TryGetValue(config, out var pool)) continue;
            TrySpawnSpecial(config, pool, columns, obstacleCount, gridLeft, takenSpecialSlots);
        }
    }

    private void TrySpawnSpecial(SpecialConfig config, ObjectPool<BaseObstacle> pool,
        List<int> columns, int usedCount, float gridLeft, HashSet<int> takenSlots)
    {
        if (UnityEngine.Random.value > config.spawnRate) return;

        List<int> candidates = new List<int>();
        for (int i = usedCount; i < _columns; i++)
            if (!takenSlots.Contains(i)) candidates.Add(i);

        if (candidates.Count == 0) return;

        int pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        takenSlots.Add(pick);

        float x = gridLeft + (columns[pick] + 0.5f) * _cellSize;
        Vector3 spawnPos = new Vector3(x, _spawnPoint.position.y, _spawnPoint.position.z);

        BaseObstacle special = pool.Get();
        special.SetSharedPool(pool);
        ((ISpecial)special).Init(spawnPos);
    }
}
