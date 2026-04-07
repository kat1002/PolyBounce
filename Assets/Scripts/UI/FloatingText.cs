using DG.Tweening;
using TMPro;
using UnityEngine;

// Spawned at a world position, floats upward, then destroys itself.
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _riseDistance = 0.8f;
    [SerializeField] private float _duration = 0.8f;
    [SerializeField] private float _holdBeforeFade = 0.25f;

    public void Play(string message, Vector3 worldPosition)
    {
        transform.position = worldPosition;
        _text.text = message;
        _text.alpha = 1f;

        transform.DOMoveY(worldPosition.y + _riseDistance, _duration).SetEase(Ease.OutQuad);
        DOVirtual.Float(1f, 0f, _duration - _holdBeforeFade, a => _text.alpha = a)
            .SetDelay(_holdBeforeFade)
            .OnComplete(() => Destroy(gameObject));
    }
}
