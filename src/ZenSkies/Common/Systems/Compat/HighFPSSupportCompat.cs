using Daybreak.Common.Features.Hooks;
using System;
using System.Diagnostics;
using System.Reflection;
using Terraria.ModLoader;
using static System.Reflection.BindingFlags;

namespace ZenSkies.Common.Systems.Compat;

[ExtendsFromMod("HighFPSSupport")]
[Autoload(Side = ModSide.Client)]
public static class HighFPSSupportCompat
{
    private static PropertyInfo? isPartialTickInfo;

    public static bool IsPartialTick => (bool?)isPartialTickInfo?.GetValue(null) ?? false;

    public static bool IsEnabled { get; private set; }

    [OnLoad]
    private static void Load()
    {
        if (!ModLoader.TryGetMod("HighFPSSupport", out Mod highFPSSupport))
        {
            return;
        }

        IsEnabled = true;

        Assembly highFPSAsm = highFPSSupport.Code;

        Type? tickRateModifier = highFPSAsm.GetType("HighFPSSupport.TickRateModifier");

        Debug.Assert(tickRateModifier is not null);

        isPartialTickInfo = tickRateModifier.GetProperty("IsPartialTick", Public | Static);

        Debug.Assert(isPartialTickInfo is not null);
    }
}
