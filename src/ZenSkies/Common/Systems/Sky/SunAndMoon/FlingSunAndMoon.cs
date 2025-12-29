using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ZenSkies.Common.Systems.Compat;
using ZenSkies.Core.Utils;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonHooks;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonSystem;

namespace ZenSkies.Common.Systems.Sky;

[Autoload(Side = ModSide.Client)]
public static class FlingSunAndMoon
{
    private static readonly Vector2 velocity_multiplier = new(.92f, .85f);
    private const float mod_multiplier = .976f;

    private static Vector2 sunMoonOldPosition;

    private static Vector2 sunMoonVelocity;

    [OnLoad]
    private static void Load()
    {
        PostDrawSunAndMoon += PostDrawSunAndMoon_Fling;
    }

    private static void PostDrawSunAndMoon_Fling(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot)
    {
        if (!Main.gameMenu ||
            Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        Vector2 position = Main.dayTime ? Info.SunPosition : Info.MoonPosition;

        float sunMoonWidth =
            Main.dayTime ?
            TextureAssets.Sun.Value.Width :
            TextureAssets.Moon[Main.moonType].Value.Width;

        double timeLength =
            Main.dayTime ?
            Main.dayLength :
            Main.nightLength;

        if (Main.alreadyGrabbingSunOrMoon)
        {
            sunMoonVelocity = position - sunMoonOldPosition;

            sunMoonOldPosition = position;

            return;
        }

        sunMoonVelocity *= velocity_multiplier;

        if (Main.dayTime)
        {
            Main.sunModY += (short)sunMoonVelocity.Y;
        }
        else
        {
            Main.moonModY += (short)sunMoonVelocity.Y;
        }

        Main.sunModY = (short)(Main.sunModY * mod_multiplier);
        Main.moonModY = (short)(Main.moonModY * mod_multiplier);

        double newTime =
            RedSunSystem.FlipSunAndMoon ?
            RedSunFlinging(position, sunMoonWidth) :
            (position.X + sunMoonVelocity.X + sunMoonWidth);

        newTime /= Utilities.ScreenSize.X + (sunMoonWidth * 2f);

        newTime *= timeLength;

        Main.time = newTime;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double RedSunFlinging(Vector2 position, float sunMoonWidth)
    {
        float moonAdjust = Main.dayTime ? 0 : RedSunSystem.MoonAdjustment.X;

        return Utilities.ScreenSize.X - (position.X + sunMoonVelocity.X) + moonAdjust + sunMoonWidth;
    }
}
