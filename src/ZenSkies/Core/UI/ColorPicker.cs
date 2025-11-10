using Microsoft.Xna.Framework;
using Terraria.UI;
using ZenSkies.Core.Utils;

namespace ZenSkies.Core.UI;

public sealed class ColorPicker : UIElement
{
    #region Public Fields

    public bool Mute;

    public readonly ColorSquare Picker;

    public readonly UISlider HueSlider;

    public readonly ColorInputFields Inputs;

    #endregion

    #region Public Properties

    public Color Color
    {
        get => Picker.Color; 
        set 
        { 
            Picker.Color = value;
            HueSlider.Ratio = Utilities.ColorToHSV(value).X;
        }
    }

    public bool IsHeld => Picker.IsHeld || HueSlider.IsHeld;

    #endregion

    #region Constructor

    public ColorPicker(Color? panelColor = null) : base()
    {
        Width.Set(0f, 1f);

        HueSlider = new();

        HueSlider.Top.Set(-40f, 1f);

        HueSlider.InnerTexture = MiscTextures.HueGradient;
        HueSlider.InnerColor = Color.White;

        Append(HueSlider);

        Picker = new();

        Append(Picker);

        Inputs = new(this, panelColor);

        Inputs.Top.Set(-16f, 1f);

        Append(Inputs);
    }

    #endregion

    #region Updating

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Picker.Hue = HueSlider.Ratio;

        Picker.Mute = Mute;
        HueSlider.Mute = Mute;
    }

    public override void Recalculate()
    {
        base.Recalculate();

        float width = GetDimensions().Width;

        Height.Set(width + 52f, 0f);
    }

    #endregion
}
