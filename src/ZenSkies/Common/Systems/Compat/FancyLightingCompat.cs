using Daybreak.Common.Features.Hooks;
using FancyLighting;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;

namespace ZenSkies.Common.Systems.Compat;

[ExtendsFromMod("FancyLighting")]
[Autoload(Side = ModSide.Client)]
public static class FancyLightingCompat
{
    public static bool IsEnabled { get; private set; }

    [ModSystemHooks.PostSetupContent]
    private static void PostSetupContent()
    {
        IsEnabled = true;

        // Unwanted sun shader
        if (SkyConfig.Instance.UseSunAndMoon)
        {
            On_Main.DrawSunAndMoon -= ModContent.GetInstance<FancyLightingMod>()._Main_DrawSunAndMoon;
        }

        /*
        // Reapply their background gradient hook so it takes priority over ours.
        On_Main.DrawStarsInBackground -= FancySkyRendering._Main_DrawStarsInBackground;
        On_Main.DrawStarsInBackground += FancySkyRendering._Main_DrawStarsInBackground;
        */
    }
}
