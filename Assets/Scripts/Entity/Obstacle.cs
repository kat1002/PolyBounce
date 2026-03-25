using TMPro;
using UnityEngine;

public class Obstacle : Entity, IPoolable
{
    [SerializeField] private int _health;
    private TextMeshProUGUI _healthText;

    public void Init(int currentRound, Vector3 position) {
        _health = Random.Range(currentRound, currentRound * 3);
        transform.position = position;
        _healthText.text = _health.ToString();
    }

    private void UpdateHealthText(int amountUpdated)
    {
        _healthText.text = _health.ToString();
    }

    public void ReceiveDamage(int damage)
    {
        _health -= damage;
        UpdateHealthText(damage);
        if (_health <= 0) GameManager.Instance.ObstaclePool.Release(this);
    }

    public void OnSpawn()
    {
        throw new System.NotImplementedException();
    }

    public void OnDespawn()
    {
        _health = 0;
        _healthText.text = "";
    }
}
