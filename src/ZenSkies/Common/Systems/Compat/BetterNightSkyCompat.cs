using BetterNightSky;
using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using static BetterNightSky.BetterNightSky;
using static ZenSkies.Common.Systems.Sky.Space.StarHooks;
using static ZenSkies.Common.Systems.Sky.SunAndMoon.SunAndMoonHooks;

namespace ZenSkies.Common.Systems.Compat;

[ExtendsFromMod("BetterNightSky")]
[Autoload(Side = ModSide.Client)]
public static class BetterNightSkyCompat
{
    private const float big_moon_scale = 4f;

    public static bool IsEnabled { get; private set; }

    public static bool UseBigMoon 
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get => NightConfig.Config.UseHighResMoon;
    }

    [OnLoad]
    private static void Load()
    {
        IsEnabled = true;

        PostDrawStars += PostDrawStars_Special;

        PreDrawMoon += PreDrawMoon_BigMoon;

        On_Main.DrawStarsInBackground -= BetterNightSky.BetterNightSky.On_Main_DrawStarsInBackground;

        // Fix incorrect asset replacement during loading and unloading.
        MonoModHooks.Modify(
            typeof(BetterNightSkySystem).GetMethod(nameof(BetterNightSkySystem.DoUnloads), BindingFlags.Public | BindingFlags.Instance),
            DoUnloads_CorrectAssetReplacement
        );

        if (!SkyConfig.Instance.UseSunAndMoon)
        {
            return;
        }

        MonoModHooks.Modify(
            typeof(BetterNightSkySystem).GetMethod(nameof(BetterNightSkySystem.OnModLoad), BindingFlags.Public | BindingFlags.Instance),
            OnModLoad_CorrectAssetReplacement
        );
    }

    private static void DoUnloads_CorrectAssetReplacement(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel skipLoopTarget = c.DefineLabel();

        // Make sure the reset star type does not try to index out of bounds
        c.TryGotoNext(
            MoveType.After,
            i => i.MatchLdcI4(4)
        );

        c.EmitPop();

        c.EmitLdcI4(3);

        c.TryGotoNext(
            MoveType.After,
            i => i.MatchLdcI4(5)
        );

        c.EmitPop();

        c.EmitLdcI4(4);

        // Correct the moon asset replacement
        if (!SkyConfig.Instance.UseSunAndMoon)
        {
            return;
        }

        c.GotoNext(
            i => i.MatchRet()
        );

        c.GotoPrev(
            MoveType.Before,
            i => i.MatchLdcI4(-1),
            i => i.MatchCall<Star>(nameof(Star.SpawnStars))
        );

        c.MarkLabel(skipLoopTarget);

        c.GotoPrev(
            MoveType.Before,
            i => i.MatchLdcI4(0),
            i => i.MatchStloc(out _),
            i => i.MatchBr(out _)
        );

        c.EmitBr(skipLoopTarget);
    }

    private static void OnModLoad_CorrectAssetReplacement(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<NightConfig>(nameof(NightConfig.Config)),
            i => i.MatchLdfld<NightConfig>(nameof(NightConfig.UseHighResMoon))
        );

        c.EmitPop();

        c.EmitLdcI4(0);
    }

    // TODO: Include other non 'Special' star drawing
    public static void PostDrawStars_Special(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot, float alpha, Matrix transform)
    {
        int i = 0;

        CountStars();
        drawStarPhase = 1;

        var sceneArea = new Main.SceneArea()
        {
            bgTopY = Main.instance.bgTopY,
            totalHeight = Main.screenHeight,
            totalWidth = Main.screenWidth,
            SceneLocalScreenPositionOffset = Vector2.Zero
        };

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearWrap,
            DepthStencilState.None,
            snapshot.RasterizerState,
            RealisticSkySystem.ApplyStarShader(),
            snapshot.TransformMatrix
        );
        {
            foreach (Star star in Main.star.Where(s => s is not null && !s.hidden && SpecialStarType(s) && CanDrawSpecialStar(s)))
            {
                i++;
                Main.instance.DrawStar(ref sceneArea, alpha, Main.ColorOfTheSkies, i, star, false, false);
            }
        }
        spriteBatch.End();
    }

    private static bool PreDrawMoon_BigMoon(
        SpriteBatch spriteBatch,
        ref Asset<Texture2D> moon,
        ref Vector2 position,
        ref Color color,
        ref float rotation,
        ref float scale,
        ref Color moonColor,
        ref Color shadowColor,
        ref bool drawExtras,
        bool eventMoon,
        GraphicsDevice device)
    {
        if (!IsEnabled ||
            !UseBigMoon ||
            eventMoon)
            return true;

        moon = SkyTextures.BetterNightSkyMoon;

        scale *= big_moon_scale;

        drawExtras = false;

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ModifyMoonScale(ref float scale)
    {
        AdjustMoonScaleMethod(ref scale);
    }
}
