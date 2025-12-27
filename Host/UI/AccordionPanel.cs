using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Chameleon.Host.UI
{
    public class AccordionPanel : Panel
    {
        public AccordionPanel()
        {
            Panel = new StackPanel();
            Header = new RelativePanel();
            Header.Background = new SolidColorBrush(Colors.Teal);
            Header.Height = TitleHeight;
            TitleText = new TextBlock();
            TitleText.Foreground = new SolidColorBrush(Colors.White);
            TitleText.FontFamily = new FontFamily("Lucida Console");
            TitleText.VerticalAlignment = VerticalAlignment.Center;
            TitleText.Margin = new Thickness(2);
            Header.Children.Add(TitleText);
            Panel.Children.Add(Header);
            Children.Add(Panel);
            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Call measure on all children
            Panel.Measure(availableSize);
            return Panel.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);
            Panel.Arrange(finalRect);
            return finalSize;
        }
        
        public string Title 
        {
            get => TitleText.Text;
            set => TitleText.Text = value;
        }

        public UIElement Content
        {
            get => _Content;
            set 
            {
                if (_Content != null)
                    Panel.Children.Remove(_Content);
                _Content = value;
                Panel.Children.Add(_Content);
            }
        }
        public UIElement _Content;
        public StackPanel Panel;
        public RelativePanel Header;
        public TextBlock TitleText;
        public int TitleHeight = 20;
    }
}
