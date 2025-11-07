using Terraria.ModLoader.Config;

namespace ZenSkies.Core.Config.Elements;

public sealed class LockedKeyAttribute : ConfigKeyAttribute
{
    public LockedKeyAttribute(string key)
        : base(key) {}
}
