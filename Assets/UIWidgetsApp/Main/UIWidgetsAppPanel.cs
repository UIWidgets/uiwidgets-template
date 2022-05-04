using System.Collections.Generic;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;

namespace UIWidgetsApp.Main
{
    public sealed class UIWidgetsAppPanel : UIWidgetsPanel
    {
        protected override void main()
        {
            ui_.runApp(new UIWidgetsApp());
        }

        protected override void onEnable()
        {
            LoadFonts();
            base.onEnable();
        }

        private void LoadFonts()
        {
            AddFont("Material Icons", new List<string> { "Font/Material-Icons.ttf" }, new List<int> { 0 });
        }
    }
}