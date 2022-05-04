using Unity.UIWidgets.engine;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace UIWidgetsApp.Main {
    public sealed class UIWidgetsAppPanel : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new Container(color: Color.black));
        }
    }
}