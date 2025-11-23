using Microsoft.Xna.Framework;
using MonoMod.Cil;
using ReLogic.OS;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using ZenSkies.Common.Config;
using ZenSkies.Common.Systems.Menu.Elements;
using ZenSkies.Core;
using ZenSkies.Core.Exceptions;
using ZenSkies.Core.UI;
using ZenSkies.Core.Utils;

#nullable disable

namespace ZenSkies.Common.Systems.Menu.Controllers;

/// <summary>
/// Edits and Hooks:
/// <list type="bullet">
///     <see cref="ModifyColors"/><br/>
///     Modifies the color of the main menu buttons.
/// </list>
/// </summary>
public sealed class ButtonColorController : MenuController
{
    #region Private Fields

    private const float DefaultHeight = 75f;

    private ColorPicker Picker;

    private UITextPanel<LocalizedText> ColorButton;
    private UITextPanel<LocalizedText> HoverColorButton;

    private static bool SettingHover;

    private const string ColorButtonNameKey = "ColorButtonName";

    private const string HoverColorButtonNameKey = "HoverColorButtonName";

    private const string CopyKey = "Mods.ZenSkies.Copy";
    private const string PasteKey = "Mods.ZenSkies.Paste";
    private const string ResetKey = "Mods.ZenSkies.Reset";

    #endregion

    #region Public Fields

    public static readonly Color DefaultColor = Color.Gray;
    public static readonly Color DefaultHover = new(255, 215, 0);

    #endregion

    #region Private Properties

    private LocalizedText ColorButtonName =>
        this.GetLocalization(ColorButtonNameKey);

    private LocalizedText HoverColorButtonName =>
        this.GetLocalization(HoverColorButtonNameKey);

    private static ref Color Modifying =>
        ref SettingHover ? ref MenuConfig.Instance.MenuButtonHoverColor : ref MenuConfig.Instance.MenuButtonColor;

    private static ref bool ModifyingUse =>
        ref SettingHover ? ref MenuConfig.Instance.UseMenuButtonHoverColor : ref MenuConfig.Instance.UseMenuButtonColor;

    private bool ShowPicker
    {
        get => field;
        set
        {
            field = value;

            if (value && !Elements.Contains(Picker))
            {
                Append(Picker);

                Recalculate();

                MenuControllerSystem.MenuControllerState.Controllers?.Recalculate();

                float height = Height.Pixels - DefaultHeight;

                MenuControllerSystem.MenuControllerState.Controllers?.ViewPosition += height;
            }
            else if (!value &&
                Picker is not null)
            {
                RemoveChild(Picker);

                float height = Height.Pixels - DefaultHeight;
                MenuControllerSystem.MenuControllerState.Controllers?.ViewPosition -= height;

                Recalculate();
                MenuControllerSystem.MenuControllerState.Controllers?.Recalculate();
            }
        }
    }

    #endregion

    #region Public Properties

    public override int Index => 7;

    public override string Name => "ButtonColor";

    public static Color ButtonColor { get; set; }

    public static Color ButtonHoverColor { get; set; }

    #endregion

    #region Initalization

    public override void OnInitialize()
    {
        base.OnInitialize();

        ShowPicker = false;

        Height.Set(DefaultHeight, 0f);

        Picker = new();

        Picker.Top.Set(64f, 0f);

        Picker.Inputs.OnAcceptInput += (p) =>
        {
            ModifyingUse = true;
            Modifying = p.Color;

            Refresh();
        };

        ColorButton = CreateColorButton(false);
        HoverColorButton = CreateColorButton(true);
    }

    #region Buttons

    private UITextPanel<LocalizedText> CreateColorButton(bool isHover)
    {
        UITextPanel<LocalizedText> button = new(isHover ? HoverColorButtonName : ColorButtonName);

        button.SetPadding(6f);

        // Reset the text with the new padding.
        button.SetText(button._text);

        button.Top.Set(30f, 0f);

        button.Left.Set(isHover ? -button.MinWidth.Pixels : 66f, isHover ? 1f : 0f);

        button._backgroundTexture = UITextures.FullPanel;
        button._borderTexture = MiscTextures.Invis;

        button.BackgroundColor = UICommon.DefaultUIBlue;

        button.TextColor = isHover ? ButtonHoverColor : ButtonColor;

        button.OnLeftMouseDown +=
            (evt, listeningElement) => ShowColor(isHover);

        button.OnUpdate +=
            affectedElement => UpdateButton(affectedElement, isHover);

        button.OnMouseOver +=
            (evt, listeningElement) => SoundEngine.PlaySound(SoundID.MenuTick);

        Append(button);

        const float buttonPadding = 2f;
        const float buttonSize = 20f;
        const float buttonTop = 35f;

        #region Copy/Paste

        MenuImageButton copyButton = new(ButtonTextures.Copy);

        copyButton.Top.Set(buttonTop, 0f);

        copyButton.Width.Set(buttonSize, 0f);
        copyButton.Height.Set(buttonSize, 0f);

        copyButton.Left.Set(isHover ? (-button.MinWidth.Pixels - ((buttonSize + buttonPadding) * 3)) : 0f, isHover ? 1f : 0f);

        copyButton.OnLeftClick +=
            (evt, listeningElement) => CopyColor(isHover);

        copyButton.OnUpdate +=
            affectedElement => MenuControllerState.HoverTooltip(affectedElement, CopyKey);

        Append(copyButton);

        MenuImageButton pasteButton = new(ButtonTextures.Paste);

        pasteButton.Top.Set(buttonTop, 0f);

        pasteButton.Width.Set(buttonSize, 0f);
        pasteButton.Height.Set(buttonSize, 0f);

        pasteButton.Left.Set(isHover ? (-button.MinWidth.Pixels - ((buttonSize + buttonPadding) * 2)) : buttonSize + buttonPadding, isHover ? 1f : 0f);

        pasteButton.OnLeftClick +=
            (evt, listeningElement) => PasteColor(isHover);

        pasteButton.OnUpdate +=
            affectedElement => MenuControllerState.HoverTooltip(affectedElement, PasteKey);

        Append(pasteButton);

        #endregion

        #region Reset

        MenuImageButton resetButton = new(ButtonTextures.Reset);

        resetButton.Top.Set(buttonTop, 0f);

        resetButton.Width.Set(buttonSize, 0f);
        resetButton.Height.Set(buttonSize, 0f);

        resetButton.Left.Set(isHover ? (-button.MinWidth.Pixels - buttonSize - buttonPadding) : ((buttonSize + buttonPadding) * 2), isHover ? 1f : 0f);

        resetButton.OnLeftClick +=
            (evt, listeningElement) => ResetColor(isHover);

        resetButton.OnUpdate +=
            affectedElement => MenuControllerState.HoverTooltip(affectedElement, ResetKey);

        Append(resetButton);

        #endregion

        return button;
    }

    private void ShowColor(bool isHover)
    {
        if (SettingHover == isHover)
            ShowPicker = !ShowPicker;
        else
            ShowPicker = true;

        SettingHover = isHover;

        Picker.Color = Modifying;

        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    private void UpdateButton(UIElement element, bool isHover)
    {
        if (element is not UIPanel panel)
            return;

        if (panel.IsMouseHovering ||
            (SettingHover == isHover &&
            ShowPicker))
            panel.BackgroundColor = UICommon.DefaultUIBlue;
        else
            panel.BackgroundColor = UICommon.DefaultUIBlueMouseOver;
    }

    #region Copy/Paste

    private static void CopyColor(bool isHover)
    {
        SettingHover = isHover;

        if (!ModifyingUse)
            return;

        Platform.Get<IClipboard>().Value = Utils.Hex3(Modifying);

        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    private void PasteColor(bool isHover)
    {
        SettingHover = isHover;

        if (!ModifyingUse)
            return;

        string hex = Platform.Get<IClipboard>().Value;

        Color color = Utilities.FromHex3(hex);

        if (color.A < byte.MaxValue)
            return;

        ModifyingUse = true;
        Modifying = color;

        Refresh();

        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    #endregion

    #region Reset

    private void ResetColor(bool isHover)
    {
        SettingHover = isHover;

        ModifyingUse = false;
        Modifying = Color.Red;

        Refresh();

        SoundEngine.PlaySound(in SoundID.MenuOpen);
    }

    #endregion

    #endregion

    #endregion

    #region Loading

    public override void Load()
    {
        MainThreadSystem.Enqueue(() =>
            IL_Main.DrawMenu += ModifyColors);

        _ = ColorButtonName;

        _ = HoverColorButtonName;
    }

    public override void Unload() => 
        MainThreadSystem.Enqueue(() => IL_Main.DrawMenu -= ModifyColors);

    private void ModifyColors(ILContext il)
    {
        try
        {
            ILCursor c = new(il);

            int colorIndex = -1;

            int rIndex = -1;
            int gIndex = -1;
            int bIndex = -1;
            int aIndex = -1;

            int hoveredIndex = -1;
            int outerIteratorIndex = -1;
            int innerIteratorIndex = -1;

            int interpolatorIndex = -1;

            ILLabel jumpColorCtorTarget = c.DefineLabel();

            for (int i = 0; i < 5; i++)
            {
                    // Grab relevant color indices.
                c.GotoNext(MoveType.After,
                    i => i.MatchLdloca(out colorIndex),
                    i => i.MatchLdloc(out rIndex),
                    i => i.MatchConvU1(),
                    i => i.MatchLdloc(out gIndex),
                    i => i.MatchConvU1(),
                    i => i.MatchLdloc(out bIndex),
                    i => i.MatchConvU1(),
                    i => i.MatchLdloc(out aIndex),
                    i => i.MatchConvU1(),
                    i => i.MatchCall<Color>(".ctor"));

                if (i == 4)
                    break;

                c.EmitLdloca(colorIndex);

                c.EmitLdloc(rIndex);
                c.EmitLdloc(gIndex);
                c.EmitLdloc(bIndex);
                c.EmitLdloc(aIndex);

                c.EmitLdcR4(0f);

                c.EmitCall(ModifyColorRGBA);

                c.EmitPop();
            }

                // Mark this label so we can skip this ctor later.
            c.MarkLabel(jumpColorCtorTarget);

                // Grab the inner iterator to check if were drawing the colored text and not the shadow.
            c.GotoNext(MoveType.After,
                i => i.MatchLdloc(out innerIteratorIndex),
                i => i.MatchLdcI4(4),
                i => i.MatchBneUn(out _));

                // Insert our stuff before the game handles hover color.
            c.GotoPrev(MoveType.Before,
                i => i.MatchLdloc(out hoveredIndex),
                i => i.MatchLdloc(out outerIteratorIndex),
                i => i.MatchBneUn(out _),
                i => i.MatchLdloc(out _),
                i => i.MatchLdcI4(4),
                i => i.MatchBneUn(out _),
                i => i.MatchLdloc(out interpolatorIndex));

            c.MoveAfterLabels();

            c.EmitLdloca(colorIndex);

            c.EmitLdloc(innerIteratorIndex);

            c.EmitLdloc(rIndex);
            c.EmitLdloc(gIndex);
            c.EmitLdloc(bIndex);
            c.EmitLdloc(aIndex);

            c.EmitLdloc(interpolatorIndex);

            c.EmitLdloc(hoveredIndex);
            c.EmitLdloc(outerIteratorIndex);

            c.EmitDelegate((ref Color color, int i, int r, int g, int b, int a, int interpolator, int hovered, int j) =>
            {
                if (i != 4)
                    return false;

                return ModifyColorRGBA(ref color, r, g, b, a, hovered == j ? interpolator / 255f : 0);
            });

            c.EmitBrtrue(jumpColorCtorTarget);
        }
        catch (Exception e)
        {
            throw new ILEditException(ModContent.GetInstance<ZenSkies>(), il, e);
        }
    }

    private static bool ModifyColorRGBA(ref Color color, int r, int g, int b, int a, float interpolator)
        => ModifyColor(ref color, new(r, g, b, a), interpolator);

    public static bool ModifyColor(ref Color color, Color baseColor, float interpolator)
    {
        MenuConfig config = MenuConfig.Instance;

        if (!config.UseMenuButtonColor)
            ButtonColor = DefaultColor;
        if (!config.UseMenuButtonHoverColor)
            ButtonHoverColor = DefaultHover;

        if (!config.UseMenuButtonColor && !config.UseMenuButtonHoverColor)
            return false;

        Color normalColor = config.UseMenuButtonColor ? ButtonColor : baseColor;
        Color hoverColor = ButtonHoverColor;

        color = Color.Lerp(normalColor, hoverColor, interpolator);

        return true;
    }

    #endregion

    #region Updating

    public override void Refresh()
    {
        MenuConfig config = MenuConfig.Instance;

        if (config.UseMenuButtonColor)
            ButtonColor = config.MenuButtonColor;

        if (config.UseMenuButtonHoverColor)
            ButtonHoverColor = config.MenuButtonHoverColor;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Picker.IsHeld)
        {
            ModifyingUse = true;
            Modifying = Picker.Color;

            Refresh();
        }

        ColorButton.TextColor = ButtonColor;
        HoverColorButton.TextColor = ButtonHoverColor;
    }

    public override void Recalculate()
    {
        base.Recalculate();

        const float margin = 12f;

        const float expandedMargin = 22f;

        if (!ShowPicker)
            Height.Set(DefaultHeight + margin, 0f);
        else
            Height.Set(DefaultHeight + Picker.Dimensions.Height + expandedMargin, 0f);
    }

    #endregion
}
