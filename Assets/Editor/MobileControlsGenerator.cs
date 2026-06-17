using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.OnScreen;
using TMPro;

/// <summary>
/// One-click on-screen touch controls for the mobile build.
/// Menu: Tools > Generate Mobile Controls (run it on the Gameplay scene).
///
/// Builds Left/Right/Jump/Attack buttons using the Input System's
/// OnScreenButton, each mapped to the keyboard key your scripts already read
/// (A/D/Space/J). So touch works on mobile AND clicking works on desktop, with
/// zero changes to PlayerMovement / PlayerAttack.
/// Re-runnable. This is editor tooling; the generated buttons live in the scene.
/// </summary>
public static class MobileControlsGenerator
{
    [MenuItem("Tools/Generate Mobile Controls")]
    public static void Generate()
    {
        if (TMP_Settings.defaultFontAsset == null)
        {
            EditorUtility.DisplayDialog("TextMeshPro required",
                "Import TMP Essentials first (Window > TextMeshPro > Import TMP Essential Resources), then run again.", "OK");
            return;
        }

        var old = GameObject.Find("MobileControls");
        if (old != null) Object.DestroyImmediate(old);

        EnsureEventSystem();

        var canvasGO = new GameObject("MobileControls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // above the HUD
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bl = new Vector2(0, 0); // bottom-left anchor
        var br = new Vector2(1, 0); // bottom-right anchor

        // Movement (bottom-left)
        CreateButton(canvasGO.transform, "LeftButton",  "<",   bl, new Vector2(200, 200), new Vector2(170, 170), "<Keyboard>/a");
        CreateButton(canvasGO.transform, "RightButton", ">",   bl, new Vector2(410, 200), new Vector2(170, 170), "<Keyboard>/d");

        // Actions (bottom-right)
        CreateButton(canvasGO.transform, "JumpButton",   "JUMP", br, new Vector2(-200, 200), new Vector2(190, 190), "<Keyboard>/space");
        CreateButton(canvasGO.transform, "AttackButton", "ATK",  br, new Vector2(-410, 360), new Vector2(160, 160), "<Keyboard>/j");

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = canvasGO;

        EditorUtility.DisplayDialog("Mobile Controls",
            "Added Left / Right / Jump / Attack on-screen buttons (new Input System).\n" +
            "They simulate A / D / Space / J, so they work on touch and on desktop.\n\nPress Ctrl+S to save.",
            "OK");
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
        var moduleType = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (moduleType != null) es.AddComponent(moduleType);
        else es.AddComponent<StandaloneInputModule>();
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 anchor,
        Vector2 pos, Vector2 size, string controlPath)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        img.color = new Color(1f, 1f, 1f, 0.4f);
        img.raycastTarget = true;

        // label
        var t = new GameObject("Label", typeof(RectTransform));
        t.transform.SetParent(go.transform, false);
        var trt = t.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 42;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        // On-Screen button mapped to a keyboard key
        var osb = go.AddComponent<OnScreenButton>();
        var so = new SerializedObject(osb);
        var prop = so.FindProperty("m_ControlPath");
        if (prop != null)
        {
            prop.stringValue = controlPath;
            so.ApplyModifiedProperties();
        }
        else
        {
            osb.controlPath = controlPath; // fallback to public API
        }
    }
}
