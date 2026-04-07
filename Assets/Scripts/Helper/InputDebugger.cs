using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Attach to any active GameObject temporarily to diagnose simulator touch issues.
// Remove once the button issue is resolved.
public class InputDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                Vector2 pos = touch.position.ReadValue();
                bool overUI = EventSystem.current != null &&
                              EventSystem.current.IsPointerOverGameObject((int)touch.touchId.ReadValue());
                Debug.Log($"[Touch] pos={pos}  overUI={overUI}  touchId={(int)touch.touchId.ReadValue()}");
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            bool overUI = EventSystem.current != null &&
                          EventSystem.current.IsPointerOverGameObject();
            Debug.Log($"[Mouse] pos={pos}  overUI={overUI}");
        }
    }
}
