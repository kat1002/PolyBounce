using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.05f, -0.05f);

    private void Awake()
    {
        _rawImage.texture.wrapMode = TextureWrapMode.Repeat;
    }

    private void Update()
    {
        _rawImage.uvRect = new Rect(
            _rawImage.uvRect.position + _scrollSpeed * Time.deltaTime,
            _rawImage.uvRect.size
        );
    }
}
