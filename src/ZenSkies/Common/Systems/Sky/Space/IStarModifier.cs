using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenSkies.Common.DataStructures;

namespace ZenSkies.Common.Systems.Sky.Space;

public interface IStarModifier
{
    public bool IsActive { get; set; }

    public void Update(ref Star star);

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice device, ref Star star, float alpha, float rotation);
}
