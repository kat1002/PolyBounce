using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private List<Obstacle> _obstaclePrefabs;

    [Header("Walls")]
    [SerializeField] private Transform _leftWall;
    [SerializeField] private Transform _rightWall;
    [SerializeField] private Transform _upWall;
    [SerializeField] private Transform _deadLine;
    [SerializeField] private Transform _endLine;

    [Header("Player")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _shootPoint;

    private ObjectPool<Ball> _ballObjectPool;
    private ObjectPool<Obstacle> _obstaclePool;

    public ObjectPool<Ball> BallObjectPool => _ballObjectPool;
    public ObjectPool<Obstacle> ObstaclePool => _obstaclePool;


    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitVariable();
    }

    private void InitVariable() { 
        
    }

    private void OnRoundChanged(int obj)
    {
        throw new NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitGame();
    }

    void InitGame() {
        _ballObjectPool = new ObjectPool<Ball>(_ballPrefab, 10, transform);
    }
}
