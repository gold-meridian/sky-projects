using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using RealisticSky;
using RealisticSky.Common.DataStructures;
using RealisticSky.Content;
using RealisticSky.Content.Atmosphere;
using RealisticSky.Content.Clouds;
using RealisticSky.Content.NightSky;
using RealisticSky.Content.Sun;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using ZenSkies.Common.DataStructures;
using ZenSkies.Common.Systems.Sky;
using ZenSkies.Common.Systems.Sky.Space;
using ZenSkies.Core.Utils;
using static ZenSkies.Common.Systems.Sky.Space.StarHooks;
using static ZenSkies.Common.Systems.Sky.SunAndMoonHooks;

namespace ZenSkies.Common.Systems.Compat;

// Likely obsolete assuming the remake replaces RealisticSky
[ExtendsFromMod("RealisticSky")]
[Autoload(Side = ModSide.Client)]
public static class RealisticSkySystem
{
    public static bool CanDraw
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return
                RealisticSkyManager.CanRender &&
                !RealisticSkyManager.TemporarilyDisabled &&
                (RealisticSkyConfig.Instance.ShowInMainMenu || !Main.gameMenu);
        }
    }

    public static bool IsEnabled { get; private set; }

    [OnLoad]
    private static void Load()
    {
        IsEnabled = true;

        PreDrawStars += PreDrawStars_RealisticSkyStars;
        PostDrawStars += PostDrawStars_RealisticSkyGalaxy;

        PreDrawSun += PreDrawSun_RealisticSky;

        OnUpdateSunAndMoonInfo += OnUpdateSunAndMoonInfo_RealisticSky;

        // Remove the positional changes to the sun and moon.
        MonoModHooks.Add(
            typeof(SunPositionSaver).GetMethod(nameof(SunPositionSaver.VerticallyBiasSunAndMoon), BindingFlags.Public | BindingFlags.Static),
            (Action _) => { }
        );

        // Various patches for inverted gravity with several visuals
        MonoModHooks.Modify(
            typeof(AtmosphereTargetContent).GetMethod("HandleUseReqest", BindingFlags.NonPublic | BindingFlags.Instance),
            HandleUseReqest_InvertedGravity
        );

        MonoModHooks.Modify(
            typeof(CloudsTargetContent).GetMethod("HandleUseReqest", BindingFlags.NonPublic | BindingFlags.Instance),
            HandleUseReqest_InvertedGravity
        );

        MonoModHooks.Modify(
            typeof(CloudsRenderer).GetMethod(nameof(CloudsRenderer.RenderToTarget), BindingFlags.Public | BindingFlags.Static),
            RenderToTarget_InvertedGravity
        );

        MonoModHooks.Modify(
            typeof(StarsRenderer).GetMethod(nameof(StarsRenderer.Render), BindingFlags.Public | BindingFlags.Static),
            Render_SunPosition
        );

        // Bulk edit for a bunch of misc drawing.
        MonoModHooks.Modify(
            typeof(RealisticSkyManager).GetMethod(nameof(RealisticSkyManager.Draw), BindingFlags.Public | BindingFlags.Instance),
            Draw_
        );
    }

    #region Inverted Gravity

    private static void HandleUseReqest_InvertedGravity(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld<Main>(nameof(Main.Rasterizer))
        );

        c.EmitPop();

        c.EmitDelegate(static () => RasterizerState.CullNone);
    }

    private static void RenderToTarget_InvertedGravity(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<SkyPlayerSnapshot>($"get_{nameof(SkyPlayerSnapshot.InvertedGravity)}")
        );

        c.EmitPop();

        c.EmitLdcI4(0);
    }

    #endregion

    private static void Render_SunPosition(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchBr(out _),
            i => i.MatchCall<SunPositionSaver>($"get_{nameof(SunPositionSaver.SunPosition)}")
        );

        c.EmitDelegate(static (Vector2 sunPosition) =>
        {
            if (Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically))
            {
                sunPosition.Y = Utilities.ScreenSize.Y - sunPosition.Y;
            }

            return sunPosition;
        });
    }

    private static void Draw_(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel galaxySkipTarget = c.DefineLabel();
        ILLabel starSkipTarget = c.DefineLabel();
        ILLabel? jumpSunRendering = null;

        for (int i = 0; i < 3; i++)
        {
            c.GotoNext(
                MoveType.After,
                i => i.MatchLdnull(),
                i => i.MatchLdloc0()
            );

            c.EmitPop();

            if (i == 0)
            {
                c.EmitDelegate(static () => Matrix.Identity);
            }
            else
            {
                c.EmitDelegate(static () => Main.BackgroundViewMatrix.EffectMatrix);
            }
        }

        c.Index = 0;

        // Skip galaxy rendering since we draw it manually
        {
            c.GotoNext(
                MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch)),
                i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.End))
            );

            c.EmitBr(galaxySkipTarget);

            c.GotoNext(
                MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch)),
                i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.End))
            );

            c.MarkLabel(galaxySkipTarget);

            c.GotoNext(MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.Rasterizer)));

            c.EmitPop();

            c.EmitDelegate(static () => RasterizerState.CullNone);
        }

        // Ditto for stars
        {
            c.GotoNext(
                MoveType.Before,
                i => i.MatchNop(),
                i => i.MatchLdsfld(typeof(RealisticSkyManager), nameof(RealisticSkyManager.Opacity)),
                i => i.MatchLdloc0(),
                i => i.MatchCall<StarsRenderer>(nameof(StarsRenderer.Render))
            );

            c.EmitBr(starSkipTarget);

            c.GotoNext(
                MoveType.After,
                i => i.MatchNop(),
                i => i.MatchLdsfld(typeof(RealisticSkyManager), nameof(RealisticSkyManager.Opacity)),
                i => i.MatchLdloc0(),
                i => i.MatchCall<StarsRenderer>(nameof(StarsRenderer.Render))
            );

            c.MarkLabel(starSkipTarget);
        }

        // Ditto for the sun
        {
            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdloc(out _),
                i => i.MatchBrfalse(out jumpSunRendering),
                i => i.MatchNop(),
                i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch)),
                i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.End))
            );

            Debug.Assert(jumpSunRendering is not null);

            c.EmitBr(jumpSunRendering);
        }

        // Correct matrix for clouds
        {
            c.GotoNext(
                MoveType.After,
                i => i.MatchLdnull(),
                i => i.MatchCall<Matrix>($"get_{nameof(Matrix.Identity)}")
            );

            c.EmitPop();
            c.EmitDelegate(static () => Main.BackgroundViewMatrix.EffectMatrix);
        }
    }

    /// <summary>
    /// Apply a star masking shader if <see cref="RealisticSky"/> is enabled and is active.
    /// </summary>
    public static Effect? ApplyStarShader()
    {
        if (!IsEnabled)
            return null;

        if (!CanDraw)
            return null;

        Effect star = CompatEffects.StarAtmosphere.Value;

        if (!CompatEffects.StarAtmosphere.IsReady)
            return null;

        SetAtmosphereParams(star);

        star.CurrentTechnique.Passes[0].Apply();

        return star;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetAtmosphereParams(Effect shader)
    {
        shader.Parameters["usesAtmosphere"]?.SetValue(true);

        shader.Parameters["screenSize"]?.SetValue(Utilities.ScreenSize);
        shader.Parameters["distanceFadeoff"]?.SetValue(Main.eclipse ? 0.11f : 1f);

        Vector2 sunPosition = SunAndMoon.Info.SunPosition;

        if (Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically))
            sunPosition.Y = Utilities.ScreenSize.Y - sunPosition.Y;

        shader.Parameters["sunPosition"]?.SetValue(Main.dayTime ? sunPosition : (Vector2.One * 50000f));

        if (AtmosphereRenderer.AtmosphereTarget?.IsReady ?? false)
            Main.instance.GraphicsDevice.Textures[1] = AtmosphereRenderer.AtmosphereTarget.GetTarget();
        else
            Main.instance.GraphicsDevice.Textures[1] = MiscTextures.Invis;

    }

    public static bool PreDrawStars_RealisticSkyStars(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot, ref float alpha, ref Matrix transform)
    {
        if (!SkyConfig.Instance.DrawRealisticStars || !CanDraw)
        {
            return true;
        }

        StarsRenderer.Render(StarSystem.StarAlpha, Matrix.Identity);

        return true;
    }

    public static void PostDrawStars_RealisticSkyGalaxy(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot, float alpha, Matrix transform)
    {
        if (!SkyConfig.Instance.DrawRealisticStars || !CanDraw)
        {
            return;
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, snapshot.RasterizerState, ApplyStarShader(), transform);

        GalaxyRenderer.Render();

        spriteBatch.End();
    }

    public static bool PreDrawSun_RealisticSky(
        SpriteBatch spriteBatch,
        ref Vector2 position,
        ref Color color,
        ref float rotation,
        ref float scale,
        GraphicsDevice device
    )
    {
        if (!CanDraw || !SkyConfig.Instance.RealisticSun)
        {
            return true;
        }

        SunRenderer.Render(1f - RealisticSkyManager.SunlightIntensityByTime);

        return false;
    }

    public static void OnUpdateSunAndMoonInfo_RealisticSky(SunAndMoonInfo info)
    {
        SunPositionSaver.SunPosition = info.SunPosition;
        SunPositionSaver.MoonPosition = info.MoonPosition;
    }
}
