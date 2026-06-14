using HarmonyLib;
using TMPro;

namespace LocalizeUs.Patches;

[HarmonyPatch]
public static class TmpAwakePatch
{
    private static bool _fallbackRegistered;

    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.Awake))]
    [HarmonyPostfix]
    public static void TmpAwakePostfix(TextMeshPro __instance)
    {
        if (__instance.font.name == "LiberationSans SDF")
        {
            // Instead of replacing the font entirely (which causes rendering
            // differences in weight, outline, and breaks other asset-bundle
            // fonts), register the extended font as a fallback on the original
            // LiberationSans. The original font renders Latin text exactly as
            // the base game does; the extended font only kicks in for glyphs
            // that are missing from the original (e.g. ĄČĘĖĮŠŲŪŽ).
            if (!_fallbackRegistered)
            {
                RegisterFallback(__instance.font, LocaleUsAssets.LibSansRegTmp);
                _fallbackRegistered = true;
            }
        }
        /*else if (component.font.name == "Brook SDF" && CustomLocale.LangsWithCustomFont.Contains(auLang))
        {
            ogBrookTmp = component.font;
            component.font = LocaleUsAssets.AmaticScBoldTmp;
        }*/
    }

    private static void RegisterFallback(TMP_FontAsset mainFont, TMP_FontAsset fallbackFont)
    {
        var fallbacks = mainFont.fallbackFontAssetTable;

        // Check whether the fallback is already registered.
        if (fallbacks != null)
        {
            foreach (var f in fallbacks)
            {
                if (f == fallbackFont)
                    return;
            }
        }

        // Create or extend the fallback list.
        var newList = new Il2CppSystem.Collections.Generic.List<TMP_FontAsset>();
        if (fallbacks != null)
        {
            foreach (var f in fallbacks)
                newList.Add(f);
        }
        newList.Add(fallbackFont);
        mainFont.fallbackFontAssetTable = newList;
    }
}