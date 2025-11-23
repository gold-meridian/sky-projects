using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace ZenSkies.Core.Utils;

[JsonConverter(typeof(StringEnumConverter))]
public enum EasingStyle : byte
{
    Linear,

    InCubic,
    OutCubic,
    InOutCubic,

    InQuart,
    OutQuart,
    InOutQuart,

    InQuint,
    OutQuint,
    InOutQuint,

    InSine,
    OutSine,
    InOutSine,

    InExpo,
    OutExpo,
    InOutExpo,

    InCirc,
    OutCirc,
    InOutCirc,

    InElastic,
    OutElastic,
    InOutElastic,

    InBack,
    OutBack,
    InOutBack
}
