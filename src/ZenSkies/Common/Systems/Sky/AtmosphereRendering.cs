using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using ZenSkies.Core.Utils;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonHooks;

namespace ZenSkies.Common.Systems.Sky;

[Autoload(Side = ModSide.Client)]
public static class AtmosphereRendering
{
    [OnLoad]
    public static void Load()
    {
        PostDrawSunAndMoon += AtmospherePostDraw;
    }

    private static void AtmospherePostDraw(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot)
    {
        Texture2D gradient = SkyTextures.SkyGradient;

        Color color = GetColor();

        spriteBatch.Begin(in snapshot);
        {
            spriteBatch.Draw(gradient, Utilities.ScreenDimensions, color);
        }
        spriteBatch.End();

        static Color GetColor()
        {
            Color grad = 
                SkyConfig.Instance.SkyGradient.GetColor(Utilities.TimeRatio) *
                Easings.InCubic(Main.atmo);

            grad.A = 0;

            return grad;
        }
    }
}
