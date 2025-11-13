using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using Terraria.UI.Chat;
using ZenSkies.Common.Config;
using ZenSkies.Common.Systems.Compat;
using ZenSkies.Common.Systems.Menu.Elements;
using ZenSkies.Core;
using ZenSkies.Core.Utils;
using ZenSkies.Core.Exceptions;
using static System.Reflection.BindingFlags;
using Terraria.Initializers;

namespace ZenSkies.Common.Systems.Menu;

/// <summary>
/// TODO: Not use a ModConfig for storing this data.<br/><br/>
/// 
/// Edits and Hooks:
/// <list type="bullet">
///     <see cref="AddToggle"/><br/>
///     Adds a small toggle in the style of a dropdown (▼ : ▲) to the main menu, on the right side of the button to change menu styles.
///     <item/>
///     <see cref="ModifyInteraction"/><br/>
///     Disables interactions with main menu buttons while hovering the controller interface.
///     <item/>
///     <see cref="UpdateInterface"/><br/>
///     Hook to have our interface update on the menu, seeing as <see cref="ModSystem.UpdateUI"/> doesn't work on the main menu.
///     <item/>
///     <see cref="EscapeControllerUI"/><br/>
///     Sets the interface's state to null on a 'back button command,' without having to use vanilla's menu interface.
///     <item/>
///     <see cref="CloseMenuOnResolutionChanged"/><br/>
///     Self-explanitory.
///     <item/>
///     <see cref="RefreshOnSave"/><br/>
///     Likely redunant but refreshes controllers when the config is saved.
/// </list>
/// </summary>
[Autoload(Side = ModSide.Client)]
public sealed class MenuControllerSystem : ModSystem
{
    #region Private Fields

    private static readonly Color NotHovered = new(120, 120, 120, 76);
    private const int HorizontalPadding = 4;

    private static ILHook? PatchUpdateAndDrawModMenuInner;

    private delegate void orig_Save(ModConfig config);
    private static Hook? PatchSaveConfig;

    private static readonly UserInterface MenuControllerInterface = new();

    #endregion

    #region Public Fields

    public static readonly int[] AllowedMenuModes =
        [0, CoolerMenuSystem.CoolerMenuID];

    public static readonly List<MenuController> Controllers = [];

    public static readonly MenuControllerState MenuControllerState = new();

    #endregion

    #region Public Properties

    public static UIState? State
    {
        get => MenuControllerInterface.CurrentState;
        set
        {
            MenuControllerInterface.SetState(value);
            value?.OnInitialize();

            if (value is null)
                ConfigManager.Save(MenuConfig.Instance);
        }
    }

    public static bool InUI => MenuControllerInterface?.CurrentState is not null;

    public static bool Hovering => InUI && MenuControllerState?.Panel?.IsMouseHovering is true;

    #endregion

    #region Loading

    public override void Load()
    {
        MainThreadSystem.Enqueue(() =>
        {
            MethodInfo? updateAndDrawModMenuInner = typeof(MenuLoader).GetMethod(nameof(MenuLoader.UpdateAndDrawModMenuInner), Static | NonPublic);

            if (updateAndDrawModMenuInner is not null)
                PatchUpdateAndDrawModMenuInner = new(updateAndDrawModMenuInner, 
                    AddToggle);

            IL_Main.DrawMenu += ModifyInteraction;
            On_Main.UpdateUIStates += UpdateInterface;

            On_UILinksInitializer.FancyExit += EscapeControllerUI;

            Main.OnResolutionChanged += CloseMenuOnResolutionChanged;
        });

        MethodInfo? save = typeof(ConfigManager).GetMethod(nameof(ConfigManager.Save), Static | NonPublic);

        if (save is not null)
            PatchSaveConfig = new(save,
                RefreshOnSave);

        MenuControllerState?.Activate();
    }

    public override void Unload()
    {
        MainThreadSystem.Enqueue(() =>
        {
            PatchUpdateAndDrawModMenuInner?.Dispose();

            PatchSaveConfig?.Dispose();

            IL_Main.DrawMenu -= ModifyInteraction;
            On_Main.UpdateUIStates -= UpdateInterface;

            On_UILinksInitializer.FancyExit -= EscapeControllerUI;

            Main.OnResolutionChanged -= CloseMenuOnResolutionChanged;
        });

        PatchSaveConfig?.Dispose();
    }

    public override void PostSetupContent() =>
        RefreshAll();

    #endregion

    #region Public Methods

    public static void RefreshAll() =>
        Controllers.ForEach((controller) => { controller.Refresh(); });

    #endregion

    #region Menu Additions

    private void AddToggle(ILContext il)
    {
        try
        {
            ILCursor c = new(il);

            int switchTextRectIndex = -1;

            ILLabel? jumpInteractionsTarget = c.DefineLabel();

                // Match to before the menu switch text is drawn.
            c.GotoNext(MoveType.Before,
                i => i.MatchLdloca(out switchTextRectIndex),
                i => i.MatchLdsfld<Main>(nameof(Main.mouseX)),
                i => i.MatchLdsfld<Main>(nameof(Main.mouseY)),
                i => i.MatchCall<Rectangle>(nameof(Rectangle.Contains)),
                i => i.MatchBrfalse(out jumpInteractionsTarget));

            c.EmitDelegate(() => Hovering);

            c.EmitBrtrue(jumpInteractionsTarget);

                // Match to before the menu switch text is drawn.
            c.GotoNext(MoveType.After,
                i => i.MatchCall(typeof(MenuLoader).FullName ?? "Terraria.ModLoader.MenuLoader", nameof(MenuLoader.OffsetModMenu)),
                i => i.MatchLdsfld<Main>(nameof(Main.menuMode)),
                i => i.MatchBrtrue(out _));

            c.EmitLdloc(switchTextRectIndex);

                // Add our dropdown menu button.
            c.EmitCall(DrawToggle);
        }
        catch (Exception e)
        {
            throw new ILEditException(Mod, il, e);
        }
    }

    public static void DrawToggle(Rectangle switchTextRect)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        Vector2 position = switchTextRect.TopRight();
        position.X += HorizontalPadding;

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        string text = InUI ? "▼" : "▲";

        Vector2 size = ChatManager.GetStringSize(font, text, Vector2.One);

        Rectangle popupRect = new((int)position.X, (int)position.Y,
            (int)size.X, (int)size.Y);

        bool hovering = popupRect.Contains(Main.mouseX, Main.mouseY) && !Main.alreadyGrabbingSunOrMoon;

        Color color = hovering ? Main.OurFavoriteColor : NotHovered;

        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, position, color, 0f, Vector2.Zero, Vector2.One);

        if (hovering && Main.mouseLeft && Main.mouseLeftRelease)
        {
            MenuControllerState.Bottom = new(popupRect.Center.X, position.Y);

            State = InUI ? null : MenuControllerState;

            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    private void ModifyInteraction(ILContext il)
    {
        try
        {
            ILCursor c = new(il);

                // TODO: Match for something better.
            c.GotoNext(i => i.MatchLdloc(173));

                // Genuinely I can't.
            string[] names = [nameof(Main.focusMenu), nameof(Main.selectedMenu), nameof(Main.selectedMenu2)];
            for (int j = 0; j < names.Length * 2; j++)
            {
                if (c.TryGotoNext(MoveType.Before, i => i.MatchStfld<Main>(names[j % names.Length])))
                    c.EmitDelegate((int hovering) => Hovering ? -1 : hovering);
            }

                // Have our popup draw.
            c.GotoNext(MoveType.After,
                i => i.MatchLdloc(out _),
                i => i.MatchLdloc(out _),
                i => i.MatchCall<Main>(nameof(Main.DrawtModLoaderSocialMediaButtons)));

            c.MoveAfterLabels();
            
            c.EmitDelegate(() =>
            {
                if (InUI &&
                    AllowedMenuModes.Contains(Main.menuMode))
                    MenuControllerInterface?.Draw(Main.spriteBatch, new GameTime());
            });
        }
        catch (Exception e)
        {
            throw new ILEditException(Mod, il, e);
        }
    }

    #endregion

    #region Updating

    private static void UpdateInterface(On_Main.orig_UpdateUIStates orig, GameTime gameTime)
    {
        if (InUI)
        {
            if (AllowedMenuModes.Contains(Main.menuMode))
                MenuControllerInterface?.Update(gameTime);
            else
                State = null;
        }

        orig(gameTime);
    }

    #endregion

    #region FancyExit

    private static void EscapeControllerUI(On_UILinksInitializer.orig_FancyExit orig)
    {
        orig();

            // Should not exit interface while typing.
        if (State is null ||
            Input.WasWritingText)
            return;

        State = null;

        SoundEngine.PlaySound(in SoundID.MenuClose);
    }

    #endregion

    #region Config

    private void RefreshOnSave(orig_Save orig, ModConfig config)
    {
        orig(config);

        if (config is MenuConfig)
            RefreshAll();
    }

    private void CloseMenuOnResolutionChanged(Vector2 obj) =>
        State = null;

    public override void OnWorldUnload() =>
        RefreshAll();

    #endregion
}
