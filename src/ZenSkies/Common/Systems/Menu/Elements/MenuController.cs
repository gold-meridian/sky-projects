using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;

#nullable disable

namespace ZenSkies.Common.Systems.Menu.Elements;

[Autoload(Side = ModSide.Client)]
public abstract class MenuController : UIPanel, ILocalizedModType, ILoadable
{
    #region Public Fields

    public UIText UIName;

    #endregion

    #region Public Properties

    public string LocalizationCategory => "MenuController";

    public Mod Mod { get; private set; }

    public string FullName =>
        (Mod?.Name ?? "Terraria") + "/" + Name;

    public abstract int Index { get; }

    public virtual string Name =>
        GetType().Name;

    public LocalizedText DisplayName =>
        this.GetLocalization("DisplayName");

    #endregion

    #region Loading

    void ILoadable.Load(Mod mod) 
    {
        Mod = mod;

        _ = DisplayName;

        Load();
        Refresh();
    }

    public virtual void Load() { }

    public virtual void Unload() { }

    public virtual void Refresh() { }

    #endregion

    #region Initialization

    public override void OnInitialize()
    {
        UIName = new(DisplayName)
        {
            HAlign = .5f
        };

        Append(UIName);
    }

    #endregion

    #region Sorting

    public override int CompareTo(object obj)
    {
        if (obj is MenuController element)
            return element.Index > Index ? -1: 1;

        return 0;
    }

    #endregion
}
