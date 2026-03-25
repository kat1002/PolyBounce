using UnityEngine;

public interface IEntity
{
    void OnRoundEndded();
    void OnRoundStarted(int currentRound);
    void OnDestroyed();
    void GoDown();
}
