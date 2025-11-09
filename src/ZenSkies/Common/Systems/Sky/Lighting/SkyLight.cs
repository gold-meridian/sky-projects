using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using ZenSkies.Common.DataStructures;


namespace ZenSkies.Common.Systems.Sky.Lighting;

public abstract class SkyLight : ILoadable
{
    #region Public Delegates

    public delegate void hook_OnGetInfo(ref SkyLightInfo info);

    #endregion

    #region Public Events

    public event hook_OnGetInfo? OnGetInfo;

    #endregion

    #region Public Properties

    public virtual bool Active => true;

    #endregion

    #region Private Properties

    protected abstract Color Color { get; }

    protected abstract Vector2 Position { get; }

    protected abstract float Size { get; }

    protected virtual Texture2D? Texture => null;

    #endregion

    #region Loading

    void ILoadable.Load(Mod mod)
    {
        SkyLightSystem.Lights.Add(this);
        Load();
    }

    public virtual void Load() { }

    public virtual void Unload() { }

    #endregion

    #region Public Methods

    public SkyLightInfo GetInfo()
    {
        SkyLightInfo info = new(Color, Position, Size, Texture);

        OnGetInfo?.Invoke(ref info);

        return info;
    }

    #endregion
}
