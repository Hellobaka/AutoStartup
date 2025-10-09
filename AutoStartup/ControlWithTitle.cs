using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoStartup
{
    /// <summary>
    /// ToggleButtonWithTitle.xaml 的交互逻辑
    /// </summary>
    public partial class ControlWithTitle : ContentControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ControlWithTitle), new PropertyMetadata(""));

        public static readonly DependencyProperty TitleWidthProperty =
            DependencyProperty.Register("TitleWidth", typeof(int), typeof(ControlWithTitle), new PropertyMetadata(80, OnTitleWidthChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public int TitleWidth
        {
            get => (int)GetValue(TitleWidthProperty);
            set => SetValue(TitleWidthProperty, value);
        }

        private ColumnDefinition _titleColumn;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _titleColumn = GetTemplateChild("PART_TitleColumn") as ColumnDefinition;
            UpdateTitleWidth();
        }

        private static void OnTitleWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ControlWithTitle)d;
            ctrl.UpdateTitleWidth();
        }

        private void UpdateTitleWidth()
        {
            if (_titleColumn != null)
            {
                _titleColumn.Width = new GridLength(TitleWidth);
            }
        }
    }
}
