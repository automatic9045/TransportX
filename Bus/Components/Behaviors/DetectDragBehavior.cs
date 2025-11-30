using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Bus.Components.Behaviors
{
    internal class DetectDragBehavior : Behavior<UIElement>
    {
        public static DependencyProperty OffsetProperty = DependencyProperty.Register(nameof(Offset), typeof(Vector), typeof(DetectDragBehavior));
        public static DependencyProperty LeftButtonProperty = DependencyProperty.Register(nameof(LeftButton), typeof(MouseButtonState), typeof(DetectDragBehavior));
        public static DependencyProperty MiddleButtonProperty = DependencyProperty.Register(nameof(MiddleButton), typeof(MouseButtonState), typeof(DetectDragBehavior));
        public static DependencyProperty RightButtonProperty = DependencyProperty.Register(nameof(RightButton), typeof(MouseButtonState), typeof(DetectDragBehavior));


        private Point OldPoisition;

        public Vector Offset
        {
            get => (Vector)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public MouseButtonState LeftButton
        {
            get => (MouseButtonState)GetValue(LeftButtonProperty);
            set => SetValue(LeftButtonProperty, value);
        }

        public MouseButtonState MiddleButton
        {
            get => (MouseButtonState)GetValue(MiddleButtonProperty);
            set => SetValue(MiddleButtonProperty, value);
        }

        public MouseButtonState RightButton
        {
            get => (MouseButtonState)GetValue(RightButtonProperty);
            set => SetValue(RightButtonProperty, value);
        }

        private bool IsAnyButtonPressed
            => LeftButton == MouseButtonState.Pressed || MiddleButton == MouseButtonState.Pressed || RightButton == MouseButtonState.Pressed;

        public DetectDragBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += OnMouseButton;
            AssociatedObject.MouseUp += OnMouseButton;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDown -= OnMouseButton;
            AssociatedObject.MouseUp -= OnMouseButton;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeave -= OnMouseLeave;
        }

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (!IsAnyButtonPressed)
            {
                OldPoisition = e.GetPosition(AssociatedObject);
                Offset = new Vector();
            }

            LeftButton = e.LeftButton;
            MiddleButton = e.MiddleButton;
            RightButton = e.RightButton;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsAnyButtonPressed) return;

            Point position = e.GetPosition(AssociatedObject);
            Offset = position - OldPoisition;
            OldPoisition = position;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            LeftButton = MouseButtonState.Released;
            MiddleButton = MouseButtonState.Released;
            RightButton = MouseButtonState.Released;

            Offset = new Vector();
        }
    }
}
