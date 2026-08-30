using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfxCustomControls.DrawerWindowTypes;

namespace WpfxCustomControls;

[TemplateVisualState(Name = "Collapsed", GroupName = "DrawerStates")]
[TemplateVisualState(Name = "Visible", GroupName = "DrawerStates")]
[TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
[TemplatePart(Name = "PART_Drawer", Type = typeof(Grid))]
public class DrawerWindow : Window
{
    private static readonly TimeSpan DrawerAnimationDuration = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan DrawerGeometrySyncDelay = DrawerAnimationDuration + TimeSpan.FromMilliseconds(100);

    private bool _isDrawerGeometryDirty = false;
    private double _vsgCollapsedOffset = 400;

    private TranslateTransform DrawerTransform { get; } = new TranslateTransform();

    public static Style DefaultStyle { get; }

    static DrawerWindow()
    {
        DefaultStyle = BuildDefaultStyle();

        StyleProperty.OverrideMetadata(typeof(DrawerWindow), new FrameworkPropertyMetadata(DefaultStyle));        
    }

    private static Style BuildDefaultStyle()
    {
        var visualTree = FrameworkElementFactoryX<Grid>(
            name: "PART_Root",
            setters: [
                SetterX(Panel.BackgroundProperty, BindingX(b => {
                    b.Path = new PropertyPath(nameof(Control.Background));
                    b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                    b.TargetNullValue = Brushes.Transparent;
                }))
            ],
            children: [
                FrameworkElementFactoryX<ContentPresenter>(
                    name: "PART_Presenter",
                    setters: [
                        SetterX(ContentPresenter.ContentSourceProperty, "Content")
                    ]
                ),
                FrameworkElementFactoryX<Grid>(
                    name: "PART_Drawer",
                    setters: [
                        SetterX(Panel.BackgroundProperty, BindingX(b => {
                            b.Path = new PropertyPath(nameof(DrawerWindow.DrawerBackground));
                            b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                        })),
                        SetterX(FrameworkElement.WidthProperty, BindingX(b => {
                            b.Path = new PropertyPath(nameof(DrawerWindow.DrawerWidth));
                            b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                        })),
                        SetterX(FrameworkElement.HeightProperty, BindingX(b => {
                            b.Path = new PropertyPath(nameof(DrawerWindow.DrawerHeight));
                            b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                        })),
                        SetterX(FrameworkElement.HorizontalAlignmentProperty, BindingX(b => {
                            b.Path = new PropertyPath(nameof(DrawerWindow.DrawerHorizontalAlignment));
                            b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                        })),
                        SetterX(FrameworkElement.VerticalAlignmentProperty, BindingX(b => {
                            b.Path = new PropertyPath(nameof(DrawerWindow.DrawerVerticalAlignment));
                            b.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                        })),
                    ],
                    children: [
                        FrameworkElementFactoryX<ContentPresenter>(
                            name: "PART_DrawerPresenter",
                            setters: [
                                SetterX(ContentPresenter.ContentSourceProperty, "DrawerContent")
                            ]
                        )
                    ]
                )
            ]
        );

        var template = ControlTemplateX<DrawerWindow>(visualTree);

        var style = StyleX<DrawerWindow>(
            setters: [
                SetterX(Control.TemplateProperty, template)
            ]
        );

        return style;
    }

    private List<VisualStateGroup> BuildVisualStateGroups()
    {
        List<VisualStateGroup> list = new List<VisualStateGroup>();

        string targetName = "PART_Drawer";
        string targetPath = DrawerSide is DrawerSide.Left or DrawerSide.Right
            ? "(UIElement.RenderTransform).(TranslateTransform.X)"
            : "(UIElement.RenderTransform).(TranslateTransform.Y)";

        VisualStateGroup group = new VisualStateGroup();
        group.Name = "DrawerStates";
        group.States.Add(BuildVisualState("Collapsed", _vsgCollapsedOffset, targetName, targetPath));
        group.States.Add(BuildVisualState("Visible", 0, targetName, targetPath));

        list.Add(group);

        return list;
    }

    private VisualState BuildVisualState(string name, double to, string targetName, string targetPath)
    {
        var animation = new DoubleAnimation {
            To = to,
            Duration = new Duration(DrawerAnimationDuration),
            EasingFunction = new CubicEase {
                EasingMode = EasingMode.EaseOut
            }
        };

        Storyboard.SetTargetName(animation, targetName);
        Storyboard.SetTargetProperty(animation, new PropertyPath(targetPath));

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);

        return new VisualState
        {
            Name = name,
            Storyboard = storyboard
        };
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateDrawerGeometry();

        VisualStateManager.GoToState(this, "Collapsed", false);
    }

    private void UpdateDrawerGeometry()
    {
        DrawerWidth = DrawerSide is DrawerSide.Left or DrawerSide.Right
            ? DrawerLength
            : double.NaN;

        DrawerHeight = DrawerSide is DrawerSide.Top or DrawerSide.Bottom
            ? DrawerLength
            : double.NaN;

        DrawerHorizontalAlignment = DrawerSide switch 
        {
            DrawerSide.Left => HorizontalAlignment.Left,
            DrawerSide.Right => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Stretch
        };

        DrawerVerticalAlignment = DrawerSide switch
        {
            DrawerSide.Top => VerticalAlignment.Top,
            DrawerSide.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Stretch
        };

        var offsetX = DrawerSide switch
        {
            DrawerSide.Right => +DrawerLength,
            DrawerSide.Left => -DrawerLength,
            _ => 0
        };

        var offsetY = DrawerSide switch
        {
            DrawerSide.Bottom => +DrawerLength,
            DrawerSide.Top => -DrawerLength,
            _ => 0
        };

        // --- Reset transform ---
        DrawerTransform.BeginAnimation(TranslateTransform.XProperty, null);
        DrawerTransform.BeginAnimation(TranslateTransform.YProperty, null);

        DrawerTransform.X = offsetX;
        DrawerTransform.Y = offsetY;

        var drawer = GetTemplateChild("PART_Drawer") as FrameworkElement;
        if (drawer == null) return;

        drawer.RenderTransform = DrawerTransform;

        _vsgCollapsedOffset = DrawerSide is DrawerSide.Left or DrawerSide.Right
            ? offsetX
            : offsetY;

        // --- Rebuild VisualStateGroups ---
        var root = GetTemplateChild("PART_Root") as FrameworkElement;
        if (root == null) return;

        var groups = VisualStateManager.GetVisualStateGroups(root);
        groups.Clear();

        var newGroups = BuildVisualStateGroups();

        foreach (var group in newGroups)
            groups.Add(group);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        var drawer = GetTemplateChild("PART_Drawer") as Grid;

        if (drawer != null && !drawer.IsMouseOver && IsDrawerVisible && !IsDrawerPinned)
        {
            IsDrawerVisible = false;
            e.Handled = true;
        }
    }

    // --- Dependency Properties ---
    public static readonly DependencyProperty DrawerContentProperty = DependencyProperty.Register(
        nameof(DrawerContent),
        typeof(object),
        typeof(DrawerWindow)
    );

    public static readonly DependencyProperty DrawerBackgroundProperty = DependencyProperty.Register(
        nameof(DrawerBackground),
        typeof(Brush),
        typeof(DrawerWindow),
        new PropertyMetadata(Brushes.LightGray)
    );

    public static readonly DependencyProperty DrawerSideProperty = DependencyProperty.Register(
        nameof(DrawerSide),
        typeof(DrawerSide),
        typeof(DrawerWindow),
        new PropertyMetadata(DrawerSide.Right, OnDrawerSideChanged)
    );

    public static readonly DependencyProperty DrawerLengthProperty = DependencyProperty.Register(
        nameof(DrawerLength),
        typeof(double),
        typeof(DrawerWindow),
        new PropertyMetadata(400d, OnDrawerLengthChanged)
    );   

    public static readonly DependencyProperty IsDrawerVisibleProperty = DependencyProperty.Register(
        nameof(IsDrawerVisible),
        typeof(bool),
        typeof(DrawerWindow),
        new PropertyMetadata(false, OnIsDrawerVisibleChanged)
    );

    public static readonly DependencyProperty IsDrawerPinnedProperty = DependencyProperty.Register(
        nameof(IsDrawerPinned),
        typeof(bool),
        typeof(DrawerWindow),
        new PropertyMetadata(false)
    );

    public static readonly DependencyProperty DrawerWidthProperty = DependencyProperty.Register(
        nameof(DrawerWidth),
        typeof(double),
        typeof(DrawerWindow),
        new PropertyMetadata(double.NaN)
    );

    public static readonly DependencyProperty DrawerHeightProperty = DependencyProperty.Register(
        nameof(DrawerHeight),
        typeof(double),
        typeof(DrawerWindow),
        new PropertyMetadata(double.NaN)
    );

    public static readonly DependencyProperty DrawerHorizontalAlignmentProperty = DependencyProperty.Register(
        nameof(DrawerHorizontalAlignment),
        typeof(HorizontalAlignment),
        typeof(DrawerWindow),
        new PropertyMetadata(HorizontalAlignment.Stretch)
    );

    public static readonly DependencyProperty DrawerVerticalAlignmentProperty = DependencyProperty.Register(
        nameof(DrawerVerticalAlignment),
        typeof(VerticalAlignment),
        typeof(DrawerWindow),
        new PropertyMetadata(VerticalAlignment.Stretch)
    );

    public static readonly DependencyProperty IsDrawerClosedProperty = DependencyProperty.Register(
        nameof(IsDrawerClosed),
        typeof(bool),
        typeof(DrawerWindow),
        new PropertyMetadata(true)
    );         

    // --- CLR Properties ---
    public object DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    public Brush DrawerBackground
    {
        get => (Brush)GetValue(DrawerBackgroundProperty);
        set => SetValue(DrawerBackgroundProperty, value);
    }

    public DrawerSide DrawerSide
    {
        get => (DrawerSide)GetValue(DrawerSideProperty);
        set => SetValue(DrawerSideProperty, value);
    }

    public double DrawerLength
    {
        get => (double)GetValue(DrawerLengthProperty);
        set => SetValue(DrawerLengthProperty, value);
    }      

    public bool IsDrawerVisible
    {
        get => (bool)GetValue(IsDrawerVisibleProperty);
        set => SetValue(IsDrawerVisibleProperty, value);
    }

    public bool IsDrawerPinned
    {
        get => (bool)GetValue(IsDrawerPinnedProperty);
        set => SetValue(IsDrawerPinnedProperty, value);
    }

    public double DrawerWidth
    {
        get => (double)GetValue(DrawerWidthProperty);
        private set => SetValue(DrawerWidthProperty, value);
    }

    public double DrawerHeight
    {
        get => (double)GetValue(DrawerHeightProperty);
        private set => SetValue(DrawerHeightProperty, value);
    }

    public HorizontalAlignment DrawerHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(DrawerHorizontalAlignmentProperty);
        private set => SetValue(DrawerHorizontalAlignmentProperty, value);
    }

    public VerticalAlignment DrawerVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(DrawerVerticalAlignmentProperty);
        private set => SetValue(DrawerVerticalAlignmentProperty, value);
    }

    public bool IsDrawerClosed
    {
        get => (bool)GetValue(IsDrawerClosedProperty);
        private set => SetValue(IsDrawerClosedProperty, value);
    }

    //---------------------------------------------------------------------------------------------------
    // Internal logic

    private static void OnIsDrawerVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (DrawerWindow)d;
        var isVisible = (bool)e.NewValue;

        window.IsDrawerClosed = false;

        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = DrawerGeometrySyncDelay;
        
        timer.Tick += (s, e) =>
        {
            timer.Stop();

            if (window._isDrawerGeometryDirty)
            {
                window._isDrawerGeometryDirty = false;
                window.UpdateDrawerGeometry();
            }

            window.IsDrawerClosed = true;
        };

        if (!isVisible)
            timer.Start();

        window.GoToDrawerState(isVisible);
    }

    private void GoToDrawerState(bool isVisible)
    {
        VisualStateManager.GoToState(this, isVisible ? "Visible" : "Collapsed", true);
    }

    private static void OnDrawerSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (DrawerWindow)d;

        if (window.IsDrawerVisible)
        {
            window._isDrawerGeometryDirty = true;
            return;
        }

        window.UpdateDrawerGeometry();
    }

    private static void OnDrawerLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (DrawerWindow)d;

        if (window.IsDrawerVisible)
        {
            window._isDrawerGeometryDirty = true;
            return;
        }

        window.UpdateDrawerGeometry();
    }

    //---------------------------------------------------------------------------------------------------
    // Public API
    
    public void ShowDrawer() => IsDrawerVisible = true;

    public void HideDrawer() => IsDrawerVisible = false;

    public void SetDrawerPinned(bool state) => IsDrawerPinned = state;
}
