using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Config.UI;
using ZenSkies.Core.Config.Elements;
using ZenSkies.Core.Utils;

namespace ZenSkies.Common.Config.Elements;

public class SkyGradientElement : GradientElement
{
    #region Drawing

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (Slider is null || !Slider.IsHeld)
            return;

        string tooltip = Utilities.GetReadableTime(Slider.TargetSegment.Position * 24f);

        UIModConfig.Tooltip = tooltip;
    }

    #endregion
}
