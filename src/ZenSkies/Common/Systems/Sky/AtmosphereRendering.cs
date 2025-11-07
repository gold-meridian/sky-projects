using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using ZenSkies.Core.Utils;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonHooks;

namespace ZenSkies.Common.Systems.Sky;

public static class AtmosphereRendering
{
    #region Loading

    [OnLoad(Side = ModSide.Client)]
    public static void Load() =>
        PostDrawSunAndMoon += AtmospherePostDraw;

    #endregion

    #region Drawing

    private static void AtmospherePostDraw(SpriteBatch spriteBatch)
    {
        Texture2D gradient = SkyTextures.SkyGradient;

        Color color = GetColor();

        spriteBatch.Draw(gradient, Utilities.ScreenDimensions, color);
    }

    #endregion

    #region Private Methods

    private static Color GetColor() =>
        (SkyConfig.Instance.SkyGradient.GetColor(Utilities.TimeRatio) *
        Easings.InCubic(Main.atmo))
        with { A = 0 };

    #endregion
}
