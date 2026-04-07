using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public enum GameState { MainMenu, StartRound, InRound, PostRound, GameOver, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Walls")]
    [SerializeField] private Transform _leftWall;
    [SerializeField] private Transform _rightWall;
    [SerializeField] private Transform _upWall;
    [SerializeField] private Transform _deadLine;
    [SerializeField] private Transform _endLine;

    public GameState CurrentState { get; private set; }
    private GameState _stateBeforePause;
    private int _activeBallCount;
    private int _currentRound;
    private int _ballCount = 5;
    private bool _firstBallLanded;

    public int CurrentRound => _currentRound;
    public int BallCount => _ballCount;
    public Transform EndLine => _endLine;
    public Transform DeadLine => _deadLine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        SetState(GameState.StartRound);
        HandleNewRound();
    }

    public void SetState(GameState newState)
    {
        Debug.Log($"[GameManager] State: {CurrentState} → {newState}");
        CurrentState = newState;
        EventManager.InvokeGameStateChanged(newState);
    }

    public void PrepareRound()
    {
        _activeBallCount = _ballCount;
        _firstBallLanded = false;
    }

    public void OnBallReturned(float xPosition)
    {
        if (!_firstBallLanded)
        {
            _firstBallLanded = true;
            EventManager.InvokeFirstBallLanded(xPosition);
        }

        _activeBallCount--;
        if (_activeBallCount <= 0 && CurrentState == GameState.InRound)
        {
            _activeBallCount = 0;
            SetState(GameState.PostRound);
        }
    }

    private void OnDestroy()
    {
        EventManager.Reset();
    }

    public void Pause()
    {
        if (CurrentState == GameState.Paused) return;
        _stateBeforePause = CurrentState;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(_stateBeforePause);
    }

    public void AddBall(int count)
    {
        _ballCount += count;
        EventManager.InvokeBallCountChanged(_ballCount);
    }

    public void HandleNewRound()
    {
        _currentRound++;
        EventManager.InvokeRoundStart(CurrentRound);
    }
}
