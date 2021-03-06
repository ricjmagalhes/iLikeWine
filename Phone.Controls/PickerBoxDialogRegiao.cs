﻿//===============================================================================
// Copyright  Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Unsupported;
using System.Linq;
using Microsoft.Phone.Shell;


namespace Phone.Controls
{
    /// <summary>
    /// Implementation for the PickerBoxDialogRegiao
    /// </summary>
    public class PickerBoxDialogRegiao : ContentControl
    {
        #region fields

        private ListBox listBoxRegiao;
        private TextBlock titleTextBlock;
        private IEnumerable itemSource;
        private string title;
        private int selectedIndexRegiao;
        private List<ListBoxItem> visibleItems;
        public event EventHandler Closed;

        private static Storyboard OpenStoryboard;
        private static Storyboard CloseStoryboard;

        #endregion

        #region constructor

        public PickerBoxDialogRegiao()
        {
            this.DefaultStyleKey = typeof(PickerBoxDialogRegiao);
            TiltEffect.SetIsTiltEnabled(this, true);
        }

        #endregion

        #region overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // We are ready to retreive controls from the template
            this.listBoxRegiao = this.GetTemplateChild("listBox") as ListBox;
            listBoxRegiao.ItemsSource = ItemSource;

            this.titleTextBlock = this.GetTemplateChild("DialogTitle") as TextBlock;
            // Assign the values
            this.ItemSource = itemSource;
            this.Title = title;
            if (this.ItemSource != null)
            {
                SelectedIndexRegiao = selectedIndexRegiao;

                if (listBoxRegiao.SelectedItem != null)
                    listBoxRegiao.SetVerticalScrollOffset(SelectedIndexRegiao);
            }
            listBoxRegiao.Opacity = 0;
            // Hook up into listBoxRegiao's events
            this.listBoxRegiao.SelectionChanged += new SelectionChangedEventHandler(listBoxRegiao_SelectionChanged);
            this.listBoxRegiao.Loaded += new RoutedEventHandler(listBoxRegiao_Loaded);
            this.listBoxRegiao.SelectionMode = SelectionMode.Multiple;
        }



        #endregion

        #region event handlers

        void listBoxRegiao_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                PrepareOpenStoryboard();
                StartOpenAnimation();
            });
        }


        void listBoxRegiao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (listBoxRegiao.SelectedIndex > -1 && e.RemovedItems.Count > 0)
            {
                return;
            }
            else if (e.RemovedItems.Count > 0)
            {
                listBoxRegiao.SelectedItem = e.RemovedItems[0];
            }


            if (e.AddedItems.Count > 0)
            {
                listBoxRegiao.SelectedItem = e.AddedItems[0];
            }

            // Raise closed eventI 
            if (this.Closed != null)
            {
                this.Closed(this, EventArgs.Empty);
            }

            this.PrepareCloseStoryboard();

            // Unhook the BackKeyPress event handler
            ((PhoneApplicationPage)RootVisual.Content).BackKeyPress -= new EventHandler<System.ComponentModel.CancelEventArgs>(PickerBoxDialogRegiao_BackKeyPress);

            //this.ChildWindowPopup.IsOpen = false;

            StartCloseAnimation();


        }

        void PickerBoxDialogRegiao_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unhook the BackKeyPress event handler
            ((PhoneApplicationPage)RootVisual.Content).BackKeyPress -= new EventHandler<System.ComponentModel.CancelEventArgs>(PickerBoxDialogRegiao_BackKeyPress);
            // Cancel navigation
            e.Cancel = true;
            // Start animation
            PrepareCloseStoryboard();
            StartCloseAnimation();
        }

        #endregion

        #region helper methods

        private void StartOpenAnimation()
        {
            OpenStoryboard.Begin();
        }

        private void StartCloseAnimation()
        {
            CloseStoryboard.Begin();
        }


        public static DoubleAnimation CreateRotationAnimation(DependencyObject obj, double from, double value, double milliseconds, double beginTime, EasingMode easing = EasingMode.EaseInOut)
        {
            CubicEase ease = new CubicEase() { EasingMode = easing };
            DoubleAnimation animation = new DoubleAnimation();

            PropertyPath propPath = new PropertyPath("(UIElement.Projection).(PlaneProjection.RotationX)");

            animation.BeginTime = TimeSpan.FromMilliseconds(beginTime);
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(milliseconds));
            animation.From = from;
            animation.To = Convert.ToDouble(value);
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.EasingFunction = ease;
            Storyboard.SetTarget(animation, obj);
            Storyboard.SetTargetProperty(animation, propPath);

            return animation;
        }

        public static DoubleAnimation CreateOpacityAnimation(DependencyObject obj, double from, double value, double milliseconds, double beginTime, EasingMode easing = EasingMode.EaseIn)
        {
            CubicEase ease = new CubicEase() { EasingMode = easing };
            DoubleAnimation animation = new DoubleAnimation();

            PropertyPath propPath = new PropertyPath("(UIElement.Opacity)");

            //PropertyPath propPath = new PropertyPath(UIElement.OpacityProperty);
            animation.BeginTime = TimeSpan.FromMilliseconds(beginTime);
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(milliseconds));
            animation.From = from;
            animation.To = Convert.ToDouble(value);
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.EasingFunction = ease;

            Storyboard.SetTarget(animation, obj);
            Storyboard.SetTargetProperty(animation, propPath);


            return animation;
        }

        private void ResetItems()
        {

            for (int i = 0; i < this.listBoxRegiao.Items.Count; i++)
            {
                ListBoxItem item = this.listBoxRegiao.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item != null && item.Projection != null)
                {
                    ((PlaneProjection)item.Projection).RotationX = 0;
                    ((PlaneProjection)item.Projection).RotationY = 0;
                }
            }
        }

        private void PrepareCloseStoryboard()
        {
            if (CloseStoryboard == null)
            {
                CloseStoryboard = new Storyboard();
            }

            if (this.titleTextBlock == null)
                return;

            CloseStoryboard.Stop();

            CloseStoryboard.Children.Clear();

            int beginTime = 100;
            // Create animation for title textblock
            // Close animation
            DoubleAnimation animationClose = CreateRotationAnimation(titleTextBlock, 0, 90, 200, beginTime);
            DoubleAnimation animationCloseOp = CreateOpacityAnimation(titleTextBlock, 1, 0, 200, beginTime);
            CloseStoryboard.Children.Add(animationClose);
            CloseStoryboard.Children.Add(animationCloseOp);


            PlaneProjection projection = new PlaneProjection();
            projection.RotationX = 0;
            projection.RotationY = 0;
            titleTextBlock.Projection = projection;

            beginTime += 30;

            VirtualizingStackPanel panel = this.listBoxRegiao.GetVisualDescendents<VirtualizingStackPanel>(true).FirstOrDefault();
            this.visibleItems = panel.GetVisualDescendents<ListBoxItem>(true).GetVisibleItems(listBoxRegiao, Orientation.Vertical).ToList();

            for (int i = 0; i < visibleItems.Count; i++)
            {
                ListBoxItem item = visibleItems[i];

                if (item != null)
                {
                    // Create projection
                    projection = new PlaneProjection();
                    //projection.RotationX = 0;
                    //projection.RotationY = 0;
                    // Assign it to the item
                    item.Projection = projection;
                    item.CacheMode = new BitmapCache();

                    // Create close animation
                    animationClose = CreateRotationAnimation(item, 0, 90, 200, beginTime);
                    animationCloseOp = CreateOpacityAnimation(item, 1, 0, 200, beginTime);
                    // Add it to the storyboard
                    CloseStoryboard.Children.Add(animationClose);
                    CloseStoryboard.Children.Add(animationCloseOp);
                    // Increment the begin time for animation
                    beginTime += 30;
                }
            }

            CloseStoryboard.Completed += new EventHandler(CloseStoryboard_Completed);

        }

        void CloseStoryboard_Completed(object sender, EventArgs e)
        {
            CloseStoryboard.Completed -= new EventHandler(CloseStoryboard_Completed);

            OpenStoryboard.Stop();
            OpenStoryboard.Children.Clear();

            ResetItems();

            CloseStoryboard.Stop();
            CloseStoryboard.Children.Clear();

            this.ChildWindowPopup.IsOpen = false;

            if (_reshowAppBar)
                ((PhoneApplicationPage)RootVisual.Content).ApplicationBar.IsVisible = true;
        }

        private void PrepareOpenStoryboard()
        {
            if (OpenStoryboard == null)
            {
                OpenStoryboard = new Storyboard();
            }

            if (this.titleTextBlock == null)
                return;

            //ResetItems();

            OpenStoryboard.Stop();
            OpenStoryboard.Children.Clear();

            int beginTime = 100;
            // Create animation for title textblock
            DoubleAnimation animationOpen = CreateRotationAnimation(titleTextBlock, -120, 0, 200, beginTime);
            DoubleAnimation animationOpacity = CreateOpacityAnimation(titleTextBlock, 0, 1, 200, beginTime);
            DoubleAnimation animationListBoxRegiaoOpacity = CreateOpacityAnimation(listBoxRegiao, 0, 1, 200, beginTime);
            OpenStoryboard.Children.Add(animationOpen);
            OpenStoryboard.Children.Add(animationOpacity);
            OpenStoryboard.Children.Add(animationListBoxRegiaoOpacity);

            PlaneProjection projection = null;

            if (titleTextBlock.Projection == null)
            {
                projection = new PlaneProjection();
                projection.RotationX = -90;
                projection.RotationY = 0;
                titleTextBlock.Projection = projection;
            }
            else
            {
                ((PlaneProjection)titleTextBlock.Projection).RotationX = -90;
            }

            beginTime += 30;

            VirtualizingStackPanel panel = this.listBoxRegiao.GetVisualDescendents<VirtualizingStackPanel>(true).FirstOrDefault();
            this.visibleItems = panel.GetVisualDescendents<ListBoxItem>(true).GetVisibleItems(listBoxRegiao, Orientation.Vertical).ToList();

            for (int i = 0; i < visibleItems.Count; i++)
            {
                ListBoxItem item = visibleItems[i];

                if (item != null)
                {
                    //if (item.Projection != null)
                    //{
                    //    item.Projection = null;
                    //}
                    // Create projection

                    if (item.Projection == null)
                    {
                        projection = new PlaneProjection();
                        projection.RotationX = -90;
                        projection.RotationY = 0;
                        item.Projection = projection;
                    }
                    else
                    {
                        ((PlaneProjection)item.Projection).RotationX = -90;
                    }

                    item.CacheMode = new BitmapCache();
                    // Create open animation
                    animationOpen = CreateRotationAnimation(item, -90, 0, 200, beginTime);
                    animationOpacity = CreateOpacityAnimation(item, 0, 1, 200, beginTime);
                    // Add it to the storyboard
                    OpenStoryboard.Children.Add(animationOpen);
                    OpenStoryboard.Children.Add(animationOpacity);
                    // Increment the begin time for animation
                    beginTime += 30;
                }
            }



        }

        #endregion

        #region properties

        internal Popup ChildWindowPopup
        {
            get;
            private set;
        }

        private static PhoneApplicationFrame RootVisual
        {
            get
            {
                return Application.Current == null ? null : Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }

        public object SelectedItem
        {
            get
            {
                return this.listBoxRegiao.SelectedItem;
            }
            set
            {
                this.listBoxRegiao.SelectedItem = value;
            }
        }

        public int SelectedIndexRegiao
        {
            get
            {
                return this.listBoxRegiao.SelectedIndex;
            }
            set
            {

                this.selectedIndexRegiao = value;
                if (this.listBoxRegiao != null)
                {
                    this.listBoxRegiao.SelectedIndex = selectedIndexRegiao;
                }

            }
        }

        public IEnumerable ItemSource
        {
            get
            {
                return itemSource;
            }
            set
            {
                this.itemSource = value;
                if (this.listBoxRegiao != null)
                {
                    this.listBoxRegiao.ItemsSource = this.itemSource;
                }
            }
        }



        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;

                if (this.titleTextBlock != null)
                {
                    this.titleTextBlock.Text = this.title;
                }
            }
        }


        #endregion

        #region public methods

        static bool _reshowAppBar;
        public void Show()
        {
            if (this.ChildWindowPopup == null)
            {
                this.ChildWindowPopup = new Popup();

                try
                {

                    this.ChildWindowPopup.Child = this;
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("The control is already shown.");
                }
            }

            if (this.ChildWindowPopup != null && Application.Current.RootVisual != null)
            {
                //SystemTray.IsVisible = false;
                // Show popup
                this.ChildWindowPopup.IsOpen = true;
            }

            if (RootVisual != null)
            {
                var page = ((PhoneApplicationPage)RootVisual.Content);

                if (page.ApplicationBar != null && page.ApplicationBar.IsVisible)
                {
                    _reshowAppBar = true;
                    page.ApplicationBar.IsVisible = false;
                }

                // Hook up into the back key press event of the current page
                page.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(PickerBoxDialogRegiao_BackKeyPress);
            }
        }

        #endregion

    }
}

