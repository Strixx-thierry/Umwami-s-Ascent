using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float smoothSpeed = 5f;
    public float baseYOffset = 2f;
    public float panAmount = 2f;
    public float panSpeed = 5f;

    float currentPanY;
    float targetPanY;

    void LateUpdate()
    {
        if (player == null) return;

        // Manual pan (Up / Down arrows)
        var kb = Keyboard.current;
        if (kb != null && kb.upArrowKey.isPressed)
            targetPanY = panAmount;
        else if (kb != null && kb.downArrowKey.isPressed)
            targetPanY = -panAmount;
        else
            targetPanY = 0f;

        currentPanY = Mathf.Lerp(currentPanY, targetPanY, Time.deltaTime * panSpeed);

        Vector3 targetPos = new Vector3(
            player.position.x,
            player.position.y + baseYOffset + currentPanY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );
    }

    // Call this when player lands (optional)
    public void RecenterY(float groundY)
    {
        Vector3 pos = transform.position;
        pos.y = groundY + baseYOffset;
        transform.position = pos;
    }
}
