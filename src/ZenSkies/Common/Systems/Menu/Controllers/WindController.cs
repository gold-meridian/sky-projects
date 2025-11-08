using Terraria;
using ZenSkies.Common.Config;
using ZenSkies.Common.Systems.Menu.Elements;

namespace ZenSkies.Common.Systems.Menu.Controllers;

public sealed class WindController : SliderController
{
    #region Properties

    public override float MaxRange => 1f;
    public override float MinRange => -1f;

    public override Color InnerColor => Color.Gray;

    public override ref float Modifying => ref MenuConfig.Instance.Wind;

    public override int Index => 3;

    public override string Name => "Wind";

    #endregion

    #region Updating

    public override void Refresh() 
    {
        if (!MenuConfig.Instance.UseWind)
            return;

        Main.windSpeedCurrent = MenuConfig.Instance.Wind;
        Main.windSpeedTarget = MenuConfig.Instance.Wind;
    }

    public override void OnSet() =>
        MenuConfig.Instance.UseWind = true;

    #endregion
}
