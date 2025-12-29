using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonSystem;

namespace ZenSkies.Common.Systems.Compat;

/*
 * Lights and Shadows -- aside from looking horrendous -- has numerous
   technical issues that make cross-compat rather difficult.
 * A majority of the issues could be solved if the mod used Terraria's
   Filter system and or their own, instead of blindly injecting into
   FilterManager.EndCapture.
 * The above is visible in-game by setting 'Wave Quality' in video
   settings to 'Off.'
*/
[ExtendsFromMod("Lights")]
[Autoload(Side = ModSide.Client)]
public static class LightsAndShadowsCompat
{
    public static bool IsEnabled { get; private set; }

    [OnLoad]
    private static void Load()
    {
        IsEnabled = true;

        MonoModHooks.Add(
            typeof(Lights.Lights).GetMethod(nameof(Lights.Lights.GetSunPos), BindingFlags.Public | BindingFlags.Static),
            GetSunPos_UseCorrectPosition
        );
    }

    private static Vector2 GetSunPos_UseCorrectPosition(Action<RenderTarget2D> orig, RenderTarget2D render)
    {
        Vector2 position = Main.dayTime ? Info.SunPosition : Info.MoonPosition;

        if (Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically))
        {
            position.Y = render.Height - position.Y;
        }

        return position;
    }
}
