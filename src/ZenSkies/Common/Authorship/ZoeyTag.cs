using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace ZenSkies.Common.Authorship;

public class ZoeyTag : ZenSkiesAuthorTag
{
    private static readonly Color glow_color = new(179, 133, 255);

    public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
    {
        var glowPosition = new Vector2(position.X, position.Y - 2);
        var glowColor = glow_color * MathF.Sin(Main.GlobalTimeWrappedHourly);
        {
            spriteBatch.Draw(AuthorshipTextures.ZoeyGlow, glowPosition, glowColor);
        }

        base.DrawIcon(spriteBatch, position);
    }
}
