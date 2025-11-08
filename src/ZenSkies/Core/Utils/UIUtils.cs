using Terraria.UI;

namespace ZenSkies.Core.Utils;

public static partial class Utilities
{
    #region UI

    extension(UIElement element)
    {
        /// <inheritdoc cref="UIElement.GetDimensions"/>
        public Rectangle Dimensions =>
            element.GetDimensions().ToRectangle();

        /// <inheritdoc cref="UIElement.GetInnerDimensions"/>
        public Rectangle InnerDimensions =>
            element.GetInnerDimensions().ToRectangle();

        public Rectangle DimensionsFromParent
        {
            get
            {
                UIElement parent = element.Parent;

                if (parent is null)
                    return element.Dimensions;

                return element.GetDimensionsBasedOnParentDimensions(parent.GetInnerDimensions()).ToRectangle();
            }
        }
    }

    #endregion
}
