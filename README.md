# DrawerWindow custom control

WPF custom control built **entirely in C#** with WPFX.


## `DrawerWindow`

Custom window control with a side drawer, based on the standard WPF `Window`.

### Properties
 
- `Content` (`object`), inherited from `Window`
- `DrawerContent` (`object`)
- `DrawerSide` (`DrawerSide`), Default: `DrawerSide.Right`
- `DrawerLength` (`double`), Default: `400`
- `DrawerBackground` (`Brush`), Default: `Brushes.LightGray`
- `IsDrawerVisible` (`bool`)
- `IsDrawerPinned` (`bool`)
- `IsDrawerClosed` (`bool`)

#### Notes
If a `Button` is used to open the drawer (see example app below), 
`IsDrawerClosed` is useful for binding to `Button.IsEnabledProperty` to act as a 'gate' to prevent repeat-setting the state `IsDrawerVisible=true` during the drawer open animation.

When the drawer is open, clicking on any part of the `DrawerWindow` client area that is _not_ part of the drawer surface, will close the drawer.

### Example demo app

#### [0] Template part: `PART_Drawer`, VisualState: `Collapsed`

![WPF demo app showing a DrawerWindow Closed](https://raw.githubusercontent.com/deeks9000/WpfxDrawerWindow/main/Images/WPFX_DrawerWindow_Closed.png)

---

#### [1] Template part: `PART_Drawer`, VisualState: `Visible`

- `DrawerSide = DrawerSide.Left`
- `DrawerLength = 400`

![WPF demo app showing a DrawerWindow Open Left](https://raw.githubusercontent.com/deeks9000/WpfxDrawerWindow/main/Images/WPFX_DrawerWindow_OpenLeft.png)

---

#### [2] Template part: `PART_Drawer`, VisualState: `Visible`

- `DrawerSide = DrawerSide.Top`
- `DrawerLength = 200`

![WPF demo app showing a DrawerWindow Open Top](https://raw.githubusercontent.com/deeks9000/WpfxDrawerWindow/main/Images/WPFX_DrawerWindow_OpenTop.png)

---

#### [3] Template part: `PART_Drawer`, VisualState: `Visible`

- `DrawerSide = DrawerSide.Right`
- `DrawerLength = 400`

![WPF demo app showing a DrawerWindow Open Right](https://raw.githubusercontent.com/deeks9000/WpfxDrawerWindow/main/Images/WPFX_DrawerWindow_OpenRight.png)

---

#### [4] Template part: `PART_Drawer`, VisualState: `Visible`

- `DrawerSide = DrawerSide.Bottom`
- `DrawerLength = 200`

![WPF demo app showing a DrawerWindow Open Bottom](https://raw.githubusercontent.com/deeks9000/WpfxDrawerWindow/main/Images/WPFX_DrawerWindow_OpenBottom.png)

---

### Example demo app source code

The demo app uses WPFX to create the `UIElement` tree.

The `DrawerWindow` has two dependency properties for content:
- `Content`: The client area content
- `DrawerContent`: The drawer content

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WpfxCustomControls;
using WpfxCustomControls.DrawerWindowTypes;

namespace Demo_DrawerWindow;

public class MainWindow : DrawerWindow
{
    private Button? _button;

    public MainWindow()
    {
        Title = "Demo DrawerWindow";
        Width = 800;
        Height = 500;
        Content = Build();
        DrawerContent = BuildDrawer();
    }

    private UIElement Build() { ... }

    private UIElement BuildDrawer() { ... }
}
```