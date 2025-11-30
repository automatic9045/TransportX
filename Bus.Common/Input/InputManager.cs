using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bus.Common.Input
{
    public class InputManager
    {
        private readonly ConcurrentDictionary<Key, List<KeyObserver>> KeyObservers = new();

        public event EventHandler<MouseDragMoveEventArgs>? MouseDragMoved;
        public event EventHandler<MouseWheelEventArgs>? MouseWheel;

        public InputManager()
        {
        }

        public KeyObserver ObserveKey(Key key)
        {
            KeyObserver observer = new KeyObserver(key);
            observer.Disposing += (sender, e) => KeyObservers[key].Remove(observer);

            List<KeyObserver> list = KeyObservers.GetOrAdd(key, new List<KeyObserver>());
            list.Add(observer);
            return observer;
        }

        public void OnKeyDown(Key key)
        {
            if (KeyObservers.TryGetValue(key, out List<KeyObserver>? list))
            {
                foreach (KeyObserver observer in list)
                {
                    observer.Press();
                }
            }
        }

        public void OnKeyUp(Key key)
        {
            if (KeyObservers.TryGetValue(key, out List<KeyObserver>? list))
            {
                foreach (KeyObserver observer in list)
                {
                    observer.Release();
                }
            }
        }

        public void OnMouseDragMove(Vector offset, MouseButtonState leftButton, MouseButtonState middleButton, MouseButtonState rightButton)
        {
            MouseDragMoved?.Invoke(this, new MouseDragMoveEventArgs(offset, leftButton, middleButton, rightButton));
        }

        public void OnMouseWheel(int delta)
        {
            MouseWheel?.Invoke(this, new MouseWheelEventArgs(delta));
        }
    }
}
