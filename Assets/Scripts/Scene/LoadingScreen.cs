using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image _wipeImage;

    private Material _mat;
    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");

    private void Awake()
    {
        // Instance the material so we don't modify the shared asset
        _mat = Instantiate(_wipeImage.material);
        _wipeImage.material = _mat;
        _mat.SetFloat(CutoffId, 0f);
    }

    // Cover the screen (transition in)
    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _mat.SetFloat(CutoffId, Mathf.Clamp01(t));
            yield return null;
        }
    }

    // Reveal the new scene (transition out)
    public IEnumerator FadeOut(float duration)
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / duration;
            _mat.SetFloat(CutoffId, Mathf.Clamp01(t));
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (_mat != null) Destroy(_mat);
    }
}
