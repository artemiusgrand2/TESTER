using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using sdm.diagnostic_section_model;
using sdm.diagnostic_section_model.client_impulses;
using TESTER.Enums;
using TESTER.Constants;

namespace TESTER
{

    
    /// <summary>
    /// делегат сообщющий о ходе загрузки
    /// </summary>
    /// <param name="info">инфо загрузки</param>
    public delegate void Info(string info);

    public delegate bool  FindEl(string nameImpuls, string filter);

    public partial class MainWindow : Window
    {
        #region Переменные

        ScaleTransform scaletransform = new ScaleTransform(1, 1);
        /// <summary>
        /// коллекция импульсов станции
        /// </summary>
        Dictionary<string, IDictionary<TypeImpuls, IList<Button>>> m_collectionbuttons = new Dictionary<string, IDictionary<TypeImpuls, IList<Button>>>();
        /// <summary>
        /// текущая выбранная станция из коллекции
        /// </summary>
        int _selectstation = -1;
        /// <summary>
        /// текущая выбранная станция ее номер шестизначный
        /// </summary>
        int _selectnumberstation = -1;
        /// <summary>
        /// цвет неконтролируемого  импульса
        /// </summary>
        Brush m_colornotcontrol = new SolidColorBrush(Color.FromRgb(230, 230, 230));
        /// <summary>
        /// цвет автивного импульса
        /// </summary>
        Brush m_activecolor = Brushes.LightGreen;
        /// <summary>
        /// цвет пассивного импульса
        /// </summary>
        Brush m_pasivecolor = Brushes.Yellow;
        /// <summary>
        /// тестовый сервер импульсов
        /// </summary>
        Server server;
        /// <summary>
        /// таймер мигания кнопки подтверждения
        /// </summary>
        System.Timers.Timer _timer_mig;
        /// <summary>
        /// хук перехвата мыши
        /// </summary>
        UserActivityHook _hook = null;
        /// <summary>
        /// Автономная работа
        /// </summary>
        public static bool IsAutoWork { get; set; }
        /// <summary>
        /// результат последнего запроса
        /// </summary>
        private List<string> _last_select = new List<string>();
        /// <summary>
        /// основное отрицательное изменение масштаба 
        /// </summary>
        double scrollminus = 0.95;
        /// <summary>
        /// основное положительное изменение масштаба
        /// </summary>
        double scrollplus = 1.05;

        ObservableCollection<RowTable> table = new ObservableCollection<RowTable>();

        /// <summary>
        /// информация по станции
        /// </summary>
        public ObservableCollection<RowTable> Table
        {
            get
            {
                return table;
            }

            set
            {
                table = value;
            }
        }

        #endregion 

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            try
            {
                panelTS.RenderTransform = scaletransform;
                panelTU.RenderTransform = scaletransform;
                IsAutoWork = true;
                _timer_mig = new System.Timers.Timer(500);
                _timer_mig.Elapsed += timer_mig_tick;
                _timer_mig.Start();
                comboBox_all_impuls.Items.Add(ProjectConstants.notcontrol);
                comboBox_all_impuls.Items.Add(ProjectConstants.pasiv);
                comboBox_all_impuls.Items.Add(ProjectConstants.activ);
                comboBox_all_impuls.Items.Add(ProjectConstants.sever);
                //
                ImpulsesClient.ConnectDisconnectionServer += ConnectCloseServer;
                ImpulsesClient.NewData += NewInfomation;
                server = new Server(checkBox_work_view.IsChecked.Value); 
                server.Start();
                FullStations(server.Load.Station);
                TableTest.ItemsSource = server.Load.TestFile.Scripts;
                server.Load.TestFile.NewState += NewStateTest;
                server.Load.TestFile.NewNumberState += NewCurrentTest;
                server.Load.TestFile.UpdateState += UpdateColor;
                server.Load.TestFile.NameTest += GetNameTest;
                server.Load.TestFile.NotVisible += NotVisible;
                server.Load.TestFile.CurrentSecondWait += CurrentTimeWait;
                _hook = new UserActivityHook();
                _hook.KeyDown += new System.Windows.Forms.KeyEventHandler(MyKeyDown);
            }
            catch (Exception error) { MessageBox.Show(error.Message); _hook.Stop(); }
        }

        private void MyKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)     
        {
            if (e.KeyData == System.Windows.Forms.Keys.Enter)
            {
                if(IsActive && !textBox_name_impuls.IsFocused && !comboBox_stations.IsFocused && !start_test.IsFocused)
                    ClickEnter();
            }
        }

        private void timer_mig_tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (enter_test.Background != Brushes.Yellow)
                        enter_test.Background = Brushes.Yellow;
                    else enter_test.Background = Brushes.Silver;
                }
              ));
            }
            catch  { }
        }

        private void GetNameTest(string nametest)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                name_current_text.Text = nametest;
            }
              ));
        }

        private void NotVisible(System.Windows.Visibility visible1, System.Windows.Visibility visible2)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if(time_wait_next_test.Text != string.Empty)
                    time_wait_next_test.Text = string.Empty;
                if(enter_test.Visibility != visible1)
                    enter_test.Visibility = visible1;
                if(time_wait_next_test.Visibility != visible2)
                    time_wait_next_test.Visibility = visible2;
            }
              ));
        }

        private void CurrentTimeWait(string second)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                time_wait_next_test.Text = second;
            }
              ));
        }

        private void NewStateTest(bool state, string info)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (state)
                        ElementEnabled(!state, System.Windows.Visibility.Visible);
                    else
                        ElementEnabled(!state, System.Windows.Visibility.Collapsed);
                    //
                    info_status_test.Content = info;
                }
                ));
        }

        private void ElementEnabled(bool enabled, System.Windows.Visibility visible)
        {
            checkBox_work_view.IsEnabled = enabled;
            comboBox_all_impuls.IsEnabled = enabled;
            panel_current_test.Visibility = visible;
            enter_test.Visibility = visible;
            if (visible == System.Windows.Visibility.Visible)
                button_load_test.Visibility = System.Windows.Visibility.Collapsed;
            else button_load_test.Visibility = System.Windows.Visibility.Visible;
            name_current_text.Text = string.Empty;

        }

        private void NewCurrentTest()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TableTest.Items.Refresh();
            }
            ));
        }

        /// <summary>
        /// Получаем новые данные по импульсам
        /// </summary>
        private void NewInfomation()
        {
            Dispatcher.Invoke(new Action(() => ProcessingData()));
        }

        /// <summary>
        /// Обработываем новые данные
        /// </summary>
        private void ProcessingData()
        {
            try
            {
                //обрабатываем данные если мы работаем не автоматическом режиме
                if (!checkBox_work_view.IsChecked.Value)
                {
                    foreach (KeyValuePair<int, Stations> value in server.Load.CollectionStations)
                    {
                        if (value.Value.IsAllActive == ProjectConstants.sever)
                        {
                            foreach (var impuls in value.Value.CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
                            {
                                impuls.State = GetState(value.Key, impuls.Name);
                                if (_selectnumberstation == value.Key)
                                {
                                    if (m_collectionbuttons.ContainsKey(impuls.Name))
                                    {
                                        if (m_collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                                        {
                                            SetState(m_collectionbuttons[impuls.Name][impuls.Type]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //
                ShowInfoImpuls();
            }
            catch { }
        }

        private void UpdateColor(int station, Impuls impuls)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (station == _selectnumberstation && impuls != null)
                    {
                        if (m_collectionbuttons.ContainsKey(impuls.Name))
                        {
                            if (m_collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                            {
                                SetState(m_collectionbuttons[impuls.Name][impuls.Type], impuls);
                            }
                        }
                    }
                }
                ));
        }

        private void SetState(IList<Button> buttons)
        {
            foreach (Button button in buttons)
            {
                try
                {
                    switch (server.Client.data.Stations[_selectnumberstation].TS.GetState(button.Content.ToString()))
                    {
                        case ImpulseState.ActiveState:
                            if (button.Background != m_activecolor)
                                button.Background = m_activecolor;
                            break;
                        case ImpulseState.PassiveState:
                            if (button.Background != m_pasivecolor)
                                button.Background = m_pasivecolor;
                            break;
                        default:
                            if (button.Background != m_colornotcontrol)
                                button.Background = m_colornotcontrol;
                            break;
                    }
                }
                catch
                {
                    if (button.Background != m_colornotcontrol)
                        button.Background = m_colornotcontrol;
                }
            }
        }

        private void SetState(IList<Button> buttons, Impuls impulse)
        {
            foreach (Button button in buttons)
            {
                try
                {
                    switch (impulse.State)
                    {
                        case StateControl.activ:
                            button.Background = m_activecolor;
                            break;
                        case StateControl.pasiv:
                            button.Background = m_pasivecolor;
                            break;
                        default:
                            button.Background = m_colornotcontrol;
                            break;
                    }
                }
                catch
                {
                    button.Background = m_colornotcontrol;
                }
            }
        }

        private void SetState(ImpulseState state, IList<Button> buttons)
        {
            foreach (Button button in buttons)
            {
                switch (state)
                {
                    case ImpulseState.ActiveState:
                        button.Background = m_activecolor;
                        break;
                    case ImpulseState.PassiveState:
                        button.Background = m_pasivecolor;
                        break;
                    default:
                        button.Background = m_colornotcontrol;
                        break;
                }
            }
        }

        private StateControl GetState(int station, string name)
        {
            try
            {
                switch (server.Client.data.Stations[station].TS.GetState(name))
                {
                    case ImpulseState.ActiveState:
                        return StateControl.activ;
                    case ImpulseState.PassiveState:
                        return StateControl.pasiv;
                    default:
                        return StateControl.notconrol;
                }
            }
            catch { return StateControl.notconrol; }
        }

        private void AllImpulsesSetValue(StateControl state, int stationnumber, string control)
        {
            if (server.Load.CollectionStations.ContainsKey(stationnumber))
            {
                foreach (var impuls in server.Load.CollectionStations[stationnumber].CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
                {
                    try
                    {
                        impuls.State = state;
                        SetState(m_collectionbuttons[impuls.Name][impuls.Type], impuls);
                    }
                    catch { }
                }
                //
                server.Load.CollectionStations[stationnumber].IsAllActive = control;
            }
        }

        private void AllImpulsesSetValue(int stationnumber, string control)
        {
            if (server.Load.CollectionStations.ContainsKey(stationnumber))
            {
                foreach (var impuls in server.Load.CollectionStations[stationnumber].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts))
                {
                    try
                    {
                        impuls.State = GetState(stationnumber, impuls.Name);
                        SetState(m_collectionbuttons[impuls.Name][impuls.Type]);
                    }
                    catch { }
                }
                //
                server.Load.CollectionStations[stationnumber].IsAllActive = control;
            }
        }
        /// <summary>
        /// Реагируем на подключение к серверу импульсов
        /// </summary>
        private void ConnectCloseServer()
        {
            Dispatcher.Invoke(new Action(() => ServerClose()));
        }

        private void ServerClose()
        {
            try
            {
              
                if (!ImpulsesClient.Connect)
                {
                    foreach (KeyValuePair<int, Stations> value in server.Load.CollectionStations)
                    {
                        if (server.Client.data.Stations.ContainsKey(value.Key))
                        {
                            server.Client.data.Stations[value.Key].TS.SetAllStatesInTable(ImpulseState.UncontrolledState, DateTime.Now);
                            server.Client.data.Stations[value.Key].TS.RealCountImpuls = 0;
                            server.Client.data.Stations[value.Key].TU.RealCountImpuls = 0;
                        }
                        //
                        if (!checkBox_work_view.IsChecked.Value)
                        {
                            if (value.Value.IsAllActive == ProjectConstants.sever)
                            {
                                foreach (var impuls in value.Value.CollectionImpulses)
                                {
                                    impuls.State = StateControl.notconrol;
                                    if (_selectnumberstation == value.Key)
                                    {
                                        if (m_collectionbuttons.ContainsKey(impuls.Name))
                                        {
                                            SetState(m_collectionbuttons[impuls.Name][impuls.Type]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //
                ShowInfoImpuls();
            }
            catch {}
        }

        private void FullStations(Dictionary<string, int> _stations)
        {
            if (_stations != null && _stations.Count > 0)
            {
                if (table.Count == 0)
                {
                    foreach (KeyValuePair<string, int> value in _stations)
                    {
                        table.Add(new RowTable() { Name = value.Key, Station = value.Value, CountImpuls = server.Load.CollectionStations[value.Value].CollectionImpulses.Count, NotcontrolCountImpuls = GetNotControl(server.Load.CollectionStations[value.Value].CollectionImpulses) });
                    }
                    if (table.Count > 0)
                        comboBox_stations.SelectedIndex = 0;
                }
                else
                {
                    int index = 0;
                    foreach (KeyValuePair<string, int> value in _stations)
                    {
                        table[index].NotcontrolCountImpuls = GetNotControl(server.Load.CollectionStations[value.Value].CollectionImpulses);
                        index++;
                    }
                }
            }
        }

        private string GetNotControlImpulses(IDictionary<int, Impuls> impulses)
        {
            int count_notcontol = 0;
            foreach (var imp in impulses.Values)
            {
                if (imp.State == StateControl.notconrol)
                    count_notcontol++;
            }
            //
            return string.Format("{0} - {1}", impulses.Values.Count, count_notcontol);
        }

        private int GetNotControl(IList<Impuls> impulses)
        {
            int count_notcontol = 0;
            foreach (var imp in impulses.Where(x=>x.Type == TypeImpuls.ts).ToList())
            {
                if (imp.State == StateControl.notconrol)
                    count_notcontol++;
            }
            //
            return count_notcontol;
        }

        /// <summary>
        /// ширина текста в пикселях
        /// </summary>
        /// <param name="textblock"></param>
        /// <returns></returns>
        private double WidthText(Button textblock)
        {
            Typeface typeface = new Typeface(textblock.FontFamily, textblock.FontStyle, textblock.FontWeight, textblock.FontStretch);
            System.Windows.Media.Brush brush = new SolidColorBrush();
            FormattedText formatedText = new FormattedText(textblock.Content.ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, textblock.FontSize, brush);
            return formatedText.Width;
        }

        private void FullPanelImpulses(int stationnumber)
        {
            m_collectionbuttons.Clear();
            panelTS.Children.Clear();
            panelTU.Children.Clear();
            double width = 75;
            try
            {
                if (!checkBox_work_view.IsChecked.Value)
                    ShowInfoImpuls();
                if (server.Load.CollectionStations.ContainsKey(stationnumber))
                {
                    foreach (var impuls in server.Load.CollectionStations[stationnumber].CollectionImpulses)
                    {
                        Button button_impuls = new Button();
                        button_impuls.Height = 25;
                        button_impuls.Click += button_impuls_Click;
                        button_impuls.LostFocus += button_impuls_LostFocus;
                        button_impuls.GotFocus += button_impuls_GotFocus;
                        button_impuls.Content = impuls.Name;
                        if (WidthText(button_impuls) > width)
                            width = WidthText(button_impuls);
                        if(impuls.Type == TypeImpuls.ts)
                        {
                            if (!checkBox_work_view.IsChecked.Value)
                                SetState(new List<Button>() { button_impuls });
                            else
                                SetState(new List<Button>() { button_impuls }, impuls);
                        }
                        else
                            button_impuls.Background = m_colornotcontrol;
                        //
                        if (impuls.Type == TypeImpuls.ts)
                            panelTS.Children.Add(button_impuls);
                        else
                            panelTU.Children.Add(button_impuls);
                        //
                        if (!m_collectionbuttons.ContainsKey(impuls.Name))
                            m_collectionbuttons.Add(impuls.Name, new Dictionary<TypeImpuls, IList<Button>>());
                        if (!m_collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                            m_collectionbuttons[impuls.Name].Add(impuls.Type, new List<Button>());
                         //
                         m_collectionbuttons[impuls.Name][impuls.Type].Add(button_impuls);
                    }
                    //
                    foreach (var typeImpuls in m_collectionbuttons)
                    {
                        foreach (var buttonList in typeImpuls.Value)
                        {
                            foreach (var button in buttonList.Value)
                            {
                                button.Width = width;
                            }
                        }
                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
            //
            comboBox_all_impuls.Text = server.Load.CollectionStations[stationnumber].IsAllActive;
        }

        private void ShowInfoImpuls()
        {
            if (server.Client.data.Stations.ContainsKey(_selectnumberstation))
            {
                Title = string.Format("Tестер (Количество импульсов в проекте - {0}, Количество импульсов полученных с сервера - {1})", server.Client.data.Stations[_selectnumberstation].TS.GetCountProjectImpuls(),
                    server.Client.data.Stations[_selectnumberstation].TS.RealCountImpuls);
            }
        }


        /// <summary>
        ///  Поиск импульса
        /// </summary>
        /// <param name="name">название импульса</param>
        private void FindImpuls(string name)
        {
            ClearLastSelect(IsShowOnlyResult.IsChecked.Value);
            if (!string.IsNullOrEmpty(name))
            {
                FindEl del = new FindEl(Find);
                foreach (var impuls in m_collectionbuttons)
                {
                    var common = impuls.Value.Values.SelectMany(x => x).ToList();
                    if (/*(del.EndInvoke(del.BeginInvoke(impuls.Key.ToUpper(), name.ToUpper(), null, null))*/Find(impuls.Key.ToUpper(), name.ToUpper()))
                    {
                        SelectButton(common, true, IsShowOnlyResult.IsChecked.Value);
                        if (!_last_select.Contains(impuls.Key))
                            _last_select.Add(impuls.Key);
                        if (_last_select.Count == 1)
                            textBox_name_impuls.Foreground = Brushes.Green;
                    }
                    else
                        SelectButton(common, false, IsShowOnlyResult.IsChecked.Value);
                }
                //
                if (_last_select.Count == 0)
                    textBox_name_impuls.Foreground = Brushes.Red;
            }
            //
            if (name.Length > 0)
                CountFindElement.Text = _last_select.Count.ToString();
            textBox_name_impuls.Focus();
           
        }

        private bool Find(string nameImpuls, string filter)
        {
            string[] filters = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string f in filters)
            {
                if (FindElement(nameImpuls, f))
                    return true;
            }
            //
            return false;
        }

        private bool FindElement(string nameImpuls, string filter)
        {
            //nameImpuls = "1-2048";
            //filter = "*?48";
            StringBuilder part_filter = new StringBuilder(string.Empty);
            int currentChar = 0;
            int countChar = 0;
            bool currentStar = false;
            bool isStar = false;
            //
            int index = -1;
            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] != '*')
                {
                    currentChar++;
                    countChar++;
                }
                //
                switch (filter[i])
                {
                    case '*':
                        {
                            currentStar = true;
                            isStar = true;
                            part_filter.Clear();
                        }
                        break;
                    case '?':
                        {
                            part_filter.Clear();
                            if ((!isStar && countChar != currentChar) || currentChar > nameImpuls.Length)
                                return false;
                            if (((i == (filter.Length - 1)) && currentChar < nameImpuls.Length))
                                return false;
                        }
                        break;
                    default:
                        {
                            index = -1;
                            part_filter.Append(filter[i]);
                            if ((index = nameImpuls.IndexOf(part_filter.ToString(0, part_filter.Length), currentChar - part_filter.Length)) != -1)
                            {
                                if ((currentChar - part_filter.Length) > index || (!currentStar && (index > currentChar -1)) /*)*/ )
                                    return false;
                                currentChar = index + part_filter.Length;
                                //if (i == 0 && index != 0)
                                //    return false;
                                //
                                if (!isStar && countChar != currentChar)
                                    return false;
                                if (((i == (filter.Length - 1)) && currentChar < nameImpuls.Length))
                                    return false;

                            }
                            else
                                return false;
                            //
                            currentStar = false;
                        }
                        break;
                }
            }
            //
            return true;
        }

        private void ClearLastSelect(bool isonly)
        {
            if (isonly)
            {
                foreach (var impuls in m_collectionbuttons)
                {
                    SelectButton(m_collectionbuttons[impuls.Key].Values.SelectMany(x=>x).ToList(), true, IsShowOnlyResult.IsChecked.Value);
                }
            }
            else
            {
                foreach (string impuls in _last_select)
                {
                    if (m_collectionbuttons.ContainsKey(impuls))
                    {
                        SelectButton(m_collectionbuttons[impuls].Values.SelectMany(x => x).ToList(), false, IsShowOnlyResult.IsChecked.Value);
                    }
                }
            }
            _last_select.Clear();
            CountFindElement.Text = string.Empty;
        }

        //private void ClearLastSelect(string except)
        //{
        //    if (!IsShowOnlyResult.IsChecked.Value)
        //    {
        //        foreach (string impuls in _last_select)
        //        {
        //            if (_collectionbuttons.ContainsKey(impuls))
        //            {
        //                SelectButton(_collectionbuttons[impuls], false, IsShowOnlyResult.IsChecked.Value);
        //            }
        //        }
        //        //foreach (string impuls in _last_select)
        //        //{
        //        //    if (_collectionbuttons.ContainsKey(impuls) && except != impuls)
        //        //    {
        //        //        SelectButton(_collectionbuttons[impuls], false, IsShowOnlyResult.IsChecked.Value);
        //        //    }
        //        //}
        //        //_last_select.Clear();
        //    }
        //}

        private void SelectButton(IList<Button> buttons, bool push, bool isonly)
        {
            foreach (Button button in buttons)
            {
                if (push)
                {
                    if (!isonly)
                    {
                        button.BorderBrush = Brushes.Green;
                        button.BorderThickness = new Thickness(3);
                    }
                    else
                        button.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    if (!isonly)
                    {
                        button.BorderBrush = Brushes.Brown;
                        button.BorderThickness = new Thickness(1);
                    }
                    else
                        button.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void LoadTest()
        {
            System.Windows.Forms.OpenFileDialog opendialog = new System.Windows.Forms.OpenFileDialog();
            opendialog.Filter = "Text files (*.txt)|*.txt|Csv files (*.csv)|*.csv|Xml files (*.xml)|*.xml";
            opendialog.Multiselect = false;
            if (opendialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                server.Load.LoadTest(opendialog.FileName);
                TableTest.Items.Refresh();
            }
            //
            if (server.Load.TestFile.Scripts.Count > 0)
            {
                panel_command.Visibility = System.Windows.Visibility.Visible;
                TableTest.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                panel_command.Visibility = System.Windows.Visibility.Collapsed;
                TableTest.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private bool IsCanStartTest()
        {
            if (!checkBox_work_view.IsChecked.Value)
            {
                MessageBox.Show("Включите автономный режим работы !!!");
                return false;
            }
            if (TableTest.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите тест из таблицы !!!");
                return false;
            }
            return true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            panelTS.Width = e.NewSize.Width;
            panelTU.Width = e.NewSize.Width;
        }

        private void comboBox_stations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBox_stations.SelectedIndex != -1 /*&& _selectstation != comboBox_stations.SelectedIndex*/)
                {
                    _selectstation = comboBox_stations.SelectedIndex;
                    if (server.Load.CollectionStations.Count > 0)
                    {
                        _selectnumberstation = (comboBox_stations.SelectedItem as RowTable).Station;
                        FullPanelImpulses(_selectnumberstation);
                        _last_select.Clear();
                        FindImpuls(textBox_name_impuls.Text);
                    }
                }
            }
            catch { };
        }

        private void button_impuls_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                string name = (sender as Button).Content.ToString();
                if(tabImpulses.SelectedIndex == 0)
                {
                    if (!IsShowOnlyResult.IsChecked.Value)
                        ClearLastSelect(/*name*/IsShowOnlyResult.IsChecked.Value);
                    if (checkBox_work_view.IsChecked.Value)
                    {
                        foreach (var impuls in server.Load.CollectionStations[_selectnumberstation].CollectionImpulses.Where(x=>x.Type== TypeImpuls.ts))
                        {
                            if (impuls.Name == name)
                            {
                                switch (impuls.State)
                                {
                                    case StateControl.activ:
                                        {
                                            (sender as Button).Background = m_pasivecolor;
                                            impuls.State = StateControl.pasiv;
                                            server.Client.data.Stations[_selectnumberstation].TS.set_state(name, ImpulseState.PassiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.pasiv:
                                        {
                                            (sender as Button).Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.Client.data.Stations[_selectnumberstation].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.notconrol:
                                        {
                                            (sender as Button).Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.Client.data.Stations[_selectnumberstation].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
                                        }
                                        break;
                                        // }
                                }
                                //}
                                return;
                            }
                        }
                    }
                }
                else if (tabImpulses.SelectedIndex == 1)
                {
                    server.Client.SendImpulse(name, _selectnumberstation, ImpulseState.Execute);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _hook.Stop();
            server.Stop();
            _timer_mig.Stop();
            server.Load.TestFile.StopTest(string.Empty);
            ImpulsesClient.ConnectDisconnectionServer -= ConnectCloseServer;
            ImpulsesClient.NewData -= NewInfomation;
        }

        private void textBox_name_impuls_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(textBox_name_impuls.Foreground != Brushes.Black)
                textBox_name_impuls.Foreground = Brushes.Black;
            FindImpuls(textBox_name_impuls.Text);
        }

        private void checkBox_work_view_Click(object sender, RoutedEventArgs e)
        {
            string answer = string.Empty;
            IsAutoWork = checkBox_work_view.IsChecked.Value;
            if (checkBox_work_view.IsChecked.Value)
                comboBox_all_impuls.Items.Remove(ProjectConstants.sever);
            else
            {
                answer = ProjectConstants.sever;
                comboBox_all_impuls.Items.Add(ProjectConstants.sever);
                comboBox_all_impuls.Text = answer;
                if (!ImpulsesClient.Connect)
                    ServerClose();
            }

            comboBox_all_impuls.Text = answer;
            foreach (KeyValuePair<int, Stations> value in server.Load.CollectionStations)
                value.Value.IsAllActive = answer;
        }

        private void button_impuls_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                (sender as Button).BorderBrush = Brushes.Green;
                (sender as Button).BorderThickness = new Thickness(3);
            }
        }

        private void button_impuls_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                (sender as Button).BorderBrush = Brushes.Brown;
                (sender as Button).BorderThickness = new Thickness(1);
            }
        }

        private void SelectItem()
        {
            if (comboBox_all_impuls.SelectedItem != null)
            {
                switch (comboBox_all_impuls.SelectedItem.ToString())
                {
                    case ProjectConstants.activ:
                        AllImpulsesSetValue(StateControl.activ, _selectnumberstation, ProjectConstants.activ);
                        break;
                    case ProjectConstants.pasiv:
                        AllImpulsesSetValue(StateControl.pasiv, _selectnumberstation, ProjectConstants.pasiv);
                        break;
                    case ProjectConstants.notcontrol:
                        AllImpulsesSetValue(StateControl.notconrol, _selectnumberstation, ProjectConstants.notcontrol);
                        break;
                    case ProjectConstants.sever:
                        AllImpulsesSetValue(_selectnumberstation, ProjectConstants.sever);
                        break;
                }
            }
        }

        private void comboBox_all_impuls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectItem();
        }

        private void button_load_test_Click(object sender, RoutedEventArgs e)
        {
            LoadTest();
        }

        private void TableTest_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (IsCanStartTest())
                {
                    MainContextMenu context = new MainContextMenu();
                    TableTest.ContextMenu = context.GetContextMenu(server.Load.TestFile, TableTest.SelectedIndex, TableTest, this);
                }
            }
        }

        private void start_test_Click(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                if (server.Load.TestFile.IsStart)
                {
                    start_test.Content = "Старт тест";
                    server.Load.TestFile.StopTest("Стоп тест");
                }
                else
                {
                    if (IsCanStartTest())
                    {
                        server.Load.TestFile.StartTest(TableTest.SelectedIndex);
                        enter_test.Focus();
                        TableTest.Items.Refresh();
                        start_test.Content = "Cтоп тест";
                    }
                }
            }
        }

        public void ClickStartStop()
        {
            if (server != null)
            {
                if (server.Load.TestFile.IsStart)
                    start_test.Content = "Cтоп тест";
                else
                    start_test.Content = "Старт тест";
            }
        }

        /// <summary>
        /// масштабируем все объекты
        /// </summary>
        /// <param name="scale_factor"></param>
        public void ModelScaleWheel(double scale_factor)
        {
            scaletransform.ScaleX *= scale_factor;
            scaletransform.ScaleY *= scale_factor;
        }

        private void enter_test_Click(object sender, RoutedEventArgs e)
        {
            ClickEnter();
        }

        private void ClickEnter()
        {
            if (server != null && enter_test.Visibility == System.Windows.Visibility.Visible)
            {
                server.Load.TestFile.IsEnter = true;
                enter_test.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void MoveForStation(int move)
        {
            if (move <0 && _selectstation < comboBox_stations.Items.Count - 1)
            {
                _selectstation++;
                comboBox_stations.SelectedIndex = _selectstation;

            }
            else if (move > 0 && _selectstation > 0)
            {
                _selectstation--;
                comboBox_stations.SelectedIndex = _selectstation;
            }
        }

        private void IsShowOnlyResult_Click(object sender, RoutedEventArgs e)
        {
            foreach (var impuls in m_collectionbuttons)
            {
                if (_last_select.Contains(impuls.Key))
                    SelectButton(impuls.Value.Values.SelectMany(x => x).ToList(), (IsShowOnlyResult.IsChecked.Value) ? false : true, false); 
                else
                    SelectButton(impuls.Value.Values.SelectMany(x => x).ToList(), (IsShowOnlyResult.IsChecked.Value) ? false : true, true);
                panelTS.UpdateLayout();
            }
        }

        private void panel_impuls_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Delta > 0)
                {
                    ModelScaleWheel(scrollplus);
                }
                else
                {
                    ModelScaleWheel(scrollminus);
                }
            }
        }

        private void panel_impuls_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        scaletransform.ScaleX = 1;
                        scaletransform.ScaleY = 1;
                    }
                    break;
                case Key.Up:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            MoveForStation(1);
                    }
                    break;
                case Key.Down:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            MoveForStation(-1);
                    }
                    break;
            }
        }

        private void comboBox_stations_DropDownOpened(object sender, EventArgs e)
        {
            //FullStations(server.Load.Station);
        }


        private void comboBox_all_impuls_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left)
            //    SelectItem();
        }

        private void comboBox_stations_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoveForStation(e.Delta);
        }
    }
}
