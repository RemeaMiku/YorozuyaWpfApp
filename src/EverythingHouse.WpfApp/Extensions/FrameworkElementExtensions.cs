using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace EverythingHouse.WpfApp.Extensions;

public static class FrameworkElementExtensions
{
    public static async Task SlideAndFadeInAsync(this FrameworkElement element, TimeSpan duration, Thickness fromMargin, EasingFunctionBase easingFunction)
    {
        var storyboard = new Storyboard();
        var fadeInAnimation = new DoubleAnimation(0, 1, duration) { EasingFunction = easingFunction };
        var slideInAnimation = new ThicknessAnimation(fromMargin, new(), duration) { EasingFunction = easingFunction };
        Storyboard.SetTarget(element, slideInAnimation);
        Storyboard.SetTarget(element, fadeInAnimation);
        Storyboard.SetTargetProperty(slideInAnimation, new(FrameworkElement.MarginProperty));
        Storyboard.SetTargetProperty(fadeInAnimation, new(UIElement.OpacityProperty));
        storyboard.Children.Add(slideInAnimation);
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin(element);
        await Task.Delay(duration);
    }

    public static async Task SlideAndFadeOutAsync(this FrameworkElement element, TimeSpan duration, Thickness toMargin, EasingFunctionBase easingFunction)
    {
        var storyboard = new Storyboard();
        var fadeOutAnimation = new DoubleAnimation(1, 0, duration) { EasingFunction = easingFunction };
        var slideOutAnimation = new ThicknessAnimation(new(), toMargin, duration) { EasingFunction = easingFunction };
        Storyboard.SetTarget(element, slideOutAnimation);
        Storyboard.SetTarget(element, fadeOutAnimation);
        Storyboard.SetTargetProperty(slideOutAnimation, new(FrameworkElement.MarginProperty));
        Storyboard.SetTargetProperty(fadeOutAnimation, new(UIElement.OpacityProperty));
        storyboard.Children.Add(slideOutAnimation);
        storyboard.Children.Add(fadeOutAnimation);
        storyboard.Begin(element);
        await Task.Delay(duration);
    }
}
