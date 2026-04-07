using UnityEngine;

public class WallManager : MonoBehaviour
{
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform topWall;

    [SerializeField] private Camera cam;

    void Start()
    {
        cam = Camera.main;
        SetupWalls();
    }

    void SetupWalls()
    {
        Vector2 left = cam.ViewportToWorldPoint(new Vector2(0, 0.5f));
        Vector2 right = cam.ViewportToWorldPoint(new Vector2(1, 0.5f));
        Vector2 top = cam.ViewportToWorldPoint(new Vector2(0.5f, 1));

        leftWall.position = new Vector2(left.x, 0);
        rightWall.position = new Vector2(right.x, 0);
        topWall.position = new Vector2(0, top.y);
    }
}
