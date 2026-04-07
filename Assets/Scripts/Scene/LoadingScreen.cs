using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;

    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            _canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / duration;
            _canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
    }
}
