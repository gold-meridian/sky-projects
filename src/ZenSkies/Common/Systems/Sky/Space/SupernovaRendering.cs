using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ZenSkies.Common.Systems.Compat;
using ZenSkies.Core.ModCall;
using static ZenSkies.Common.Systems.Sky.Space.StarHooks;
using static ZenSkies.Common.Systems.Sky.Space.StarSystem;

namespace ZenSkies.Common.Systems.Sky.Space;

public static class SupernovaRendering
{
    #region Loading

    [OnLoad(Side = ModSide.Client)]
    public static void Load() =>
        PostDrawStars += SupernovaePostDraw;

    #endregion

    #region Drawing

    [ModCall(
        "DrawSupernova", "DrawSupernovas",
        "DrawAllSupernova", "DrawAllSupernovas", "DrawAllSupernovae")]
    public static void DrawSupernovae(SpriteBatch spriteBatch, GraphicsDevice device, float alpha)
    {
        if (!SkyEffects.Supernova.IsReady)
            return;

        float rotation = -StarRotation;

        if (RealisticSkySystem.IsEnabled)
            RealisticSkySystem.SetAtmosphereParams(SkyEffects.Supernova.Value);

        DrawStarModifiers<Supernova>(spriteBatch, device, alpha, rotation);
    }

    private static void SupernovaePostDraw(SpriteBatch spriteBatch, in SpriteBatchSnapshot snapshot, float alpha, Matrix transform)
    {
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, snapshot.DepthStencilState, snapshot.RasterizerState, null, transform);

        GraphicsDevice device = Main.instance.GraphicsDevice;

        DrawSupernovae(spriteBatch, device, alpha);

        spriteBatch.End();
    }

    #endregion
}
