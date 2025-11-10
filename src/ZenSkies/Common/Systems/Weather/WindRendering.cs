using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Config;
using ZenSkies.Core;

namespace ZenSkies.Common.Systems.Weather;

public static class WindRendering
{
    #region Loading

    [OnLoad(Side = ModSide.Client)]
    public static void Load() 
    {
        MainThreadSystem.Enqueue(() =>
            On_Main.DrawBackgroundBlackFill += MenuDraw);

        On_Main.DrawInfernoRings += InGameDraw;
    }

    [OnUnload(Side = ModSide.Client)]
    public static void Unload()
    {
        MainThreadSystem.Enqueue(() =>
            On_Main.DrawBackgroundBlackFill -= MenuDraw);

        On_Main.DrawInfernoRings -= InGameDraw;
    }

    private static void MenuDraw(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
    {
        orig(self);

        if (!ZenSkies.CanDrawSky ||
            !Main.gameMenu ||
            !SkyConfig.Instance.UseWindParticles ||
            SkyConfig.Instance.WindOpacity <= 0)
            return;

        Draw();
    }

    private static void InGameDraw(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);

        if (!ZenSkies.CanDrawSky ||
            Main.gameMenu ||
            !SkyConfig.Instance.UseWindParticles ||
            SkyConfig.Instance.WindOpacity <= 0)
            return;

        Draw();
    }

    #endregion

    #region Drawing

    private static void Draw()
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        GraphicsDevice device = Main.graphics.GraphicsDevice;

        if (SkyConfig.Instance.UsePixelatedSky)
            DrawPixelated(spriteBatch, device);
        else
        {
            spriteBatch.End(out var snapshot);

            DrawWinds(spriteBatch, device, snapshot);

            spriteBatch.Begin(in snapshot);
        }
    }

    private static void DrawPixelated(SpriteBatch spriteBatch, GraphicsDevice device)
    {
        if (!SkyConfig.Instance.UsePixelatedSky || 
            !SkyEffects.PixelateAndQuantize.IsReady || 
            Main.mapFullscreen)
            return;

        Viewport viewport = device.Viewport;

        spriteBatch.End(out var snapshot);

        RenderTargetLease leasedTarget = RenderTargetPool.Shared.Rent(device, viewport.Width, viewport.Height, RenderTargetDescriptor.Default);

        using (new RenderTargetScope(device, leasedTarget.Target, true, true, Color.Transparent))
            DrawWinds(spriteBatch, device, snapshot);

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);

        Vector2 screenSize = new(viewport.Width, viewport.Height);

        SkyEffects.PixelateAndQuantize.ScreenSize = screenSize;
        SkyEffects.PixelateAndQuantize.PixelSize = new(2);

        SkyEffects.PixelateAndQuantize.Steps = SkyConfig.Instance.ColorSteps;

        int pass = (SkyConfig.Instance.ColorSteps == 255).ToInt();

        SkyEffects.PixelateAndQuantize.Apply(pass);

        spriteBatch.Draw(leasedTarget.Target, viewport.Bounds, Color.White);

        spriteBatch.Restart(in snapshot);

        leasedTarget.Dispose();
    }

    private static void DrawWinds(SpriteBatch spriteBatch, GraphicsDevice device, SpriteBatchSnapshot snapshot)
    {
        Matrix matrix = snapshot.TransformMatrix* Matrix.CreateTranslation(new(-Main.screenPosition, 0));

        spriteBatch.Begin(snapshot.SortMode, snapshot.BlendState, snapshot.SamplerState, snapshot.DepthStencilState, snapshot.RasterizerState, null, matrix);

        device.Textures[0] = SkyTextures.SunBloom;

        WindSystem.Winds.Draw(spriteBatch, device);

        spriteBatch.End();
    }

    #endregion
}
