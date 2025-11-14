using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using ZenSkies.Core.Utils;
using Star = ZenSkies.Common.Systems.Sky.Space.Star;

namespace ZenSkies.Common.Systems.Sky.Space;

public sealed class Supernova : IStarModifier
{
    #region Private Fields

    private const float ContractIncrement = .002f;
    private const float ExpandIncrement = .0005f;

    private const float MinStarScale = .3f;
    private const float MaxStarScale = 1.25f;

    private const float SmallSpeedMultiplier = 1.5f;
    private const float BigSpeedMultiplier = .5f;

    private readonly float SpeedMultiplier;

    private readonly Color StartingColor;
    private readonly Color EndingColor = Color.White;

    private readonly float StartingScale;

    private const float FlareWaveFrequency = 8f;
    private const float FlareWaveAmplitude = .06f;

    private static readonly Vector2 FlareSize = new(.9f, .17f);

    #endregion

    #region Public Properties

    public Color NebulaColor { get; private init; }

    public SupernovaState State { get; private set; }

    public float Contract { get; set; }
    public float Expand { get; set; }

    public bool IsActive { get; set; }

    #endregion

    #region Public Constructors

    /// <param name="target">The <see cref="Star"/> that this supernova belongs to; will be modified during updating.</param>
    /// <param name="supernovaColor">The color of the inital explosion effect.</param>
    /// <param name="nebulaHue">The hue of the resulting nebula(e).</param>
    public Supernova(Star star, Color? nebulaColor = null)
    {
        StartingColor = star.Color;
        StartingScale = star.Scale;

            // Smaller stars should explode faster.
        SpeedMultiplier = Utils.Remap(star.Scale, MinStarScale, MaxStarScale, SmallSpeedMultiplier, BigSpeedMultiplier);

        NebulaColor = nebulaColor ?? Utilities.HSVToColor(Main.rand.NextFloat());

        Contract = 0f;
        Expand = 0f;

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
            _ => UpdateComplete()
        };
    }

    private SupernovaState UpdateContracting(ref Star star)
    {
        Contract += ContractIncrement * SpeedMultiplier;
        Contract = Utilities.Saturate(Contract);

            // Have the start fade to white.
        float colorInterpolator = MathF.Sin(Contract * MathHelper.Pi);
        star.Color = Color.Lerp(StartingColor, EndingColor, colorInterpolator);

            // Have the star quickly fade out allowing the flare to take prominence.
        float scaleMultiplier = Easings.InCubic(1 - Contract);
        star.Scale = StartingScale * scaleMultiplier;

        if (Contract < 1f)
            return SupernovaState.Contracting;

        star.IsActive = false;

        return SupernovaState.Expanding;
    }

    private SupernovaState UpdateExpanding()
    {
        Expand += ExpandIncrement * SpeedMultiplier;
        Expand = Utilities.Saturate(Expand);

        if (Expand < 1f)
            return SupernovaState.Expanding;

        return SupernovaState.Complete;
    }

    private SupernovaState UpdateComplete()
    {
        IsActive = false;

        return SupernovaState.Complete;
    }

    #endregion

    #region Drawing

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice device, ref Star star, float alpha, float rotation)
    {
        Vector2 position = star.Position;

        float scale = StartingScale;

        Color color = star.Color;

        if (State == SupernovaState.Contracting)
            DrawFlare(spriteBatch, position, color, scale, rotation);
    }

    private void DrawFlare(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float rotation)
    {
        Texture2D texture = StarTextures.FourPointedStar;

        Vector2 origin = texture.Size() * .5f;

            // Pulse the flare out smoothly.
        float pulse = MathF.Sin(Contract * MathHelper.Pi);
        scale *= pulse;

            // Subtle triangle wave to make it feel chaotic.
        float time = Main.GlobalTimeWrappedHourly * FlareWaveFrequency;

        float wave = MathF.Abs((time % 2) - 1f) * FlareWaveAmplitude * Easings.InCubic(pulse);
        wave += 1;
        scale *= wave;

            // Flare will be longer horizontally.
        Vector2 size = scale * FlareSize;

        color.A = 0;

        spriteBatch.Draw(texture, position, null, color, rotation, origin, size, SpriteEffects.None, 0f);
    }

    #endregion
}
