using Microsoft.Xna.Framework.Graphics;

namespace ZenSkies.Common.Systems.Sky.Space;

public interface IStarModifier
{
    public bool IsActive { get; set; }

    public void Update(ref Star star);

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice device, ref Star star, float alpha, float rotation);
}
