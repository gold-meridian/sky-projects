using Terraria;
using ZenSkies.Common.Config;
using ZenSkies.Common.Systems.Menu.Elements;

namespace ZenSkies.Common.Systems.Menu.Controllers;

public sealed class CloudDensityController : SliderController
{
    #region Properties

    public override float MaxRange => 1f;
    public override float MinRange => 0f;

    public override Color InnerColor => Color.LightCyan;

    public override ref float Modifying => ref MenuConfig.Instance.CloudDensity;

    public override int Index => 1;

    public override string Name => "CloudDensity";

    #endregion

    #region Updating

    public override void Refresh()
    {
        int prior = Main.numClouds;

        if (MenuConfig.Instance.UseCloudDensity)
        {
            float density = MenuConfig.Instance.CloudDensity;
            Main.numClouds = (int)(density * Main.maxClouds);

            Main.cloudBGActive = Utils.Remap(density, 0.75f, 1f, 0f, 1f);
        }
        else
            Slider?.Ratio = (float)Main.numClouds / Main.maxClouds;

        if (Main.numClouds != prior)
            Cloud.resetClouds();
    }

    public override void OnSet() =>
        MenuConfig.Instance.UseCloudDensity = true;

    #endregion
}
