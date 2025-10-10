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


        private bool IsLeftButtonDown = false;
        private Point OldPoisition;

        public Vector Offset
        {
            get => (Vector)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        public DetectDragBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeave += OnMouseLeave;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeave -= OnMouseLeave;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsLeftButtonDown = true;
            OldPoisition = e.GetPosition(AssociatedObject);
            Offset = new Vector();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsLeftButtonDown = false;
            Offset = new Vector();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsLeftButtonDown) return;

            Point position = e.GetPosition(AssociatedObject);
            Offset = position - OldPoisition;
            OldPoisition = position;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsLeftButtonDown = false;
            Offset = new Vector();
        }
    }
}
