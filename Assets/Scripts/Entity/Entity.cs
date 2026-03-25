using UnityEngine;

public class Entity : MonoBehaviour, IEntity
{
    public void GoDown()
    {
        transform.position += new Vector3(0, -1, 0);
    }

    public void OnDestroyed()
    {
        throw new System.NotImplementedException();
    }

    public void OnRoundEndded()
    {
        throw new System.NotImplementedException();
    }

    public void OnRoundStarted(int currentRound)
    {
        throw new System.NotImplementedException();
    }
}
