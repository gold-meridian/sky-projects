using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ZenSkies.Core.Utils;
using Star = ZenSkies.Common.DataStructures.Star;

namespace ZenSkies.Common.Systems.Sky.Space;

public sealed class Supernova : IStarModifier
{
    #region Private Fields

    private const float Increment = .003f;

    private const float ContractMultiplier = 1.25f;
    private const float ExpandMultiplier = .55f;
    private const float DecayMultiplier = .035f;

    private const float MinStarScale = .3f;
    private const float MaxStarScale = 1.25f;

    private const float SmallMultiplier = 1.5f;
    private const float BigMultiplier = .5f;

    private const float MinAlpha = .25f;
    private const float MaxAlpha = 1f;

    private const float Scale = .04f;

    private readonly Color EndingColor = Color.White;

    private readonly float Multiplier;

    private readonly Color StartingColor;
    private readonly float StartingScale;

    #endregion

    #region Public Properties

    public Color SupernovaColor { get; private init; }
    public float NebulaHue { get; private init; }

    public SupernovaState State { get; private set; }

    public float Contract { get; set; }
    public float Expand { get; set; }
    public float Decay { get; set; }

    public bool IsActive { get; set; }

    #endregion

    #region Public Constructors

    /// <param name="target">The <see cref="Star"/> that this supernova belongs to; will be modified during updating.</param>
    /// <param name="supernovaColor">The color of the inital explosion effect.</param>
    /// <param name="nebulaHue">The hue of the resulting nebula(e).</param>
    public Supernova(Star star, Color? supernovaColor = null, float nebulaHue = -1f)
    {
        StartingColor = star.Color;
        StartingScale = star.Scale;

            // Have smaller stars explode faster.
        Multiplier = Utils.Remap(star.Scale, MinStarScale, MaxStarScale, SmallMultiplier, BigMultiplier);

        SupernovaColor = supernovaColor ?? star.Color;

        if (nebulaHue == -1f)
            nebulaHue = Main.rand.NextFloat();

        NebulaHue = nebulaHue;

        Contract = 0f;
        Expand = 0f;
        Decay = 0f;

        IsActive = true;
    }

    #endregion

    #region Updating

    public void Update(ref Star star)
    {
        State = State switch
        {
            SupernovaState.Contracting => UpdateContracting(ref star),
            SupernovaState.Expanding => UpdateExpanding(),
            _ => SupernovaState.Complete
        };
    }

    private SupernovaState UpdateContracting(ref Star star)
    {
        Contract += Increment * ContractMultiplier * Multiplier;
        Contract = Utilities.Saturate(Contract);

        float colorInterpolator = Easings.OutQuint(Contract);
        star.Color = Color.Lerp(StartingColor, EndingColor, colorInterpolator);

            // Have the star increase in scale slightly before shrinking.
        float scaleMultiplier = Easings.OutBack(1 - Contract, 7);
        star.Scale = StartingScale * scaleMultiplier;

        if (Contract < 1f)
            return SupernovaState.Contracting;

        star.IsActive = false;

        return SupernovaState.Expanding;
    }

    private SupernovaState UpdateExpanding()
    {
        Expand += Increment * ExpandMultiplier * Multiplier;
        Expand = Utilities.Saturate(Expand);

        Decay += Increment * DecayMultiplier * Multiplier;
        Decay = Utilities.Saturate(Decay);

        if (Expand < 1f || Decay < 1f)
            return SupernovaState.Expanding;

        IsActive = false;

        return SupernovaState.Complete;
    }

    #endregion

    #region Drawing

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice device, ref Star star, float alpha, float rotation)
    {
        if (State == SupernovaState.Contracting)
            return;

        Vector2 position = star.Position;
        rotation = star.Rotation + rotation;

        SkyEffects.Supernova.Hue = NebulaHue;
        SkyEffects.Supernova.ExplosionColor = SupernovaColor.ToVector4();

        SkyEffects.Supernova.Expand = Expand;
        SkyEffects.Supernova.Decay = Decay;

        SkyEffects.Supernova.GlobalTime = Main.GlobalTimeWrappedHourly;

            // Arbitrary noise offset.
        SkyEffects.Supernova.Offset = position;

            // Textures[1] is reserved for Realistic Sky's atmosphere effect.
        device.Textures[2] = SkyTextures.SupernovaNoise[1];
        device.SamplerStates[2] = SamplerState.PointWrap;

        SkyEffects.Supernova.Apply();

        Texture2D texture = SkyTextures.SupernovaNoise[0];

        Vector2 origin = texture.Size() * .5f;

        Color color = SupernovaColor * Utils.Remap(alpha, 0, 1, MinAlpha, MaxAlpha);

        float scale = Scale * StartingScale;

        spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
    }

    #endregion
}
