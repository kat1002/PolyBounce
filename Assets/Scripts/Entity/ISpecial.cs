using UnityEngine;

public interface ISpecial
{
    int PointValue { get; }
    void Init(Vector3 position);
    void TriggerEffect();
}
