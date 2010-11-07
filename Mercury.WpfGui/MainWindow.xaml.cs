using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Mercury.WpfGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();
            /*
            ctl1.Focus();
            Keyboard.Focus(ctl1);
            FocusManager.SetFocusedElement(this, ctl1); */
        }

        private void DragAttempt(object sender, MouseButtonEventArgs e)
        {
            wnd.DragMove();
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(ctl1);
            FocusHelper.Focus(ctl1);
            Keyboard.Focus(ctl1);
            //Keyboard.Focus(ctl1);
            //Keyboard.Focus(foo);
            //FocusManager.SetFocusedElement(this, ctl1);
        }
    }
}