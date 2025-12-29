using Daybreak.Common.Features.Authorship;

namespace ZenSkies.Common.Authorship;

public abstract class ZenSkiesAuthorTag : AuthorTag
{
    private const string suffix = "Tag";

    public override string Name => base.Name.EndsWith(suffix) ? base.Name.Replace(suffix, string.Empty) : base.Name;

    public override string Texture => AuthorshipTextures.PATH + $"/{Name}";
}
