using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Text;

namespace Mercury.WpfGui
{
    static class FocusHelper
    {
        private delegate void MethodInvoker();

        public static void Focus(UIElement element)
        {
            //Focus in a callback to run on another thread, ensuring the main UI thread is initialized by the
            //time focus is set
            ThreadPool.QueueUserWorkItem(delegate(Object foo) {
                UIElement elem = (UIElement)foo;
                elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (MethodInvoker)delegate()
                {
                    elem.Focus();
                    Keyboard.Focus(elem);
                });                
            }, element);
        }
    }
}
