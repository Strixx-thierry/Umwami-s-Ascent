using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shows the on-screen mobile controls only on touch devices (phones/tablets,
/// including a mobile WebGL browser) and hides them on desktop.
/// Put this on the "MobileControls" Canvas object.
/// </summary>
public class MobileControlsVisibility : MonoBehaviour
{
    [Tooltip("Keep the controls visible while testing in the Editor.")]
    public bool showInEditor = true;

    void Awake()
    {
        bool isTouch = Application.isMobilePlatform || Touchscreen.current != null;

#if UNITY_EDITOR
        if (showInEditor) isTouch = true;
#endif

        gameObject.SetActive(isTouch);
    }
}
