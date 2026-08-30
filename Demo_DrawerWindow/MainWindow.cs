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
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Width = 800;
        Height = 500;
        Content = Build();
        DrawerContent = BuildDrawer();

        DrawerBackground = new SolidColorBrush(Color.FromArgb(240, 150, 150, 255));
    }

    private UIElement Build()
    {
        return GridX(
            children: [
                TextBlockX(
                    configure: x => {
                        x.Text = "Content panel";
                        x.FontSize = 20;
                        x.Margin = ThicknessX(10);
                        x.HorizontalAlignment = HorizontalAlignment.Center;
                        x.VerticalAlignment = VerticalAlignment.Top;
                    }
                ),
                ButtonX(
                    configure: x => {
                        _button = x;
                        x.Content = "Click to Open drawer";
                        x.FontSize = 16;
                        x.HorizontalAlignment = HorizontalAlignment.Center;
                        x.VerticalAlignment = VerticalAlignment.Center;
                        x.Padding = ThicknessX(10);
                        x.Margin = ThicknessX(10);

                        x.SetBinding(Button.IsEnabledProperty, BindingX(b => {
                            b.Source = this;
                            b.Path = new PropertyPath(nameof(DrawerWindow.IsDrawerClosed));
                            b.Mode = BindingMode.OneWay;
                        }));

                        x.Click += (s, e) => {
                            IsDrawerVisible = true;
                        };
                    }
                )
            ]
        );
    }

    private UIElement BuildDrawer()
    {
        return GridX(
            children: [
                StackPanelX(
                    configure: x => {
                        x.HorizontalAlignment = HorizontalAlignment.Left;
                        x.VerticalAlignment = VerticalAlignment.Top;
                    },
                    children: [
                        RadioButtonX(
                            configure: x => {
                                x.Content = "Drawer LEFT";
                                x.Margin = ThicknessX(10);

                                x.Click += (s,e) => {
                                    DrawerSide = DrawerSide.Left;
                                    DrawerLength = 400;
                                };
                            }
                        ),
                        RadioButtonX(
                            configure: x => {
                                x.Content = "Drawer TOP";
                                x.Margin = ThicknessX(10);

                                x.Click += (s,e) => {
                                    DrawerSide = DrawerSide.Top;
                                    DrawerLength = 200;
                                };
                            }
                        ),
                        RadioButtonX(
                            configure: x => {
                                x.Content = "Drawer RIGHT";
                                x.Margin = ThicknessX(10);
                                x.IsChecked = true;

                                x.Click += (s,e) => {
                                    DrawerSide = DrawerSide.Right;
                                    DrawerLength = 400;
                                };
                            }
                        ),
                        RadioButtonX(
                            configure: x => {
                                x.Content = "Drawer BOTTOM";
                                x.Margin = ThicknessX(10);

                                x.Click += (s,e) => {
                                    DrawerSide = DrawerSide.Bottom;
                                    DrawerLength = 200;
                                };
                            }
                        ),
                    ]
                ),
                TextBlockX(
                    configure: x => {
                        x.Text = "DrawerContent panel";
                        x.FontSize = 20;
                        x.FontStyle = FontStyles.Italic;
                        x.HorizontalAlignment = HorizontalAlignment.Center;
                        x.VerticalAlignment = VerticalAlignment.Center;
                    }
                ),
                CheckBoxX(
                    configure: x => {
                        x.Content = "Drawer PINNED";
                        x.VerticalContentAlignment = VerticalAlignment.Center;
                        x.HorizontalAlignment = HorizontalAlignment.Left;
                        x.VerticalAlignment = VerticalAlignment.Bottom;
                        x.Margin = ThicknessX(10);

                        x.Click += (s, e) => {
                            IsDrawerPinned = (x.IsChecked == true);
                        };
                    }
                )
            ]
        );
    }
}
