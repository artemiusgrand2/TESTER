using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Timers;
using sdm.diagnostic_section_model.client_impulses;
using TESTER.Enums;
using TESTER.ServerListen;

namespace TESTER
{
    public delegate void NewState(bool state, string info);
    public delegate void NewNumberTest();
    public delegate void UpdateStateButton(int station, Impuls impulse);
    public delegate void NameCurrentTest(string info);
    public delegate void NotVisible(System.Windows.Visibility visible1, System.Windows.Visibility visible2);
    public delegate void CurrentSecondWait(string second);
    /// <summary>
    /// класс описания фала тестов
    /// </summary>
    public class FileScript
    {
        #region Переменные и свойства
        List<ScriptTest> _scripts = new List<ScriptTest>();
        /// <summary>
        /// сценарии тестирования станции
        /// </summary>
        public List<ScriptTest> Scripts { get { return _scripts; } }
        private int _number_current_test = -1;
        /// <summary>
        /// Номер текущего теста
        /// </summary>
        public int CurrentTest { get { return _number_current_test; } set { _number_current_test = value; } }
        /// <summary>
        /// запущено ли тестирование
        /// </summary>
        public bool IsStart { get; set; }
        /// <summary>
        /// подтверждение ввода
        /// </summary>
        public bool IsEnter { get; set; }
        /// <summary>
        /// выполняется ли комманда ТУ
        /// </summary>
        public bool IsCommandTU { get; set; }
        /// <summary>
        /// изменение состояния тестирования
        /// </summary>
        public event NewState NewState;
        /// <summary>
        /// таймер обработки сценариев
        /// </summary>
        Timer _timer_play;
        /// <summary>
        /// таймер ожидания подтверждения
        /// </summary>
        Timer _timer_wait;
        /// <summary>
        /// загруженный проект
        /// </summary>
        public ListenController Server {get;set;}
        /// <summary>
        /// переход на новый тест
        /// </summary>
        public  event NewNumberTest NewNumberState;
        /// <summary>
        /// изменяем цвет импульсов
        /// </summary>
        public event UpdateStateButton UpdateState;
        /// <summary>
        /// показываем текущий тест
        /// </summary>
        public event NameCurrentTest NameTest;
        /// <summary>
        /// убрать кнопку подтверждения
        /// </summary>
        public event NotVisible NotVisible;
        /// <summary>
        /// показывает текущуую секунду ожидания
        /// </summary>
        public event CurrentSecondWait CurrentSecondWait;
        /// <summary>
        /// текущая секунда ожидания
        /// </summary>
        private double _current_wait_second = -1;
        private List<CommandTU> _list_Tu = new List<CommandTU>();
        /// <summary>
        /// Спимок комманд ТУ в очереди
        /// </summary>
        public List<CommandTU> ListTU
        {
            get
            {
                return _list_Tu;
            }
            set
            {
                _list_Tu = value;
            }
        }
        #endregion

        public FileScript()
        {
            _timer_play = new Timer(100);
            _timer_play.Elapsed += timer_tick;
            //
            _timer_wait = new Timer(1000);
            _timer_wait.Elapsed += timer_wait;
        }

        public void LoadScript(string[] scriptrows)
        {
            CreateScripts(scriptrows);
            AnalisScript();
        }

        /// <summary>
        /// проверяем актуальность номера теста
        /// </summary>
        /// <returns></returns>
        private bool ActiveNumberTest(int number)
        {
            if (number >= 0 && number < _scripts.Count)
                return true;
            else return false;
        }

        private void timer_wait(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (CurrentSecondWait != null && _current_wait_second > 0)
                {
                    if(_current_wait_second >= 10)
                        CurrentSecondWait(_current_wait_second.ToString());
                    else CurrentSecondWait(string.Format("0{0}", _current_wait_second));
                }
                else _timer_wait.Stop();
                _current_wait_second--;
            }
            catch { }
        }

        private void timer_tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer_play.Stop();
                //если послана команда ТУ
                if (IsCommandTU)
                {
                    while ( _list_Tu.Count > 0)
                    {
                        int number = GetNumberCommandTU(_list_Tu[0].StationNumber, _list_Tu[0].NameTU);
                        if (number != -1)
                        {
                            if (!_list_Tu[0].isRun)
                            {
                                _number_current_test = number;
                                _list_Tu[0].isRun = true;
                                UpdateCurrentTest(_number_current_test);
                                if (NewNumberState != null)
                                    NewNumberState();
                            }
                            break;
                        }
                        else
                        {
                            _number_current_test = number;
                            _list_Tu.RemoveAt(0);
                        }
                    }
                }
                //
                if (Server != null && ActiveNumberTest(_number_current_test))
                {
                    //показываем текущий тест
                    if (NameTest != null)
                    {
                        if(!IsCommandTU)
                            NameTest(string.Format("{0} - {1} - {2}", _scripts[_number_current_test].NameStation, _scripts[_number_current_test].NameRecord, _scripts[_number_current_test].NameTest));
                        else NameTest(string.Format("Выполнение команды ТУ {0} по станции {1}", _scripts[_number_current_test].Command, _scripts[_number_current_test].NameStation));
                    }
                    //производим выполнение одного сценария со вложенным сценарием
                    PlayCurrentTest(_scripts[_number_current_test]);
                    //if (!IsCommandTU)
                    //{
                        _number_current_test++;
                        UpdateCurrentTest(_number_current_test);
                        if (NewNumberState != null)
                            NewNumberState();
                    //}
                    //else
                    //{
                        //if (_list_Tu.Count > 0)
                        //{
                        //    _list_Tu.RemoveAt(0);
                        //    if (_list_Tu.Count == 0)
                        //    {
                        //        IsCommandTU = false;
                        //       // StopTest("Все комманды ТУ выполнены !!!");
                        //    }
                        //}
                   // }
                    //
                    if (IsStart)
                        _timer_play.Start();
                }
                else
                    StopTest("Тест окончен !!!");
            }
            catch (Exception error) { StopTest(string.Format("{0}, произошла ошибка - {1}", "Тест окончен !!!", error.Message)); }
        }



        private void PlayCurrentTest(ScriptTest currenttest)
        {
            try
            {
                if (Server.ProjectTester.Station.ContainsKey(currenttest.NameStation))
                {
                    if (IsStart)
                    {
                        //если есть исполняемые команды
                        if (currenttest.Impulses.Count > 0)
                        {
                            foreach (Script script in currenttest.Impulses)
                            {
                                if (script is ImpulsLabel)
                                {
                                    ScriptTest script_label = (script as ImpulsLabel).LabelPlay;
                                    PlayCurrentTest(script_label);
                                }
                                else
                                {
                                    ImpulsGroup impulsgroup = script as ImpulsGroup;
                                    //активируем импульсы текущего сценария
                                    foreach (Impuls imp in impulsgroup.Impulses)
                                    {
                                        SetImpuls(currenttest.StationNumber, imp);
                                    }
                                }
                            }
                            TransitionNextTest(currenttest);
                        }
                        else
                            TransitionNextTest(currenttest);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Станции - {0} нет в списке станций участка", currenttest.NameStation));
                    return;
                }
            }
            catch (Exception error) { StopTest(string.Format("{0}, произошла ошибка - {1}", "Тест окончен !!!", error.Message)); }
        }

        private void TransitionNextTest(ScriptTest currenttest)
        {
            //если необходимо подтверждение
            if (currenttest.Confirmation)
            {
                if (!IsCommandTU)
                {
                    if (NotVisible != null)
                        NotVisible(System.Windows.Visibility.Visible, Visibility.Collapsed);
                    while (!IsEnter && IsStart) { System.Threading.Thread.Sleep(100); }
                    IsEnter = false;
                    return;
                }
                else
                {
                    if (_list_Tu.Count > 0)
                    {
                        _list_Tu.RemoveAt(0);
                        if (_list_Tu.Count == 0)
                        {
                            IsCommandTU = false;
                             StopTest("Все комманды ТУ выполнены !!!");
                        }
                    }
                }
            }
            else
            {
                _current_wait_second = currenttest.TimeWait;
                if (NotVisible != null)
                    NotVisible(System.Windows.Visibility.Collapsed, Visibility.Visible);
                _timer_wait.Start();
                while ((_current_wait_second > 0) && IsStart) { System.Threading.Thread.Sleep(100); }
                if (NotVisible != null)
                    NotVisible(System.Windows.Visibility.Collapsed, Visibility.Collapsed);
                _timer_wait.Stop();
                return;
            }
        }

        private void SetImpuls(int numberstation, Impuls imp)
        {
            if (Server.ProjectTester.CollectionStations.ContainsKey(numberstation))
            {
                foreach (var impuls in Server.ProjectTester.CollectionStations[numberstation].CollectionImpulses)
                {
                    if (imp.Name != ScriptTest.AllImp)
                    {
                        if (impuls.Name == imp.Name)
                        {
                            impuls.State = imp.State;
                            Server.SourceImpulsServer.data.Stations[numberstation].TS.set_state(imp.Name, GetState(imp.State), DateTime.Now);
                            if (UpdateState != null)
                                UpdateState(numberstation, impuls);
                            return;
                        }
                    }
                    else
                    {
                        impuls.State = imp.State;
                        Server.SourceImpulsServer.data.Stations[numberstation].TS.set_state(imp.Name, GetState(imp.State), DateTime.Now);
                        if (UpdateState != null)
                            UpdateState(numberstation, impuls);
                    }
                }
            }
        }

        private ImpulseState GetState(StateControl state)
        {
            switch (state)
            {
                case StateControl.activ:
                    return ImpulseState.ActiveState;
                case StateControl.pasiv:
                    return ImpulseState.PassiveState;
                default:
                    return ImpulseState.UncontrolledState;
            }
        }

        private void CreateScripts(string[] scriptrows)
        {
            try
            {
                _number_current_test = -1;
                _scripts.Clear();
                if (scriptrows != null)
                {
                    foreach (string row in scriptrows)
                    {
                        try
                        {
                           string [] cells = row.Split(new string[] { ";" }, StringSplitOptions.None);
                           if (cells.Length == 6 && row.IndexOf("#") == -1)
                           {
                               ScriptTest test = new ScriptTest(cells[0], cells[1], cells[2], cells[3], cells[4], cells[5], _scripts);
                               _scripts.Add(test);
                           }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
        }

        private void AnalisScript()
        {
            string _lastnamestation = string.Empty;
            string _lastnamerecord = string.Empty;
            foreach (ScriptTest script in _scripts)
            {
                if (script.Recordtype == ViewTypeRecord.section)
                {
                    if (!string.IsNullOrEmpty(script.NameStation))
                        _lastnamestation = script.NameStation;
                    if (!string.IsNullOrEmpty(script.NameTest))
                        _lastnamerecord = script.NameTest;
                }
                else
                {
                    if (!string.IsNullOrEmpty(script.NameStation))
                    {
                        if (Server.ProjectTester.Station.ContainsKey(script.NameStation))
                            script.StationNumber = Server.ProjectTester.Station[script.NameStation];
                        script.NameRecord = _lastnamerecord;
                        continue;
                    }
                }
                //
                if (Server.ProjectTester.Station.ContainsKey(_lastnamestation))
                    script.StationNumber = Server.ProjectTester.Station[_lastnamestation];
                script.NameStation = _lastnamestation;
                script.NameRecord = _lastnamerecord;
            }
        }

        public int GetNumberCommandTU(int StationNumber, string NameTU)
        {
            int index = 0;
            foreach (ScriptTest test in Scripts)
            {
                if ((test.StationNumber == StationNumber && StationNumber != 0) && test.Command == NameTU)
                    return index;
                index++;
            }
            //
            return -1;
        }

        public void StartTest(int number)
        {
            if (UpdateCurrentTest(number))
            {
                _number_current_test = number;
                if (!IsStart)
                {
                    try
                    {
                        IsStart = true;
                        IsCommandTU = false;
                        _timer_play.Start();
                        if (NewState != null)
                            NewState(IsStart, string.Format("Старт тест - станция {0} ({1} - {2})", _scripts[number].NameStation, _scripts[number].NameRecord, _scripts[number].NameTest));
                        //
                        _list_Tu.Clear();
                    }
                    catch (Exception error) {
                        MessageBox.Show(error.Message); 
                    }
                }
            }
        }

        public void StartTest()
        {
            if (!IsStart && MainWindow.TypeWork == TypeWork.autonomy)
            {
                try
                {
                    IsCommandTU = true;
                    IsStart = true;
                    _timer_play.Start();
                    if (NewState != null)
                        NewState(IsStart, string.Format("{0}", "Выполнение комманд ТУ"));
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private bool UpdateCurrentTest(int number)
        {
            foreach (ScriptTest test in _scripts)
                test.CurrentPlay = false;
            if (ActiveNumberTest(number))
            {
                _scripts[number].CurrentPlay = true;
                return true;
            }
            else return false;
        }

        public void StopTest(string info)
        {
            try
            {
                if (IsStart)
                {
                    IsStart = false;
                    IsCommandTU = false;
                    _timer_play.Stop();
                    if (NewState != null)
                        NewState(IsStart, info);
                    //
                    _list_Tu.Clear();
                }
            }
            catch { }
        }
    }
}
