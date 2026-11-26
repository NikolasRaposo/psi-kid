using UnityEditor;
using UnityEngine;

/// <summary>
/// Import settings for the generated pixel art in Resources/Art: 16 pixels per
/// unit, point filtering, no compression. The back wall tiles (repeat + full rect)
/// so it can fill a room in Tiled draw mode. UI sprites (ui_*) get 9-slice borders.
/// </summary>
public class PixelArtImporter : AssetPostprocessor {
    private void OnPreprocessTexture() {
        if (!assetPath.Replace('\\', '/').Contains("/Resources/Art/")) {
            return;
        }
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 16;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;

        bool smooth = assetPath.EndsWith("glow.png") || assetPath.EndsWith("vignette.png");
        if (smooth) {
            ti.filterMode = FilterMode.Bilinear;
        }

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        if (assetPath.EndsWith("back_wall.png")) {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.wrapMode = TextureWrapMode.Repeat;
        }
        if (assetPath.EndsWith("plate_off.png") || assetPath.EndsWith("plate_on.png")) {
            settings.spriteAlignment = (int)SpriteAlignment.Center;
        }
        ti.SetTextureSettings(settings);

        // UI pieces are 9-sliced: keep the full rect and set the corner borders.
        string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (file.StartsWith("ui_")) {
            float b = file.StartsWith("ui_button") ? 5f : file == "ui_panel" ? 8f : file.StartsWith("ui_slider") ? 3f : 0f;
            settings.spriteBorder = new Vector4(b, b, b, b);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }
    }
}
