using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using ZenSkies.Core.Config.Elements;
using ZenSkies.Core.UI;
using ZenSkies.Core.Utils;

namespace ZenSkies.Common.Config.Elements;

internal class SkyGradientElement : GradientElement
{
    protected override void OnExpand()
    {
        base.OnExpand();

        Slider?.OnUpdate += UpdateSlider;
    }

    private void UpdateSlider(UIElement affectedElement)
    {
        if (affectedElement is not GradientSlider slider
            || !slider.IsHeld)
        {
            return;
        }

        string tooltip = Utilities.GetReadableTime(slider.TargetSegment.Position * 24f);

        UIModConfig.Tooltip = tooltip;
    }
}
