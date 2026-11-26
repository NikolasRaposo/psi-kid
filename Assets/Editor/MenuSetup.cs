using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// One-click restyle of the menus (PSI-KID ▸ Apply menu style): builds the Barlow
/// font asset, skins every button with the pixel-art sprites, adds the Settings
/// buttons, frames the pause and end panels, and puts the music under the music
/// volume. Safe to run again.
/// </summary>
public static class MenuSetup {
    private const string FontPath = "Assets/Resources/Fonts/Barlow-Bold SDF.asset";
    private const string MainMenuScene = "Assets/Scenes/MainMenu/Main Menu.unity";
    private const string LevelScene = "Assets/Scenes/Levels/Fase 01.unity";

    [MenuItem("PSI-KID/Apply menu style")]
    public static void Apply() {
        BuildFont();
        StyleMainMenu();
        StyleLevel();
        AssetDatabase.SaveAssets();
    }

    private static void BuildFont() {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath) != null) {
            return;
        }
        System.IO.Directory.CreateDirectory("Assets/Resources/Fonts");
        var ttf = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Barlow-Bold.ttf");
        var fa = TMP_FontAsset.CreateFontAsset(ttf, 64, 6, GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
        fa.name = "Barlow-Bold SDF";
        AssetDatabase.CreateAsset(fa, FontPath);
        fa.material.name = "Barlow-Bold SDF Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        fa.atlasTextures[0].name = "Barlow-Bold SDF Atlas";
        AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
        // bake the printable ASCII range and freeze the atlas so the build needs no font file
        string chars = new string(Enumerable.Range(32, 95).Select(i => (char)i).ToArray()) + "▸·–—";
        fa.TryAddCharacters(chars, out _);
        fa.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- main menu

    private static void StyleMainMenu() {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
        var ctrl = Object.FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
        var root = ctrl.transform;

        var start = Find<Button>(root, "BtnStart");
        var exit = Find<Button>(root, "BtnExit");
        var settings = Find<Button>(root, "BtnSettings") ?? Duplicate(start, "BtnSettings");
        Rewire(settings, ctrl.OpenSettings);

        Size(start, new Vector2(0f, -60f));
        Size(settings, new Vector2(0f, -170f));
        Size(exit, new Vector2(0f, -280f));
        UiStyle.Button(start, "START", 42f);
        UiStyle.Button(settings, "SETTINGS", 42f);
        UiStyle.Button(exit, "QUIT", 42f);

        var logo = root.Find("Logo") as RectTransform;
        if (logo != null) {
            logo.anchoredPosition = new Vector2(0f, 210f);
            logo.sizeDelta = new Vector2(330f, 330f);
        }
        var credits = root.Find("CreditsTxt")?.GetComponent<TMP_Text>();
        if (credits != null) {
            UiStyle.Label(credits, "By Nikolas Raposo  ·  Music MintoDog  ·  SFX Kenney (CC0)", 24f, UiStyle.Muted);
            credits.characterSpacing = 2f;
            credits.alignment = TextAlignmentOptions.BottomLeft;
            var rt = credits.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(32f, 24f);
            rt.sizeDelta = new Vector2(900f, 40f);
        }

        var so = new SerializedObject(ctrl);
        so.FindProperty("exitButton").objectReferenceValue = exit.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();

        AddMusicVolume();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- level: pause + end panels

    private static void StyleLevel() {
        var scene = EditorSceneManager.OpenScene(LevelScene, OpenSceneMode.Single);
        var pause = Object.FindAnyObjectByType<PauseManager>(FindObjectsInactive.Include);

        var pausePanel = FindAll("PausePanel");
        if (pausePanel != null) {
            Overlay(pausePanel, 30f, new Vector2(620f, 680f));
            Title(pausePanel, "PAUSED", 250f);
            var resume = Find<Button>(pausePanel, "BtnResume");
            var settings = Find<Button>(pausePanel, "BtnSettings") ?? Duplicate(resume, "BtnSettings");
            Rewire(settings, pause.OpenSettings);
            var restart = Find<Button>(pausePanel, "BtnRestart");
            var menu = Find<Button>(pausePanel, "BtnMainMenu");
            Stack(new[] { resume, settings, restart, menu }, new[] { "RESUME", "SETTINGS", "RESTART", "MAIN MENU" }, 120f);
        }

        var end = FindAll("EndGame");
        if (end != null) {
            Overlay(end, -5f, new Vector2(800f, 420f));
            Title(end, "THANKS FOR PLAYING", 120f);
            var again = Find<Button>(end, "BtnTryAgain");
            var menu = Find<Button>(end, "BtnMainMenu");
            Stack(new[] { again, menu }, new[] { "PLAY AGAIN", "MAIN MENU" }, -20f);
        }

        AddMusicVolume();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- helpers

    private static T Find<T>(Transform root, string name) where T : Component =>
        root.GetComponentsInChildren<T>(true).FirstOrDefault(c => c.name == name);

    private static Transform FindAll(string name) =>
        Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(t => t.name == name);

    private static Button Duplicate(Button src, string name) {
        var copy = Object.Instantiate(src.gameObject, src.transform.parent);
        copy.name = name;
        copy.transform.SetSiblingIndex(src.transform.GetSiblingIndex() + 1);
        return copy.GetComponent<Button>();
    }

    private static void Rewire(Button b, UnityAction action) {
        while (b.onClick.GetPersistentEventCount() > 0) {
            UnityEventTools.RemovePersistentListener(b.onClick, 0);
        }
        UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    private static void Size(Button b, Vector2 pos, float width = 380f) {
        var rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 95f);
    }

    private static void Stack(Button[] buttons, string[] labels, float top) {
        for (int i = 0; i < buttons.Length; i++) {
            if (buttons[i] == null) {
                continue;
            }
            Size(buttons[i], new Vector2(0f, top - i * 110f), 440f);
            UiStyle.Button(buttons[i], labels[i], 38f);
        }
    }

    /// <summary>Darkens the full-screen panel and puts a pixel-art frame behind its content.</summary>
    private static void Overlay(Transform panel, float centerY, Vector2 frameSize) {
        var bg = panel.GetComponent<Image>();
        bg.sprite = null;
        bg.type = Image.Type.Simple;
        bg.color = UiStyle.Backdrop;

        var frame = panel.Find("Frame") as RectTransform;
        if (frame == null) {
            frame = UiStyle.Rect("Frame", panel);
            frame.SetAsFirstSibling();
            frame.gameObject.AddComponent<Image>();
        }
        UiStyle.Place(frame, new Vector2(0f, centerY), frameSize);
        var img = frame.GetComponent<Image>();
        UiStyle.Sliced(img, "ui_panel");
        img.raycastTarget = false;
    }

    private static void Title(Transform panel, string text, float y) {
        var t = panel.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(x => x.transform.parent == panel);
        if (t == null) {
            return;
        }
        UiStyle.Label(t, text, 60f, UiStyle.Title);
        UiStyle.Place(t.rectTransform, new Vector2(0f, y), new Vector2(900f, 90f));
    }

    private static void AddMusicVolume() {
        foreach (var src in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (src.clip != null && src.loop && src.GetComponent<MusicVolume>() == null) {
                src.gameObject.AddComponent<MusicVolume>();
            }
        }
    }
}
