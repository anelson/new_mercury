using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Mercury.App;

namespace Mercury.WpfGui
{
    /// <summary>
    /// Interaction logic for CatalogItemPickerControl.xaml
    /// </summary>

    public partial class CatalogItemPickerControl : System.Windows.Controls.UserControl
    {
        IList<CatalogItem> _items;
        int _selectedItemIndex;

        public CatalogItemPickerControl()
        {
            InitializeComponent();

            _items = CatalogItemFactory.GetStartMenuItems();

            SelectItem(0, String.Empty);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            Trace.WriteLine(String.Format("Keyboard focused: {0}", IsKeyboardFocusWithin));
            Trace.WriteLine(String.Format("Current keyboard focus: {0}", Keyboard.FocusedElement));

            if (IsKeyboardFocusWithin)
            {
                _controlBorder.Style = (Style)Resources["controlBorderFocused"];
            }
            else
            {
                _controlBorder.Style = (Style)Resources["controlBorder"];
            }
        }

        private void SelectItem(int index, string substring)
        {
            _selectedItemIndex = index;
            _tb.Text = _items[index].Title;
            _icon.Source = CreateImageSourceFromStream(_items[index].IconStream);
            
        }

        private ImageSource CreateImageSourceFromStream(System.IO.Stream stream)
        {
            BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }
    }
}