using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config.UI;
using ZenSkies.Core.DataStructures;
using ZenSkies.Core.UI;
using ZenSkies.Core.Utils;

namespace ZenSkies.Core.Config.Elements;

public class GradientElement : DropDownConfigElement<Gradient>
{
    #region Private Fields

    private const string SliderHoverKey = "Mods.ZenSkies.Configs.GradientSliderHover";

    #endregion

    #region Public Fields

    public GradientSlider? Slider;

    public ColorPicker? Picker;

    #endregion

    #region Public Properties

        // TODO: Better calculation here.
    public override float ExpandedHeight =>
        BaseHeight + 36 + 256f + 52f + 20;

    #endregion

    #region Drop Down

    protected override void OnExpand()
    {
        const float margin = 10;

        #region Slider

        Slider = new(Value);

        Slider.Top.Set(BaseHeight + 5, 0f);

        Slider.HAlign = .5f;

        Slider.Width.Set(-margin * 2, 1f);

            // For whatever reason most -- maybe all(?) -- ConfigElements don't make any sound for hovering, nor clicking.
        Slider.Mute = true;

        Slider.OnSegmentSelected +=
            (s) => Picker?.Color = s.TargetSegment.Color;

        Append(Slider);

        #endregion

        float topMargin = BaseHeight + 5 + Slider.Height.Pixels + 5;

        #region Color Picker

        Picker = new();

        Picker.Top.Set(topMargin, 0f);

        Picker.Left.Set(margin, 0f);

        Picker.Width.Set(-margin, .5f);

        Picker.Mute = true;

        Picker.Color = Slider.TargetSegment.Color;

        Append(Picker);

            // Sketchy but forces the color inputs to use the width of the entire panel.
        Picker.RemoveChild(Picker.Inputs);

        float inputsMargin = Picker.Inputs.Height.Pixels + margin;

        Picker.Inputs.Top.Set(-inputsMargin, 1f);

        Picker.Inputs.HAlign = .5f;

        Picker.Inputs.Width.Set(-margin * 2, 1f);

        Append(Picker.Inputs);

        #endregion

        UIPanel easingPanel =
            new(UITextures.EmptyPanel, MiscTextures.Invis);

        easingPanel.SetPadding(6);

        easingPanel.Top.Set(topMargin, 0f);

        easingPanel.Left.Set(margin, .5f);

        easingPanel.Width.Set(-margin * 2, .5f);

        easingPanel.Height.Set(-topMargin - inputsMargin - margin, 1f);

        easingPanel.BackgroundColor = backgroundColor;

        UIPanel inner =
            new(UITextures.FullPanel, MiscTextures.Invis);

        inner.Width.Set(0f, 1f);

        inner.Height.Set(30, 0f);

        inner.BackgroundColor = backgroundColor;

        easingPanel.Append(inner);

        Append(easingPanel);
    }

    #endregion

    #region Updating

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Slider is null ||
            Picker is null)
            return;

        Slider.TargetSegment.Color = Picker.Color;
    }

    #endregion

    #region Drawing

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (!MenuOpen)
            DrawDisplaySlider(spriteBatch);

        if (Slider is null ||
            Picker is null)
            return;

        if (Slider.IsMouseHovering && !Slider.IsHeld)
        {
            string tooltip = Utilities.GetTextValueWithGlyphs(SliderHoverKey);

            UIModConfig.Tooltip = tooltip;
        }
    }

    protected void DrawDisplaySlider(SpriteBatch spriteBatch)
    {
        Rectangle dims = this.Dimensions;

        IngameOptions.valuePosition = new(dims.X + dims.Width - 10f, dims.Y + 16f);

        Texture2D colorBar = TextureAssets.ColorBar.Value;

        IngameOptions.valuePosition.X -= colorBar.Width;

        Utilities.DrawVanillaSlider(spriteBatch, Color.White, false, out _, out _, out Rectangle inner);

        for (int i = 0; i < inner.Width; i++)
        {
            Rectangle segement = new(inner.X + i, inner.Y, 1, inner.Height);

            Color color = Value.GetColor(i / (float)inner.Width);

            spriteBatch.Draw(MiscTextures.Pixel, segement, color);
        }
    }

    #endregion
}
