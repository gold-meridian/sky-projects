using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using ZenSkies.Core.Utils;

namespace ZenSkies.Core.UI;

public class EasingStyleList : UIPanel
{
    #region Private Fields

    private readonly NestedUIList Easings;

    #endregion

    #region Public Events

    public event Action<EasingStyle>? OnSelected;

    #endregion

    #region Public Constructors

    public EasingStyleList()
        : base(UITextures.EmptyPanel, MiscTextures.Invis)
    {
        SetPadding(6);

        Easings = [];

        Easings.ListPadding = 2f;

        EasingStyle[] styles = Enum.GetValues<EasingStyle>();

        for (int i = 0; i < styles.Length; i++)
        {
            EasingStyleOption button = new(styles[i]);

            button.OnLeftMouseDown += ButtonSelected;

            Easings.Add(button);
        }

        Easings.Width.Set(-25f, 1f);
        Easings.Height.Set(0f, 1f);

        MenuScrollbar scrollbar = new();

        scrollbar.Mute = true;

        scrollbar.SetView(100f, 1000f);

        scrollbar.Top.Set(6f, 0f);

        scrollbar.Height.Set(-12f, 1f);
        scrollbar.Left.Set(-1f, 0f);
        scrollbar.HAlign = 1f;

        Append(scrollbar);

        Easings.SetScrollbar(scrollbar);

        Append(Easings);
    }

    #endregion

    #region Interactions

    private void ButtonSelected(UIMouseEvent evt, UIElement listeningElement)
    {
        if (listeningElement is not EasingStyleOption button)
            return;

        OnSelected?.Invoke(button.Value);
    }

    #endregion
}
