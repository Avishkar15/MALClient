﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace MALClient.UWP.Shared.UserControls.AttachedProperties
{
    public class AnimatedVisibilityExtension : DependencyObject
    {
        public static readonly DependencyProperty AnimatedVisiblityProperty =
            DependencyProperty.RegisterAttached(
                "AnimatedVisiblity",
                typeof(Visibility),
                typeof(AnimatedVisibilityExtension),
                new PropertyMetadata(Visibility.Collapsed, PropertyChangedCallback));


        public static readonly DependencyProperty AnimatedVisibleStoryboardProperty =
            DependencyProperty.RegisterAttached("AnimatedVisibleStoryboard", typeof(Storyboard),
                typeof(AnimatedVisibilityExtension), new PropertyMetadata(null));


        public static readonly DependencyProperty AnimatedCollapsedStoryboardProperty =
            DependencyProperty.RegisterAttached("AnimatedCollapsedStoryboard", typeof(Storyboard),
                typeof(AnimatedVisibilityExtension), new PropertyMetadata(default(Storyboard)));

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            UpdateVisibility(sender,(Visibility)args.NewValue);
        }

        private static void UpdateVisibility(DependencyObject sender, Visibility value)
        {
            var element = sender as FrameworkElement;
            Storyboard sb;
            switch (value)
            {
                case Visibility.Visible:
                    sb = GetAnimatedVisibleStoryboard(sender);
                    if (sb != null)
                    {
                        try
                        {
                            sb.Begin();
                            element.Visibility = Visibility.Visible;
                        }
                        catch (Exception)
                        {
                            element.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case Visibility.Collapsed:
                    sb = GetAnimatedCollapsedStoryboard(sender);
                    if (sb != null)
                    {
                        try
                        {
                            sb.Completed += (o, o1) => element.Visibility = Visibility.Collapsed;
                            sb.Begin();
                        }
                        catch (Exception )
                        {
                            element.Visibility = Visibility.Collapsed;
                        }

                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetAnimatedVisiblity(DependencyObject element, Visibility value)
        {
            if (GetAnimatedVisiblity(element) == value)
            {
                UpdateVisibility(element,value);
            }

            element.SetValue(AnimatedVisiblityProperty, value);

            
        }

        public static Visibility GetAnimatedVisiblity(DependencyObject element)
        {
            return (Visibility)element.GetValue(AnimatedVisiblityProperty);
        }


        public static void SetAnimatedVisibleStoryboard(DependencyObject element, Storyboard value)
        {
            element.SetValue(AnimatedVisibleStoryboardProperty, value);
        }

        public static Storyboard GetAnimatedVisibleStoryboard(DependencyObject element)
        {
            return (Storyboard)element.GetValue(AnimatedVisibleStoryboardProperty);
        }

        public static void SetAnimatedCollapsedStoryboard(DependencyObject element, Storyboard value)
        {
            element.SetValue(AnimatedCollapsedStoryboardProperty, value);
        }

        public static Storyboard GetAnimatedCollapsedStoryboard(DependencyObject element)
        {
            return (Storyboard)element.GetValue(AnimatedCollapsedStoryboardProperty);
        }
    }
}