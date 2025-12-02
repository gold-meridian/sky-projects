using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using ZenSkies.Common.Systems.Menu.Elements;
using ZenSkies.Core;
using ZenSkies.Core.Exceptions;

namespace ZenSkies.Common.Systems.Menu.Controllers;

/// <summary>
/// Edits and Hooks:
/// <list type="bullet">
///     <see cref="ChangeParallaxDirection"/><br/>
///     Modifies the speed of the menu parallax.
/// </list>
/// </summary>
public sealed class ParallaxController : SliderController
{
    #region Properties

    public override float MaxRange => 5f;
    public override float MinRange => -5f;

    public override Color InnerColor => Color.CornflowerBlue;

    public override ref float Modifying => ref MenuConfig.Instance.Parallax;

    public override int Index => 2;

    public override string Name => "Parallax";

    #endregion

    #region Loading

    public override void Load() => 
        IL_Main.DrawMenu += ChangeParallaxDirection;

    public override void Unload() => 
        IL_Main.DrawMenu -= ChangeParallaxDirection;

    private void ChangeParallaxDirection(ILContext il)
    {
        try 
        { 
            ILCursor c = new(il);

            c.GotoNext(MoveType.Before,
                i => i.MatchStsfld<Main>(nameof(Main.MenuXMovement)));

            c.EmitPop();

            c.EmitDelegate(() => MenuConfig.Instance.Parallax);
        }
        catch (Exception e)
        {
            throw new ILEditException(il, e);
        }
    }

    #endregion
}
