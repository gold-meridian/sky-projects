using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using ZenSkies.Common.Config;
using ZenSkies.Core.Particles;
using ZenSkies.Core.Rendering;

namespace ZenSkies.Common.DataStructures;

public record struct WindParticle : IParticle
{
    #region Private Fields

    private const int MaxOldPositions = 43;

    private const float WidthAmplitude = 3.5f;

    private const float LifeTimeIncrement = .004f;

    private const float SinLifeTimeFrequency = 7f;
    private const float SinGlobalTimeFrequency = .6f;

    private const float SinAmplitude = .1f;

    private const float LoopRange = .06f;

    private const float LoopMaxOffset = .3f;

    private const float Magnitude = 13f;

    #endregion

    #region Public Properties

    public Vector2 Position { get; set; }

    public Vector2[] OldPositions { get; init; }

    public Vector2 Velocity { get; set; }

    public float Wind { get; init; }

    public float LoopOffset { get; init; }

    public bool ShouldLoop { get; init; }

    public float LifeTime { get; set; }

    public bool IsActive { get; set; }

    #endregion

    #region Public Constructors

    public WindParticle(Vector2 position, float wind, bool shouldLoop)
    {
        Position = position;
        OldPositions = new Vector2[MaxOldPositions];
        Velocity = Vector2.Zero;
        Wind = wind;
        LoopOffset = Main.rand.NextFloat(-LoopMaxOffset, LoopMaxOffset);
        ShouldLoop = shouldLoop;
        LifeTime = 0f;
        IsActive = true;
    }

    #endregion

    #region Updating

    void IParticle.Update()
    {
        float increment = LifeTimeIncrement * MathF.Abs(Wind);

        LifeTime += increment;
        if (LifeTime > 1f)
            IsActive = false;

        Vector2 newVelocity = new(Wind, 
            MathF.Sin((LifeTime * SinLifeTimeFrequency + Main.GlobalTimeWrappedHourly) * SinGlobalTimeFrequency) * SinAmplitude);

        if (ShouldLoop)
        {
            float range = LoopRange / MathHelper.Clamp(MathF.Abs(Wind), .01f, 1);
            range *= .5f;

            float offset = .5f + LoopOffset;

            float interpolator = Utils.Remap(LifeTime, offset - range, offset + range, 0f, 1f);

            newVelocity = newVelocity.RotatedBy(MathHelper.TwoPi * interpolator * -MathF.Sign(Wind));
        }

        Velocity = newVelocity.SafeNormalize(Vector2.UnitY) * Magnitude * MathF.Abs(Wind);

        Position += Velocity;

            // Update the old positions.
        for (int i = OldPositions.Length - 2; i >= 0; i--)
            OldPositions[i + 1] = OldPositions[i];

        OldPositions[0] = Position;
    }

    #endregion

    #region Drawing

    readonly void IParticle.Draw(SpriteBatch spriteBatch, GraphicsDevice device)
    {
            // TODO: Better method of applying a matrix to these blasted particles.
        IReadOnlyList<Vector3> positions =
            [.. OldPositions.Where(pos => pos != default)
            .Select(p => new Vector3(Vector2.Transform(p, spriteBatch.transformMatrix), 0))];

        if (positions.Count <= 2)
            return;

        float brightness = MathF.Sin(LifeTime * MathHelper.Pi) * Main.atmo * MathF.Abs(Wind);

        float alpha = SkyConfig.Instance.WindOpacity;

            // Get the color based on the lighting at the center of the trail.
        Vector3 center = positions[positions.Count / 2];

        Point tilePosition = (new Vector2(center.X, center.Y) - Main.screenPosition).ToTileCoordinates();

        Color color = Lighting.GetColor(tilePosition).MultiplyRGB(Main.ColorOfTheSkies) * brightness * alpha;
        color.A = 0;

        VertexPositionColorTexture[] vertices =
            TriangleStripBuilder.BuildPath(positions,
            t => MathF.Sin(t * MathHelper.Pi) * brightness * WidthAmplitude,
            t => color,
            smoothingSubdivisions: 3);

        if (vertices.Length > 3)
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
    }

    #endregion
}
