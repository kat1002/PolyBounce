using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static event Action<int> OnRoundStart;
    public static event Action<int> OnRoundEnded;
    public static event Action<int> OnObstacleDestroyed;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<float> OnFirstBallLanded; // x position of first ball to touch bottom
    public static event Action<int> OnBallCountChanged;  // new total ball count

    // Called by GameManager.OnDestroy to clear all subscribers on scene unload
    public static void Reset()
    {
        OnRoundStart = null;
        OnRoundEnded = null;
        OnObstacleDestroyed = null;
        OnGameStateChanged = null;
        OnFirstBallLanded = null;
        OnBallCountChanged = null;
    }

    public static void InvokeRoundStart(int round) => OnRoundStart?.Invoke(round);
    public static void InvokeRoundEnded(int round) => OnRoundEnded?.Invoke(round);
    public static void InvokeObstacleDestroyed(int points) => OnObstacleDestroyed?.Invoke(points);
    public static void InvokeGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
    public static void InvokeFirstBallLanded(float x) => OnFirstBallLanded?.Invoke(x);
    public static void InvokeBallCountChanged(int count) => OnBallCountChanged?.Invoke(count);
}
