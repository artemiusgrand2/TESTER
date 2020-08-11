using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
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
using TESTER.Models;
using TESTER.ServerListen;

namespace TESTER
{

    
    /// <summary>
    /// делегат сообщющий о ходе загрузки
    /// </summary>
    /// <param name="info">инфо загрузки</param>
    public delegate void Info(string info);

    public delegate bool  FindEl(string nameImpuls, string filter);

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Переменные

        ScaleTransform scaletransform = new ScaleTransform(1, 1); 
        /// <summary>
        /// индекс последней из выбранных станций
        /// </summary>
        int lastSelectIndexStation = -1;
        /// <summary>
        /// номер последней из выбранных станций
        /// </summary>
        int lastSelectStation = -1;
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
        ListenController server;
        /// <summary>
        /// таймер мигания кнопки подтверждения
        /// </summary>
        System.Timers.Timer _timer_mig;
        /// <summary>
        /// хук перехвата мыши
        /// </summary>
        UserActivityHook _hook = null;
        /// <summary>
        /// Тип работы
        /// </summary>
        public static TypeWork TypeWork { get; set; }
        /// <summary>
        /// результат последнего запроса
        /// </summary>
        private List<string> last_select = new List<string>();
        /// <summary>
        /// основное отрицательное изменение масштаба 
        /// </summary>
        double scrollminus = 0.95;
        /// <summary>
        /// основное положительное изменение масштаба
        /// </summary>
        double scrollplus = 1.05;

        //int step = 1;

        //DateTime timeSend = DateTime.MinValue;

        ObservableCollection<RowTable> table = new ObservableCollection<RowTable>() ;

        List<DataStationView> panels = new List<DataStationView>();

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

        bool isDifferences = false;

        public bool IsDifferences
        {
            get
            {
                return isDifferences;
            }

            set
            {
                if (value != isDifferences)
                {
                    isDifferences = value;
                    OnPropertyChanged("IsDifferences");
                }
            }
        }


        bool isShowFindResult = false;

        public bool IsShowFindResult
        {
            get
            {
                return isShowFindResult;
            }

            set
            {
                if (value != isShowFindResult)
                {
                    isShowFindResult = value;
                    OnPropertyChanged("IsShowFindResult");
                }
            }
        }

        bool IsRunShowFindResult = false;

        #endregion 

        public MainWindow()
        {
            InitializeComponent();
            if (!App.Close)
                Start();
        }

        private void Start()
        {
            try
            {
                //panelTS.RenderTransform = scaletransform;
                panelTU.RenderTransform = scaletransform;
                TypeWork = TypeWork.fromServer;
                _timer_mig = new System.Timers.Timer(500);
                _timer_mig.Elapsed += timer_mig_tick;
                _timer_mig.Start();
                comboBox_all_impuls.Items.Add(ProjectConstants.notcontrol);
                comboBox_all_impuls.Items.Add(ProjectConstants.pasiv);
                comboBox_all_impuls.Items.Add(ProjectConstants.activ);
                comboBox_all_impuls.Items.Add(ProjectConstants.sever);
                //
                panels.Add(new DataStationView(panel1)); panels.Add(new DataStationView(panel2));
                panels.Add(new DataStationView(panel3)); panels.Add(new DataStationView(panel4));
                panels[0].IsShow = true;
               
                //
                ImpulsesClient.ConnectDisconnectionServer += ConnectCloseServer;
                ImpulsesClient.NewData += NewInfomation;
                checkBox_reserve.IsChecked = !App.IsServer1;
                SetNameColorServer(App.IsServer1);
                //
                if (App.CountServer == 1)
                    checkBox_reserve.IsEnabled = false;
                server = new ListenController(checkBox_work_view.IsChecked.Value, (!checkBox_reserve.IsChecked.Value) ? App.Config.AppSettings.Settings["server1"].Value : App.Config.AppSettings.Settings["server2"].Value);
                FullStations(server.ProjectTester.Station);
                server.Start();
                //
                TableTest.ItemsSource = server.ProjectTester.TestFile.Scripts;
                server.ProjectTester.TestFile.NewState += NewStateTest;
                server.ProjectTester.TestFile.NewNumberState += NewCurrentTest;
                server.ProjectTester.TestFile.UpdateState += UpdateColor;
                server.ProjectTester.TestFile.NameTest += GetNameTest;
                server.ProjectTester.TestFile.NotVisible += NotVisible;
                server.ProjectTester.TestFile.CurrentSecondWait += CurrentTimeWait;
                //_hook = new UserActivityHook();
                //_hook.KeyDown += new System.Windows.Forms.KeyEventHandler(MyKeyDown);
                //
                IsDifferences = App.IsDifferences;
                textBox_name_impuls.Text = App.Filter;
                if (!string.IsNullOrEmpty(App.Filter))
                    IsShowFindResult = true;
                //
                if (App.StationsNumber.Count > 0)
                {
                    switch (App.StationsNumber.Count)
                    {
                        case 2:
                            {
                                TwoTable.IsChecked = true;
                                radioFunction(TwoTable);
                            }
                            break;
                        case 4:
                            {
                                FourTable.IsChecked = true;
                                radioFunction(FourTable);
                            }
                            break;
                    }
                    //
                    foreach (var station in App.StationsNumber)
                    {
                        switch (App.StationsNumber.IndexOf(station))
                        {
                            case 0:
                                {
                                    selectStation1.SelectedIndex = Table.IndexOf(Table.Where(x => x.Station == station).FirstOrDefault());
                                }
                                break;
                            case 1:
                                {
                                    selectStation2.SelectedIndex = Table.IndexOf(Table.Where(x => x.Station == station).FirstOrDefault());
                                }
                                break;
                            case 2:
                                {
                                    selectStation3.SelectedIndex = Table.IndexOf(Table.Where(x => x.Station == station).FirstOrDefault());
                                }
                                break;
                            case 3:
                                {
                                    selectStation4.SelectedIndex = Table.IndexOf(Table.Where(x => x.Station == station).FirstOrDefault());
                                }
                                break;
                        }

                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); /*_hook.Stop();*/ }
        }

        private void MyKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)     
        {
            if (e.KeyData == System.Windows.Forms.Keys.Enter)
            {
                if(IsActive && !textBox_name_impuls.IsFocused  && !start_test.IsFocused)
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

        private void SetNameColorServer(bool IsServer1)
        {
            if (IsServer1)
            {
                if (App.Config.AppSettings.Settings.AllKeys.Contains("name1"))
                    info_status_test.Content = App.Config.AppSettings.Settings["name1"].Value;

                if (App.Config.AppSettings.Settings.AllKeys.Contains("fpColor1"))
                    info_status_test.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Config.AppSettings.Settings["fpColor1"].Value));
            }
            else
            {
                if (App.Config.AppSettings.Settings.AllKeys.Contains("name2"))
                    info_status_test.Content = App.Config.AppSettings.Settings["name2"].Value;

                if (App.Config.AppSettings.Settings.AllKeys.Contains("fpColor2"))
                    info_status_test.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(App.Config.AppSettings.Settings["fpColor2"].Value));
            }
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
                    foreach (KeyValuePair<int, Stations> value in server.ProjectTester.CollectionStations)
                    {
                        if (TypeWork == TypeWork.fromServer)
                        {
                            var findModel = panels.Where(x => x.CurrentStation == value.Key).ToList();
                            var impulsesUpdates = new List<Impuls>();
                            foreach (var impuls in value.Value.CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
                            {
                                var newState = GetState(value.Key, impuls.Name);
                                if (newState != impuls.State)
                                {
                                    impuls.State = newState;
                                    impulsesUpdates.Add(impuls);
                                    findModel.ForEach(x =>
                                    {
                                        if (x.Collectionbuttons.ContainsKey(impuls.Name))
                                        {
                                            if (x.Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                                            {
                                                SetState(impuls.State, x.Collectionbuttons[impuls.Name][impuls.Type]);
                                                x.Collectionbuttons[impuls.Name][impuls.Type].ToList().ForEach(y =>
                                                {
                                                    y.BorderBrush = Brushes.DarkBlue;
                                                    y.BorderThickness = new Thickness(3);
                                                });
                                            }
                                        }
                                    });
                                } 
                            }
                            //
                            if(impulsesUpdates.Count > 0)
                            {
                                foreach (var impuls in value.Value.CollectionImpulses.Where(x => x.Type == TypeImpuls.ts && !impulsesUpdates.Contains(x)))
                                {
                                    findModel.ForEach(x =>
                                    {
                                        if (x.Collectionbuttons.ContainsKey(impuls.Name))
                                        {
                                            if (x.Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                                            {
                                                x.Collectionbuttons[impuls.Name][impuls.Type].ToList().ForEach(y =>
                                                {
                                                    y.BorderBrush = Brushes.Brown;
                                                    y.BorderThickness = new Thickness(1);
                                                });
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    }
                }

                //if((DateTime.Now- timeSend).TotalSeconds > 7)
                //{
                //    switch (step)
                //    {
                //        case 1:
                //            {
                //                server.Client.SendImpulse("ГРИп", 76000012, ImpulseState.Execute); server.Client.SendImpulse("М2 М6", 76000012, ImpulseState.Execute);
                //                server.Client.SendImpulse("М1 Ч1о", 76000012, ImpulseState.Execute);
                //                step++;
                //            }
                //            break;
                //        case 2:
                //            {
                //                server.Client.SendImpulse("М6 Н1", 76000012, ImpulseState.Execute); server.Client.SendImpulse("М2 М6о", 76000012, ImpulseState.Execute);
                //                step++;
                //            }
                //            break;
                //        case 3:
                //            {
                //                server.Client.SendImpulse("М1 Ч1", 76000012, ImpulseState.Execute); server.Client.SendImpulse("М6 Н1о", 76000012, ImpulseState.Execute);
                //                server.Client.SendImpulse("ГРИп", 76000012, ImpulseState.Execute);
                //                step =1;
                //            }
                //            break;
                //    }
                //    //
                //    timeSend = DateTime.Now;
                //}
                //
                FindDifferences();
                if (!IsRunShowFindResult && IsShowFindResult)
                {
                    if (IsShowFindResult)
                        IsShowOnlyFunction();
                    IsRunShowFindResult = !IsRunShowFindResult;
                }
                //
                FullStations(server.ProjectTester.Station);
                ShowInfoImpuls();
            }
            catch { }
        }

        private void UpdateColor(int station, Impuls impuls)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (impuls != null)
                    {
                        panels.Where(x => x.CurrentStation == station).ToList().ForEach(x =>
                        {
                            if (x.Collectionbuttons.ContainsKey(impuls.Name))
                            {
                                if (x.Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                                {
                                    SetState(impuls.State, x.Collectionbuttons[impuls.Name][impuls.Type]);
                                }
                            }
                        });
                    }
                }
                ));
        }

        private void SetState(StateControl state, IList<Button> buttons)
        {
            foreach (Button button in buttons)
            {
                try
                {
                    switch (state)
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
                switch (server.SourceImpulsServer.data.Stations[station].TS.GetState(name))
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

        private void AllImpulsesSetValue(StateControl state, DataStationView stationModel)
        {
            if (server.ProjectTester.CollectionStations.ContainsKey(stationModel.CurrentStation))
            {
                foreach (var impuls in server.ProjectTester.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
                {
                    try
                    {
                        impuls.State = state;
                        SetState(impuls.State, stationModel.Collectionbuttons[impuls.Name][impuls.Type]);
                    }
                    catch { }
                }
            }
        }

        private void AllImpulsesSetValue(DataStationView stationModel)
        {
            if (server.ProjectTester.CollectionStations.ContainsKey(stationModel.CurrentStation))
            {
                foreach (var impuls in server.ProjectTester.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts))
                {
                    try
                    {
                        impuls.State = GetState(stationModel.CurrentStation, impuls.Name);
                        SetState(impuls.State, stationModel.Collectionbuttons[impuls.Name][impuls.Type]);
                    }
                    catch { }
                }
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
                    foreach (KeyValuePair<int, Stations> value in server.ProjectTester.CollectionStations)
                    {
                        if (server.SourceImpulsServer.data.Stations.ContainsKey(value.Key))
                        {
                            server.SourceImpulsServer.data.Stations[value.Key].TS.SetAllStatesInTable(ImpulseState.UncontrolledState, DateTime.Now);
                            server.SourceImpulsServer.data.Stations[value.Key].TS.RealCountImpuls = 0;
                            server.SourceImpulsServer.data.Stations[value.Key].TU.RealCountImpuls = 0;
                        }
                        //
                        if (!checkBox_work_view.IsChecked.Value)
                        {
                            if (TypeWork == TypeWork.fromServer)
                            {
                                var findModels = panels.Where(x => x.CurrentStation == value.Key).ToList();
                                foreach (var impuls in value.Value.CollectionImpulses)
                                {
                                    impuls.State = StateControl.notconrol;
                                    findModels.ForEach(x =>
                                    {
                                        if (x.Collectionbuttons.ContainsKey(impuls.Name))
                                        {
                                            SetState(impuls.State, x.Collectionbuttons[impuls.Name][impuls.Type]);
                                        }
                                    });
                                }
                            }
                        }
                    }
                    //
                    FullStations(server.ProjectTester.Station);
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
                        table.Add(new RowTable()
                        {
                            Name = value.Key,
                            Station = value.Value,
                            CountImpulsTs = server.SourceImpulsServer.data.Stations[value.Value].TS.GetCountProjectImpuls(),
                            CountImpulsTu = server.SourceImpulsServer.data.Stations[value.Value].TU.GetCountProjectImpuls(),
                            CountReceiveTs = server.SourceImpulsServer.data.Stations[value.Value].TS.RealCountImpuls,
                            NotcontrolCountImpuls = server.SourceImpulsServer.data.Stations[value.Value].TS.GetCountStateImpuls(ImpulseState.UncontrolledState)
                        });
                    }
                    //if (table.Count > 0)
                    //    comboBox_stations.SelectedIndex = 0;
                }
                else
                {
                    int index = 0;
                    foreach (KeyValuePair<string, int> value in _stations)
                    {
                        table[index].CountReceiveTs = server.SourceImpulsServer.data.Stations[value.Value].TS.RealCountImpuls;
                        table[index].NotcontrolCountImpuls = server.SourceImpulsServer.data.Stations[value.Value].TS.GetCountStateImpuls(ImpulseState.UncontrolledState);
                        index++;
                    }
                }
            }
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

        private void FullPanelImpulses(int stationnumber, DataStationView stationModel)
        {
            panelTU.Children.Clear();
            double width = 75;
            stationModel.Panel.Children.Clear();
            stationModel.CurrentStation = stationnumber;
            stationModel.Collectionbuttons.Clear();
            try
            {
                if (!checkBox_work_view.IsChecked.Value)
                    ShowInfoImpuls();
                if (server.ProjectTester.CollectionStations.ContainsKey(stationnumber))
                {
                    foreach (var impuls in server.ProjectTester.CollectionStations[stationnumber].CollectionImpulses)
                    {
                        Button button_impuls = new Button();
                        button_impuls.Height = 25;
                        button_impuls.Click += button_impuls_Click;
                        button_impuls.LostFocus += button_impuls_LostFocus;
                        button_impuls.GotFocus += button_impuls_GotFocus;
                        button_impuls.Content = impuls.Name;
                        button_impuls.ToolTip = impuls.ToolTip;
                        if (WidthText(button_impuls) > width)
                            width = WidthText(button_impuls);
                        if(impuls.Type == TypeImpuls.ts)
                        {
                            //if (!checkBox_work_view.IsChecked.Value)
                            //    SetState(new List<Button>() { button_impuls });
                            //else
                                SetState(impuls.State, new List<Button>() { button_impuls });
                        }
                        else
                            button_impuls.Background = m_colornotcontrol;
                        //
                        if (impuls.Type == TypeImpuls.ts)
                            stationModel.Panel.Children.Add(button_impuls);
                        else
                            panelTU.Children.Add(button_impuls);
                        //
                        if (!stationModel.Collectionbuttons.ContainsKey(impuls.Name))
                            stationModel.Collectionbuttons.Add(impuls.Name, new Dictionary<TypeImpuls, IList<Button>>());
                        if (!stationModel.Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                            stationModel.Collectionbuttons[impuls.Name].Add(impuls.Type, new List<Button>());
                        //
                        stationModel.Collectionbuttons[impuls.Name][impuls.Type].Add(button_impuls);
                    }
                    //
                    foreach (var typeImpuls in stationModel.Collectionbuttons)
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
        }

        private void ShowInfoImpuls()
        {
            if (server.SourceImpulsServer.data.Stations.ContainsKey(lastSelectStation))
            {
                Title = $"Tестер (Количество импульсов TC - {server.SourceImpulsServer.data.Stations[lastSelectStation].TS.GetCountProjectImpuls()}|ТУ - {server.SourceImpulsServer.data.Stations[lastSelectStation].TU.GetCountProjectImpuls()})" +
                     $"|полученных ТС {server.SourceImpulsServer.data.Stations[lastSelectStation].TS.RealCountImpuls}| неконтр {server.SourceImpulsServer.data.Stations[lastSelectStation].TS.GetCountStateImpuls(ImpulseState.UncontrolledState)}";
            }
        }


        /// <summary>
        ///  Поиск импульса
        /// </summary>
        /// <param name="name">название импульса</param>
        private void FindImpuls(string name)
        {
            ClearLastSelect(IsShowFindResult);
            if (!string.IsNullOrEmpty(name))
            {
                try
                {
                    foreach (var impuls in panels.SelectMany(x => x.Collectionbuttons))
                    {
                        var common = impuls.Value.Values.SelectMany(x => x).ToList();
                        if (/*(del.EndInvoke(del.BeginInvoke(impuls.Key.ToUpper(), name.ToUpper(), null, null))*/Helps.Find(impuls.Key.ToUpper(), name.ToUpper()))
                        {
                            if (common.Where(x => x.Visibility == Visibility.Visible).Count() != common.Count)
                                continue;
                            SelectButton(common, true, IsShowFindResult);
                            if (!last_select.Contains(impuls.Key))
                                last_select.Add(impuls.Key);
                            if (last_select.Count == 1)
                                textBox_name_impuls.Foreground = Brushes.Green;
                        }
                        else
                            SelectButton(common, false, IsShowFindResult);
                    }
                }
                catch (ArgumentException){ }
                //
                if (last_select.Count == 0)
                    textBox_name_impuls.Foreground = Brushes.Red;
            }
            //
            if (name.Length > 0)
                CountFindElement.Text = last_select.Count.ToString();
            textBox_name_impuls.Focus();
           
        }

        private void ClearLastSelect(bool isonly)
        {
            if (isonly)
            {
                panels.Where(x => x.IsShow).ToList().ForEach(x => 
                {
                    foreach (var impuls in x.Collectionbuttons)
                    {
                        SelectButton(x.Collectionbuttons[impuls.Key].Values.SelectMany(y => y).ToList(), true, IsShowFindResult);
                    }
                });
            }
            else
            {
                panels.Where(x => x.IsShow).ToList().ForEach(x =>
                {
                    foreach (string impuls in last_select)
                    {
                        if (x.Collectionbuttons.ContainsKey(impuls))
                        {
                            SelectButton(x.Collectionbuttons[impuls].Values.SelectMany(y => y).ToList(), false, IsShowFindResult);
                        }
                    }
                });
            }
            last_select.Clear();
            CountFindElement.Text = string.Empty;
        }

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
                server.ProjectTester.LoadTest(opendialog.FileName);
                TableTest.Items.Refresh();
            }
            //
            if (server.ProjectTester.TestFile.Scripts.Count > 0)
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

        private void FindDifferences()
        {
            if (IsDifferences)
            {
                var starr = DateTime.Now;
                var stationsModel = panels.Where(x => x.IsShow && x.CurrentStation != -1).ToList();
                var maxTS = stationsModel.Max(x => x.Collectionbuttons.Count);
                var stationModel = stationsModel.Where(x => x.Collectionbuttons.Count == maxTS).FirstOrDefault();
                var stationsNumber = stationsModel.Select(x => x.CurrentStation).Distinct().ToList();
                for (var index = 0; index < server.ProjectTester.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts).Count(); index++)
                {
                    var compares = new List<StateControl>();
                    stationsNumber.ForEach(x =>
                    {
                        var impulses = server.ProjectTester.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts).ToList();
                        if (impulses.Count > index)
                            compares.Add(impulses[index].State);
                    });

                    if (compares.Count == stationsNumber.Count  && compares.Distinct().Count() == 1)
                    {
                        //видимые элементы
                        stationsNumber.ForEach(x =>
                        {
                            var impulses = server.ProjectTester.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts);
                            if (impulses.Count() > index)
                                foreach(var station in stationsModel.Where(y => y.CurrentStation == x))
                                {
                                    station.SetVisiblity(impulses.ElementAt(index), Visibility.Collapsed);
                                }
                        });
                    }
                    else
                    {
                        stationsNumber.ForEach(x =>
                        {
                            var impulses = server.ProjectTester.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts);
                            if (impulses.Count() > index)
                                foreach (var station in stationsModel.Where(y => y.CurrentStation == x))
                                {
                                    station.SetVisiblity(impulses.ElementAt(index), Visibility.Visible);
                                }
                        });
                    }
                }
                var delta = DateTime.Now - starr;
                var d = 0;
                //
               // stationsModel.ForEach(x => x.Panel.UpdateLayout());
            }
            else
                IsShowOnlyFunction();
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
            panels.ForEach(x => x.Panel.Width = e.NewSize.Width / GridTS.ColumnDefinitions.Count);
            panelTU.Width = e.NewSize.Width;
        }


        private void comboBox_stations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboxElement = (ComboBox)sender;   
                if (comboxElement.SelectedIndex != -1)
                {
                    lastSelectIndexStation = comboxElement.SelectedIndex;
                    if (server.ProjectTester.CollectionStations.Count > 0)
                    {
                        var selectStation = (comboxElement.SelectedItem as RowTable).Station;
                        lastSelectStation = selectStation;
                        var selectPanel = (comboxElement.Name.IndexOf("1") != -1) ? panel1 : (comboxElement.Name.IndexOf("2") != -1) ? panel2 :
                                          (comboxElement.Name.IndexOf("3") != -1) ? panel3 : (comboxElement.Name.IndexOf("4") != -1) ? panel4 : null;
                        FullPanelImpulses(selectStation, panels.Where(x => x.Panel == selectPanel).FirstOrDefault());
                        last_select.Clear();
                        FindImpuls(textBox_name_impuls.Text);
                    }
                }
            }
            catch { };
        }

        private void button_impuls_Click(object sender, RoutedEventArgs e)
        {
            var selectButton = (Button)sender;
            if (selectButton != null)
            {
                string name = selectButton.Content.ToString();
                if(tabImpulses.SelectedIndex == 0)
                {
                    var station = panels.Where(x => x.IsFindButton(selectButton)).FirstOrDefault().CurrentStation;
                    if (!IsShowFindResult)
                        ClearLastSelect(IsShowFindResult);
                    if (checkBox_work_view.IsChecked.Value)
                    {
                        foreach (var impuls in server.ProjectTester.CollectionStations[station].CollectionImpulses.Where(x=>x.Type== TypeImpuls.ts))
                        {
                            if (impuls.Name == name)
                            {
                                switch (impuls.State)
                                {
                                    case StateControl.activ:
                                        {
                                            selectButton.Background = m_pasivecolor;
                                            impuls.State = StateControl.pasiv;
                                            server.SourceImpulsServer.data.Stations[station].TS.set_state(name, ImpulseState.PassiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.pasiv:
                                        {
                                            selectButton.Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.SourceImpulsServer.data.Stations[station].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.notconrol:
                                        {
                                            selectButton.Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.SourceImpulsServer.data.Stations[station].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
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
                    server.SourceImpulsServer.SendImpulse(name, lastSelectStation, ImpulseState.Execute);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.Close)
            {
               // _hook.Stop();
                server.Stop();
                _timer_mig.Stop();
                server.ProjectTester.TestFile.StopTest(string.Empty);
                ImpulsesClient.ConnectDisconnectionServer -= ConnectCloseServer;
                ImpulsesClient.NewData -= NewInfomation;
            }
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
            if (checkBox_work_view.IsChecked.Value)
                TypeWork = TypeWork.autonomy;
            else
                TypeWork = TypeWork.fromServer;
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
                        panels.Where(x => x.IsShow).ToList().ForEach(x => AllImpulsesSetValue(StateControl.activ, x));
                        break;
                    case ProjectConstants.pasiv:
                        panels.Where(x => x.IsShow).ToList().ForEach(x => AllImpulsesSetValue(StateControl.pasiv, x));
                        break;
                    case ProjectConstants.notcontrol:
                        panels.Where(x => x.IsShow).ToList().ForEach(x => AllImpulsesSetValue(StateControl.notconrol, x));
                        break;
                    case ProjectConstants.sever:
                        panels.Where(x => x.IsShow).ToList().ForEach(x => AllImpulsesSetValue(x));
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
                    TableTest.ContextMenu = context.GetContextMenu(server.ProjectTester.TestFile, TableTest.SelectedIndex, TableTest, this);
                }
            }
        }

        private void start_test_Click(object sender, RoutedEventArgs e)
        {
            if (server != null)
            {
                if (server.ProjectTester.TestFile.IsStart)
                {
                    start_test.Content = "Старт тест";
                    server.ProjectTester.TestFile.StopTest("Стоп тест");
                }
                else
                {
                    if (IsCanStartTest())
                    {
                        server.ProjectTester.TestFile.StartTest(TableTest.SelectedIndex);
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
                if (server.ProjectTester.TestFile.IsStart)
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
                server.ProjectTester.TestFile.IsEnter = true;
                enter_test.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void MoveForStation(int move, ComboBox element)
        {
            if (move <0 && lastSelectIndexStation < element.Items.Count - 1)
            {
                lastSelectIndexStation++;
                element.SelectedIndex = lastSelectIndexStation;

            }
            else if (move > 0 && lastSelectIndexStation > 0)
            {
                lastSelectIndexStation--;
                element.SelectedIndex = lastSelectIndexStation;
            }
        }

        private void IsShowOnlyFunction()
        {
            panels.Where(x => x.IsShow).ToList().ForEach(x =>
            {
                foreach (var impuls in x.Collectionbuttons)
                {
                    if (last_select.Contains(impuls.Key))
                        SelectButton(impuls.Value.Values.SelectMany(y => y).ToList(), (IsShowFindResult) ? false : true, false);
                    else
                        SelectButton(impuls.Value.Values.SelectMany(y => y).ToList(), (IsShowFindResult) ? false : true, true);
                }
                //
                x.Panel.UpdateLayout();
            });
        }

        private void IsShowOnlyResult_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender as CheckBox).IsChecked.Value)
                IsDifferences = false;
            IsShowOnlyFunction();
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
                //case Key.Up:
                //    {
                //        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                //            MoveForStation(1);
                //    }
                //    break;
                //case Key.Down:
                //    {
                //        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                //            MoveForStation(-1);
                //    }
                //    break;
            }
        }


        private void comboBox_stations_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoveForStation(e.Delta, (ComboBox)sender);
        }

        private void radioFunction(RadioButton radio)
        {
            if (radio.Content.ToString() == "1")
            {
                if (GridTS.RowDefinitions.Count > 1)
                    GridTS.RowDefinitions.RemoveAt(1);
                if (GridTS.ColumnDefinitions.Count > 1)
                {
                    GridTS.ColumnDefinitions.RemoveAt(1);
                    panels.ForEach(x => x.Panel.Width = x.Panel.Width * 2);
                }
                //
                panels.Where(x => panels.IndexOf(x) > 0).ToList().ForEach(x => x.IsShow = false);
                IsDifferences = false;
                FindDifferences();
            }
            else if (radio.Content.ToString() == "2")
            {
                if (GridTS.RowDefinitions.Count > 1)
                    GridTS.RowDefinitions.RemoveAt(1);
                //
                if (GridTS.ColumnDefinitions.Count == 1)
                {
                    GridTS.ColumnDefinitions.Add(new ColumnDefinition());
                    panels.ForEach(x => x.Panel.Width = x.Panel.Width / 2);
                }
                //
                panels[1].IsShow = true;
                panels.Where(x => panels.IndexOf(x) > 1).ToList().ForEach(x => x.IsShow = false);
            }
            else if (radio.Content.ToString() == "4")
            {
                if (GridTS.ColumnDefinitions.Count == 1)
                {
                    GridTS.ColumnDefinitions.Add(new ColumnDefinition());
                    panels.ForEach(x => x.Panel.Width = x.Panel.Width / 2);
                }

                if (GridTS.RowDefinitions.Count == 1)
                    GridTS.RowDefinitions.Add(new RowDefinition());
                //
                panels.ForEach(x => x.IsShow = true);
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            radioFunction((RadioButton)sender);
        }

        private void DifferencesCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.Value)
                IsShowFindResult = false;
            FindDifferences();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void checkBox_reserve_Click(object sender, RoutedEventArgs e)
        {
            SetNameColorServer(!checkBox_reserve.IsChecked.Value);
            server.SourceImpulsServer.Restart();
            server.SourceImpulsServer.ConnectionString = (!checkBox_reserve.IsChecked.Value) ? App.Config.AppSettings.Settings["server1"].Value : App.Config.AppSettings.Settings["server2"].Value;
        }
    }
}
