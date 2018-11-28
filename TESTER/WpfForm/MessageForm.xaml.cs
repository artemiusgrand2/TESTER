using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TESTER
{
    enum ViewFormMessage
    {
        OK =0,
        OKCancel = 1
    }

    public partial class MessageForm : Window
    {

        public MessageForm(string message)
        {
            InitializeComponent();
            textBlock_message.Text = message;
        }

        public static void Show(string message)
        {
            MessageForm form = new MessageForm(message);
            form.ShowDialog();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
