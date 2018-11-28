using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TESTER
{
    class MainContextMenu
    {
        FileScript _test = null;
        int _number = -1;
        DataGrid _datagrid = null;
        MainWindow _main = null;

        public ContextMenu GetContextMenu(FileScript test, int number, DataGrid datagrid, MainWindow window)
        {
            _test = test;
            _number = number;
            _datagrid = datagrid;
            _main = window;
            ContextMenu result = new ContextMenu();
            //
            MenuItem itembstarttest = new MenuItem();
            itembstarttest.Header = "Начать тест";
            itembstarttest.Click += StartClick;
            result.Items.Add(itembstarttest);
            return result;
        }

      /// <summary>
        /// включаем или выключаем звуковые сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StartClick(object sender, RoutedEventArgs args)
        {
            if (_test != null)
            {
                _test.StartTest(_number);
                if (_datagrid != null)
                {
                    _datagrid.Items.Refresh();
                    _datagrid.SelectedIndex = 0;
                }
                //
                if (_main != null)
                    _main.ClickStartStop();
            }

        }
    }
}
