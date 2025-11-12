using Microsoft.Xna.Framework;
using Terraria;
using ZenSkies.Core.UI;

namespace ZenSkies.Common.Systems.Menu.Elements;

public abstract class SliderController : MenuController
{
    #region Private Fields

    protected const float DefaultHeight = 75f;

    #endregion

    #region Public Fields

    public UISlider? Slider;

    #endregion

    #region Public Properties

    public abstract float MaxRange { get; }
    public abstract float MinRange { get; }

    public abstract Color InnerColor { get; }

    public abstract ref float Modifying {  get; }

    #endregion

    #region Initialization

    public override void OnInitialize()
    {
        base.OnInitialize();

        Height.Set(DefaultHeight, 0f);

        Slider = new();

        Slider.Top.Set(30f, 0f);

        Slider.InnerColor = InnerColor;

        Append(Slider);
    }

    #endregion

    #region Updating

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Slider is null)
            return;

        if (Slider.IsHeld)
        {
            Modifying = Utils.Remap(Slider.Ratio, 0, 1, MinRange, MaxRange);

            OnSet();
            Refresh();
        }
        else
            Slider.Ratio = Utils.Remap(Modifying, MinRange, MaxRange, 0, 1);
    }

    #endregion

    public virtual void OnSet() { }
}
