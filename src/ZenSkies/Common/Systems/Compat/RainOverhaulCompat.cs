using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Graphics;
using RainOverhaul.Content;
using System;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace ZenSkies.Common.Systems.Compat;

[ExtendsFromMod("RainOverhaul")]
[Autoload(Side = ModSide.Client)]
public static class RainOverhaulCompat
{
    private const string rain_filter_key = "RainFilter";

    public static bool IsEnabled { get; private set; }

    [ModSystemHooks.PostSetupContent]
    private static void PostSetupContent()
    {
        IsEnabled = true;

        // See https://github.com/supchyan/terraria-rain-overhaul/issues/1
        MonoModHooks.Add(
            typeof(RainSystem).GetMethod(nameof(RainSystem.PostUpdateTime), BindingFlags.Public | BindingFlags.Instance),
            PostUpdateTime_UseNoiseTexture
        );
    }

    private static void PostUpdateTime_UseNoiseTexture(Action<RainSystem> orig, RainSystem self)
    {
        orig(self);

        if (Filters.Scene[rain_filter_key] is null)
        {
            return;
        }

        Filters.Scene[rain_filter_key].GetShader()
            .UseImage(MiscTextures.ColoredNoise.Asset, 0, SamplerState.LinearWrap);
    }
}
