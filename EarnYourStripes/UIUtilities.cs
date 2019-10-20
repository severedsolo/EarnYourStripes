using UnityEngine;

namespace EarnYourStripes
{
    public class UiUtilities
    {
        public UIStyle Header()
        {
            UIStyle style = new UIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = new UIStyleState(),
                fontSize = 18
            };
            style.normal.textColor = Color.yellow;
            return style;
        }
    }
}