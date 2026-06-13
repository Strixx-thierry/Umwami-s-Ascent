using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;

/// <summary>
/// One-click generator for Umwami's Ascent scenes.
/// Menu: Umwami > Generate Scenes
///
/// Builds MainMenu (with a hideable Settings panel), Win and Lose scenes,
/// converts the existing SampleScene into "Gameplay" (adding GameManager +
/// MusicManager without touching your level), and registers all four in
/// Build Settings in the right order.
///
/// Re-runnable: it overwrites the generated menu/win/lose scenes each time.
/// </summary>
public static class UmwamiSceneGenerator
{
    const string ScenesFolder = "Assets/Scenes";
    const string BgGuid = "f65cc1cbf17867245bbe28efc51f1453"; // the .jpg already in Assets/

    static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);

    [MenuItem("Umwami/Generate Scenes")]
    public static void GenerateAll()
    {
        if (!CheckTmp()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (!AssetDatabase.IsValidFolder(ScenesFolder))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        try
        {
            Sprite bg = LoadBackgroundSprite();

            string menuPath = ScenesFolder + "/MainMenu.unity";
            string winPath  = ScenesFolder + "/Win.unity";
            string losePath = ScenesFolder + "/Lose.unity";

            // Do gameplay first (it renames/opens scenes), then build the UI scenes.
            string gameplayPath = EnsureGameplayScene();

            BuildMainMenu(menuPath, bg);
            BuildEndScreen(winPath, bg, "YOU WON",  new Color(0.45f, 0.85f, 0.45f));
            BuildEndScreen(losePath, bg, "YOU LOST", new Color(0.9f, 0.4f, 0.4f));

            SetBuildSettings(menuPath, gameplayPath, winPath, losePath);

            // Leave the editor on the main menu.
            EditorSceneManager.OpenScene(menuPath, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Umwami's Ascent",
                "Generated and added to Build Settings:\n" +
                "  0  MainMenu  (with Settings panel)\n" +
                "  1  Gameplay  (from your SampleScene)\n" +
                "  2  Win\n" +
                "  3  Lose\n\n" +
                "Next:\n" +
                " - Assign Lobby/Game music clips on the MusicManager (in MainMenu & Gameplay).\n" +
                " - Swap the TitleImage sprite for your logo (MainMenu > Canvas > TitleImage).\n" +
                " - Call GameManager.Instance.Win()/Lose(), or drop a WinZone on a trigger.",
                "OK");
        }
        catch (Exception e)
        {
            Debug.LogError("Umwami scene generation failed: " + e);
            EditorUtility.DisplayDialog("Umwami's Ascent", "Generation failed:\n" + e.Message, "OK");
        }
    }

    // ---------------------------------------------------------------- helpers

    static bool CheckTmp()
    {
        if (TMP_Settings.defaultFontAsset != null) return true;

        EditorUtility.DisplayDialog("TextMeshPro required",
            "TMP Essentials are not imported yet.\n\n" +
            "Open  Window > TextMeshPro > Import TMP Essential Resources,\n" +
            "then run  Umwami > Generate Scenes  again.",
            "OK");
        return false;
    }

    static Sprite LoadBackgroundSprite()
    {
        string path = AssetDatabase.GUIDToAssetPath(BgGuid);
        if (string.IsNullOrEmpty(path)) return null;

        // The asset is imported in "Multiple" sprite mode, so grab the first Sprite sub-asset.
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            if (obj is Sprite s) return s;

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static string EnsureGameplayScene()
    {
        string gameplayPath = ScenesFolder + "/Gameplay.unity";
        string samplePath   = ScenesFolder + "/SampleScene.unity";

        bool hasGameplay = AssetDatabase.LoadAssetAtPath<SceneAsset>(gameplayPath) != null;
        bool hasSample   = AssetDatabase.LoadAssetAtPath<SceneAsset>(samplePath) != null;

        if (!hasGameplay && hasSample)
        {
            // Don't rename a scene that is currently open.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string err = AssetDatabase.RenameAsset(samplePath, "Gameplay");
            if (!string.IsNullOrEmpty(err))
                Debug.LogWarning("Could not rename SampleScene -> Gameplay: " + err);
            AssetDatabase.SaveAssets();
            hasGameplay = AssetDatabase.LoadAssetAtPath<SceneAsset>(gameplayPath) != null;
        }

        Scene scene = hasGameplay
            ? EditorSceneManager.OpenScene(gameplayPath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        EnsureComponentInScene<GameManager>("GameManager");
        EnsureMusicManager();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, gameplayPath);
        return gameplayPath;
    }

    static T EnsureComponentInScene<T>(string goName) where T : Component
    {
        var existing = UnityEngine.Object.FindFirstObjectByType<T>();
        if (existing != null) return existing;
        var go = new GameObject(goName);
        return go.AddComponent<T>();
    }

    static MusicManager EnsureMusicManager()
    {
        var mm = UnityEngine.Object.FindFirstObjectByType<MusicManager>();
        if (mm != null) return mm;
        var go = new GameObject("MusicManager");   // AudioSource auto-added via RequireComponent
        return go.AddComponent<MusicManager>();
    }

    // ---------------------------------------------------------------- UI scene scaffolding

    static Transform NewUIScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateEventSystem();
        return canvasGO.transform;
    }

    static void CreateEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;

        var es = new GameObject("EventSystem", typeof(EventSystem));
        // New Input System only -> use InputSystemUIInputModule when available.
        var moduleType = Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (moduleType != null)
            es.AddComponent(moduleType);
        else
            es.AddComponent<StandaloneInputModule>();
    }

    static RectTransform SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image CreateStretchImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static Image CreateImage(Transform parent, string name, Sprite sprite,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), aMin, aMax, pivot, pos, size);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
        TextAlignmentOptions align, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), aMin, aMax, pivot, pos, sizeDelta);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), Mid, Mid, Mid, pos, size);

        var img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = new Color(0.13f, 0.13f, 0.16f, 0.94f);

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1f, 0.84f, 0.4f, 1f);
        colors.pressedColor     = new Color(0.82f, 0.66f, 0.3f, 1f);
        colors.selectedColor    = new Color(1f, 0.84f, 0.4f, 1f);
        btn.colors = colors;

        var txt = CreateText(go.transform, "Text", label, 40, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Mid, Vector2.zero, Vector2.zero, Color.white);
        var trt = txt.GetComponent<RectTransform>();
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        if (onClick != null)
            UnityEventTools.AddPersistentListener(btn.onClick, onClick);

        return btn;
    }

    static Slider CreateSlider(Transform parent, Vector2 pos, Vector2 size)
    {
        var res = new DefaultControls.Resources
        {
            standard   = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            knob       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        };

        var go = DefaultControls.CreateSlider(res);
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), Mid, Mid, Mid, pos, size);

        var slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = MusicManager.DefaultVolume;
        return slider;
    }

    // ---------------------------------------------------------------- scene builders

    static void BuildMainMenu(string path, Sprite bg)
    {
        var canvas = NewUIScene();
        EnsureMusicManager();

        CreateStretchImage(canvas, "Background", bg, Color.white);
        CreateStretchImage(canvas, "Overlay", null, new Color(0f, 0f, 0f, 0.35f));

        // Title is an Image — swap its sprite for your logo.
        CreateImage(canvas, "TitleImage", bg,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -110), new Vector2(840, 300), Color.white);

        var menuHost = new GameObject("MainMenuUI", typeof(RectTransform));
        menuHost.transform.SetParent(canvas, false);
        var menuUI = menuHost.AddComponent<MainMenuUI>();

        CreateButton(canvas, "PlayButton",     "PLAY",     new Vector2(0, -30),  new Vector2(380, 90), menuUI.OnPlay);
        CreateButton(canvas, "SettingsButton", "SETTINGS", new Vector2(0, -140), new Vector2(380, 90), menuUI.OnOpenSettings);
        CreateButton(canvas, "QuitButton",     "QUIT",     new Vector2(0, -250), new Vector2(380, 90), menuUI.OnQuit);

        // ---- Settings panel (hidden at runtime by MainMenuUI.Start) ----
        var panelGO = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvas, false);
        var prt = panelGO.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panelGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.94f);

        var settingsUI = panelGO.AddComponent<SettingsPanelUI>();
        menuUI.settingsPanel = panelGO;

        CreateText(panelGO.transform, "SettingsTitle", "SETTINGS", 72, TextAlignmentOptions.Center,
            Mid, Mid, Mid, new Vector2(0, 250), new Vector2(700, 110), Color.white);

        CreateText(panelGO.transform, "LobbyLabel", "Lobby Music", 40, TextAlignmentOptions.Right,
            Mid, Mid, Mid, new Vector2(-330, 80), new Vector2(360, 70), Color.white);
        var lobbySlider = CreateSlider(panelGO.transform, new Vector2(120, 80), new Vector2(360, 30));
        var lobbyVal = CreateText(panelGO.transform, "LobbyValue", "70%", 36, TextAlignmentOptions.Left,
            Mid, Mid, Mid, new Vector2(390, 80), new Vector2(140, 70), Color.white);

        CreateText(panelGO.transform, "GameLabel", "Bossfight Music", 40, TextAlignmentOptions.Right,
            Mid, Mid, Mid, new Vector2(-330, -30), new Vector2(360, 70), Color.white);
        var gameSlider = CreateSlider(panelGO.transform, new Vector2(120, -30), new Vector2(360, 30));
        var gameVal = CreateText(panelGO.transform, "GameValue", "70%", 36, TextAlignmentOptions.Left,
            Mid, Mid, Mid, new Vector2(390, -30), new Vector2(140, 70), Color.white);

        settingsUI.lobbyVolumeSlider = lobbySlider;
        settingsUI.gameVolumeSlider  = gameSlider;
        settingsUI.lobbyValueLabel   = lobbyVal;
        settingsUI.gameValueLabel    = gameVal;

        CreateButton(panelGO.transform, "BackButton", "BACK", new Vector2(0, -230), new Vector2(300, 80), menuUI.OnCloseSettings);

        panelGO.SetActive(false);

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    static void BuildEndScreen(string path, Sprite bg, string title, Color titleColor)
    {
        var canvas = NewUIScene();
        EnsureMusicManager();

        CreateStretchImage(canvas, "Background", bg, Color.white);
        CreateStretchImage(canvas, "Overlay", null, new Color(0f, 0f, 0f, 0.55f));

        CreateText(canvas, "ResultText", title, 130, TextAlignmentOptions.Center,
            Mid, Mid, Mid, new Vector2(0, 190), new Vector2(1300, 260), titleColor);

        var host = new GameObject("EndScreenUI", typeof(RectTransform));
        host.transform.SetParent(canvas, false);
        var end = host.AddComponent<EndScreenUI>();

        CreateButton(canvas, "RetryButton", "RETRY",     new Vector2(0, -40),  new Vector2(380, 90), end.OnRetry);
        CreateButton(canvas, "MenuButton",  "MAIN MENU", new Vector2(0, -150), new Vector2(380, 90), end.OnMainMenu);
        CreateButton(canvas, "QuitButton",  "QUIT",      new Vector2(0, -260), new Vector2(380, 90), end.OnQuit);

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
    }

    static void SetBuildSettings(params string[] paths)
    {
        var list = new List<EditorBuildSettingsScene>();
        foreach (var p in paths)
            if (!string.IsNullOrEmpty(p))
                list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
