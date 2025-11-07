using Terraria.ModLoader.Config;

namespace ZenSkies.Core.Config.Elements;

public sealed class LockedArgsAttribute : ConfigArgsAttribute
{
    public LockedArgsAttribute(params object[] args)
        : base(args) {}
}
