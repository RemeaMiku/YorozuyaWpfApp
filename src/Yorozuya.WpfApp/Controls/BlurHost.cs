using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

#pragma warning disable

namespace Yorozuya.WpfApp.Controls;

public class BlurHost : ContentControl
{
    #region Public Fields

    public static readonly DependencyProperty BlurBackgroundProperty =
        DependencyProperty.Register(
          "BlurBackground",
          typeof(Visual),
          typeof(BlurHost),
          new PropertyMetadata(default(Visual), OnBlurBackgroundChanged));

    public static readonly DependencyProperty BlurOpacityProperty =
        DependencyProperty.Register(
          "BlurOpacity",
          typeof(double),
          typeof(BlurHost),
          new PropertyMetadata(1.0));

    public static readonly DependencyProperty BlurEffectProperty =
        DependencyProperty.Register(
          "BlurEffect",
          typeof(BlurEffect),
          typeof(BlurHost),
          new PropertyMetadata(
            new BlurEffect()
            {
                Radius = 10,
                KernelType = KernelType.Gaussian,
                RenderingBias = RenderingBias.Performance
            }));

    #endregion Public Fields

    #region Public Constructors

    static BlurHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BlurHost), new FrameworkPropertyMetadata(typeof(BlurHost)));
    }

    public BlurHost()
    {
        Loaded += OnLoaded;
        // TODO::Update Opacity of VisualBrush when property BlurOpacity changes
        BlurDecoratorBrush = new VisualBrush()
        {
            ViewboxUnits = BrushMappingMode.Absolute,
            Opacity = BlurOpacity
        };
    }

    #endregion Public Constructors

    #region Public Properties

    public Visual BlurBackground
    {
        get => (Visual)GetValue(BlurBackgroundProperty);
        set => SetValue(BlurBackgroundProperty, value);
    }
    public double BlurOpacity
    {
        get => (double)GetValue(BlurOpacityProperty);
        set => SetValue(BlurOpacityProperty, value);
    }
    public BlurEffect BlurEffect
    {
        get => (BlurEffect)GetValue(BlurEffectProperty);
        set => SetValue(BlurEffectProperty, value);
    }

    #endregion Public Properties

    #region Public Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.PART_BlurDecorator = GetTemplateChild("PART_BlurDecorator") as Border;
        this.PART_BlurDecorator.Effect = this.BlurEffect;
        this.PART_BlurDecorator.Background = this.BlurDecoratorBrush;
    }

    #endregion Public Methods

    #region Private Properties

    private Border PART_BlurDecorator { get; set; }
    private VisualBrush BlurDecoratorBrush { get; set; }

    #endregion Private Properties

    #region Private Methods

    private static void OnBlurBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var this_ = d as BlurHost;
        this_.BlurDecoratorBrush.Visual = e.NewValue as Visual;
        this_.DrawBlurredElementBackground();
    }

    private void DrawBlurredElementBackground()
    {
        if (!TryFindVisualRootContainer(this, out FrameworkElement rootContainer))
        {
            return;
        }

        // Get the section of the image where the BlurHost element is located
        Rect elementBounds = TransformToVisual(rootContainer)
          .TransformBounds(new Rect(this.RenderSize));

        // Use the section bounds to actually "cut out" the image tile 
        this.BlurDecoratorBrush.Viewbox = elementBounds;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (TryFindVisualRootContainer(this, out FrameworkElement rootContainer))
        {
            rootContainer.SizeChanged += OnRootContainerElementResized;
        }

        DrawBlurredElementBackground();
    }
    private void OnRootContainerElementResized(object sender, SizeChangedEventArgs e)
      => DrawBlurredElementBackground();

    private bool TryFindVisualRootContainer(DependencyObject child, out FrameworkElement rootContainerElement)
    {
        rootContainerElement = null;
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        if (parent == null)
        {
            return false;
        }

        if (parent is not Window visualRoot)
        {
            return TryFindVisualRootContainer(parent, out rootContainerElement);
        }

        rootContainerElement = visualRoot.Content as FrameworkElement;
        return true;
    }

    #endregion Private Methods
}
