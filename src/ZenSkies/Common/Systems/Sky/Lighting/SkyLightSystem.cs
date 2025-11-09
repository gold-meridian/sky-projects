using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using ZenSkies.Common.DataStructures;
using ZenSkies.Core.Utils;

namespace ZenSkies.Common.Systems.Sky.Lighting;

public static class SkyLightSystem
{
    #region Public Properties

    public static List<SkyLight> Lights { get; }
        = [];

    #endregion

    #region Public Methods

    public static void InvokeForActiveLights(Action<SkyLightInfo> action)
    {
        foreach (SkyLight light in Lights)
        {
            SkyLightInfo info = light.GetInfo();

            Color color = info.Color;

            if (light.Active &&
                (color.R > 0 ||
                color.G > 0 ||
                color.B > 0))
                action(light.GetInfo());
        }
    }

    #endregion
}
