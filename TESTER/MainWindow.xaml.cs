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

        bool IsRunDifferences = false;

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
                server = new Server(checkBox_work_view.IsChecked.Value);
                FullStations(server.Load.Station);
                server.Start();
           
                TableTest.ItemsSource = server.Load.TestFile.Scripts;
                server.Load.TestFile.NewState += NewStateTest;
                server.Load.TestFile.NewNumberState += NewCurrentTest;
                server.Load.TestFile.UpdateState += UpdateColor;
                server.Load.TestFile.NameTest += GetNameTest;
                server.Load.TestFile.NotVisible += NotVisible;
                server.Load.TestFile.CurrentSecondWait += CurrentTimeWait;
                _hook = new UserActivityHook();
                _hook.KeyDown += new System.Windows.Forms.KeyEventHandler(MyKeyDown);
                //
                IsDifferences = App.IsDifferences;
                textBox_name_impuls.Text = App.Filter;
                if (!string.IsNullOrEmpty(App.Filter))
                {
                    IsShowFindResult = true;
                }
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
            catch (Exception error) { MessageBox.Show(error.Message); _hook.Stop(); }
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
                        if (TypeWork == TypeWork.fromServer)
                        {
                            var findModel = panels.Where(x => x.CurrentStation == value.Key).ToList();
                            foreach (var impuls in value.Value.CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
                            {
                                impuls.State = GetState(value.Key, impuls.Name);
                                findModel.ForEach(x =>
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
                    }
                }
                //
                if (!IsRunDifferences && (isDifferences || IsShowFindResult))
                {
                    if (IsDifferences)
                        FindDifferences();
                    if (IsShowFindResult)
                        IsShowOnlyFunction();
                    IsRunDifferences = !IsRunDifferences;
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

        private void AllImpulsesSetValue(StateControl state, DataStationView stationModel)
        {
            if (server.Load.CollectionStations.ContainsKey(stationModel.CurrentStation))
            {
                foreach (var impuls in server.Load.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x=>x.Type == TypeImpuls.ts))
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
            if (server.Load.CollectionStations.ContainsKey(stationModel.CurrentStation))
            {
                foreach (var impuls in server.Load.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts))
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
                    //if (table.Count > 0)
                    //    comboBox_stations.SelectedIndex = 0;
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
            if (server.Client.data.Stations.ContainsKey(lastSelectStation))
            {
                Title = string.Format("Tестер (Количество импульсов в проекте - {0}, Количество импульсов полученных с сервера - {1})", server.Client.data.Stations[lastSelectStation].TS.GetCountProjectImpuls(),
                    server.Client.data.Stations[lastSelectStation].TS.RealCountImpuls);
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

        private void FindDifferences()
        {
            if (IsDifferences)
            {
                var stationsModel = panels.Where(x => x.IsShow && x.CurrentStation != -1).ToList();
                var maxTS = stationsModel.Max(x => x.Collectionbuttons.Count);
                var stationModel = stationsModel.Where(x => x.Collectionbuttons.Count == maxTS).FirstOrDefault();
                var stationsNumber = stationsModel.Select(x => x.CurrentStation).Distinct().ToList();
                for (var index = 0; index < server.Load.CollectionStations[stationModel.CurrentStation].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts).Count(); index++)
                {
                    var compares = new List<StateControl>();
                    stationsNumber.ForEach(x =>
                    {
                        var impulses = server.Load.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts).ToList();
                        if (impulses.Count > index)
                            compares.Add(impulses[index].State);
                    });

                    if (compares.Count == stationsNumber.Count  && compares.Distinct().Count() == 1)
                    {
                        //видимые элементы
                        stationsNumber.ForEach(x =>
                        {
                            var impulses = server.Load.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts).ToList();
                            if (impulses.Count > index)
                                stationsModel.Where(y => y.CurrentStation == x).ToList().ForEach(y =>
                                {
                                    y.SetVisiblity(impulses[index], Visibility.Collapsed);
                                });
                        });
                    }
                    else
                    {
                        stationsNumber.ForEach(x =>
                        {
                            var impulses = server.Load.CollectionStations[x].CollectionImpulses.Where(y => y.Type == TypeImpuls.ts).ToList();
                            if (impulses.Count > index)
                                stationsModel.Where(y => y.CurrentStation == x).ToList().ForEach(y =>
                            {
                                y.SetVisiblity(impulses[index], Visibility.Visible);
                            });
                        });
                    }
                }
                //
                stationsModel.ForEach(x => x.Panel.UpdateLayout());
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
                    if (server.Load.CollectionStations.Count > 0)
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
                var station = panels.Where(x => x.IsFindButton(selectButton)).FirstOrDefault().CurrentStation;
                if(tabImpulses.SelectedIndex == 0)
                {
                    if (!IsShowFindResult)
                        ClearLastSelect(IsShowFindResult);
                    if (checkBox_work_view.IsChecked.Value)
                    {
                        foreach (var impuls in server.Load.CollectionStations[station].CollectionImpulses.Where(x=>x.Type== TypeImpuls.ts))
                        {
                            if (impuls.Name == name)
                            {
                                switch (impuls.State)
                                {
                                    case StateControl.activ:
                                        {
                                            selectButton.Background = m_pasivecolor;
                                            impuls.State = StateControl.pasiv;
                                            server.Client.data.Stations[station].TS.set_state(name, ImpulseState.PassiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.pasiv:
                                        {
                                            selectButton.Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.Client.data.Stations[station].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
                                        }
                                        break;
                                    case StateControl.notconrol:
                                        {
                                            selectButton.Background = m_activecolor;
                                            impuls.State = StateControl.activ;
                                            server.Client.data.Stations[station].TS.set_state(name, ImpulseState.ActiveState, DateTime.Now);
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
                    server.Client.SendImpulse(name, lastSelectStation, ImpulseState.Execute);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.Close)
            {
                _hook.Stop();
                server.Stop();
                _timer_mig.Stop();
                server.Load.TestFile.StopTest(string.Empty);
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

    }
}
