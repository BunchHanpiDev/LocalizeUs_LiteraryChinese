using HarmonyLib;
using TMPro;
using UnityEngine;

namespace LocalizeUs.Patches;

[HarmonyPatch]
public static class TmpAwakePatch
{
    private static bool _sharedMaterialAdjusted;

    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.Awake))]
    [HarmonyPostfix]
    public static void TmpAwakePostfix(TextMeshPro __instance)
    {
        if (__instance.font.name == "LiberationSans SDF")
        {
            // The custom LiberationSans Extended font asset ships with a basic
            // SDF material that differs from the original in two ways:
            //   1. SDF parameters (e.g. _FaceDilate) differ → text looks bolder
            //   2. The shader variant may lack outline support → text loses its
            //      outline stroke (visible on player names in chat bubbles etc.)
            //
            // Fix both by copying the original material's shader and rendering
            // properties to the replacement font's material (once, statically).
            if (!_sharedMaterialAdjusted)
            {
                FixupFontMaterial(__instance.font.material, LocaleUsAssets.LibSansRegTmp.material);
                _sharedMaterialAdjusted = true;
            }

            __instance.font = LocaleUsAssets.LibSansRegTmp;
        }
        /*else if (component.font.name == "Brook SDF" && CustomLocale.LangsWithCustomFont.Contains(auLang))
        {
            ogBrookTmp = component.font;
            component.font = LocaleUsAssets.AmaticScBoldTmp;
        }*/
        __instance.ForceMeshUpdate(false, false);
    }

    /// <summary>
    /// Copies the original font material's shader and rendering properties
    /// to the replacement font material, while preserving the replacement
    /// font's atlas texture (which holds the extended character glyphs).
    /// </summary>
    private static void FixupFontMaterial(Material orig, Material repl)
    {
        // Preserve the replacement font's atlas texture.
        var replAtlas = repl.mainTexture;

        // Adopt the original material's shader so that outline, underlay,
        // and other rendering features match the base game exactly.
        repl.shader = orig.shader;

        // Restore the atlas — changing the shader may reset it.
        repl.mainTexture = replAtlas;

        // Copy scalar SDF properties that control glyph weight and spacing.
        CopyFloat("_FaceDilate");
        CopyFloat("_OutlineWidth");
        CopyFloat("_OutlineSoftness");
        CopyFloat("_UnderlayDilate");
        CopyFloat("_UnderlaySoftness");
        CopyFloat("_UnderlayOffsetX");
        CopyFloat("_UnderlayOffsetY");
        CopyFloat("_GlowPower");
        CopyFloat("_GlowOffset");
        CopyFloat("_ScaleRatioA");
        CopyFloat("_ScaleRatioB");
        CopyFloat("_ScaleRatioC");
        CopyFloat("_WeightNormal");
        CopyFloat("_WeightBold");
        CopyFloat("_Sharpness");

        // Copy color properties (outline color, face color, underlay color).
        CopyColor("_OutlineColor");
        CopyColor("_FaceColor");
        CopyColor("_UnderlayColor");
        CopyColor("_GlowColor");

        return;

        void CopyFloat(string name)
        {
            if (orig.HasProperty(name) && repl.HasProperty(name))
                repl.SetFloat(name, orig.GetFloat(name));
        }

        void CopyColor(string name)
        {
            if (orig.HasProperty(name) && repl.HasProperty(name))
                repl.SetColor(name, orig.GetColor(name));
        }
    }
}