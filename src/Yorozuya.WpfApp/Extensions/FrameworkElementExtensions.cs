using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Yorozuya.WpfApp.Extensions;

/// <summary>
/// 给 FrameworkElement 添加滑动和淡入淡出动画的扩展方法集
/// </summary>
public static class FrameworkElementExtensions
{
    /// <summary>
    /// 滑入并淡入
    /// </summary>
    /// <param name="element"></param>
    /// <param name="duration"></param>
    /// <param name="fromMargin"></param>
    /// <param name="easingFunction"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 滑出并淡出
    /// </summary>
    /// <param name="element"></param>
    /// <param name="duration"></param>
    /// <param name="toMargin"></param>
    /// <param name="easingFunction"></param>
    /// <returns></returns>
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
