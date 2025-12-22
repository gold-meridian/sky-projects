using Daybreak.Common.Features.Authorship;
using Daybreak.Common.Features.ModPanel;
using ReLogic.Content.Sources;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using ZenSkies.Core.ModCall;
using ZenSkies.Core.Net;
using ZenSkies.GeneratedAssets.AssetReaders;

#pragma warning disable CS8603 // Possible null reference return.

namespace ZenSkies;

public sealed class ZenSkies : Mod, IHasCustomAuthorMessage
{
    #region Public Properties

    public static bool CanDrawSky
    {
        get => field && !ModLoader.isLoading;
        private set;
    }

    public static bool Unloading { get; private set; }

    #endregion

    #region Loading

    public override void Close()
    {
            // Technically redundant.
        CanDrawSky = false;
        Unloading = true;

        base.Close();
    }

    public override void PostSetupContent() =>
        CanDrawSky = true;

    #endregion

    #region Content

    public override IContentSource CreateDefaultContentSource()
    {
        if (!Main.dedServ)
            AddContent(new ObjModelReader());

        return base.CreateDefaultContentSource();
    }

    #endregion

    #region Authorshp

    private const string AuthorshipHeaderKey = "Mods.ZenSkies.AuthorTags.Header";

    string IHasCustomAuthorMessage.GetAuthorText() =>
        AuthorText.GetAuthorTooltip(this, Language.GetTextValue(AuthorshipHeaderKey));

    #endregion

    #region Packets

    public override void HandlePacket(BinaryReader reader, int whoAmI) =>
        PacketSystem.Handle(this, reader, whoAmI);

    #endregion

    #region ModCall

    public override object Call(params object[] args)
    {
        if (args.Length <= 0)
            throw new ArgumentException("Zero arguments provided!");

        if (args[0] is not string name)
            throw new ArgumentException("First argument was not of type string!");

        return ModCallSystem.HandleCall(name, [.. args.Skip(1)]);
    }

    #endregion
}
