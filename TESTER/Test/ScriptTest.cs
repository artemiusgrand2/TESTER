using System;
using System.Collections.Generic;
using System.Windows;
using TESTER.Enums;

namespace TESTER
{
    /// <summary>
    /// тир записи
    /// </summary>
    struct RecordType
    {
        public const string execute = "и";
        public const string section = "р";
    }

    public enum ViewTypeRecord
    {
        none = 0,
        execute = 1,
        section = 2
    }

    /// <summary>
    /// вид контроля
    /// </summary>
    struct ViewControl
    {
        public const string label = "s";
        public const string command = "u";
        public const string notcontrol = "?";
        public const string pasiv = "0";
        public const string activ = "1";
    }

    /// <summary>
    /// класс описания сценария тестирования
    /// </summary>
    public  class ScriptTest
    {
        #region Переменные и свойства
        /// <summary>
        /// выполняется ли строчка в текущий момент
        /// </summary>
        public bool CurrentPlay { get; set; }
        /// <summary>
        /// метка удаления
        /// </summary>
        public static string LabelDel = "*";
        /// <summary>
        /// метка контроля все импульсов
        /// </summary>
        public static string AllImp = "*";
        ViewTypeRecord _recordtype = ViewTypeRecord.none;
        /// <summary>
        /// Тип записи
        /// </summary>
        public ViewTypeRecord Recordtype { get { return _recordtype; } }
        string _label = string.Empty;
        /// <summary>
        /// Метка строки
        /// </summary>
        public string LabelStr { get { return _label; } }
        string _name_record = string.Empty;
        /// <summary>
        /// Название раздела
        /// </summary>
        public string NameRecord { get { return _name_record; } set { _name_record = value; } }
        /// <summary>
        /// номер станции
        /// </summary>
        public int StationNumber { get; set; }
        string _namestation = string.Empty;
        /// <summary>
        /// Название станции
        /// </summary>
        public string NameStation { get { return _namestation; } set { _namestation = value; } }
        List<Script> _impulses = new List<Script>();
        /// <summary>
        /// перечень импульсов контроля
        /// </summary>
        public List<Script> Impulses { get { return _impulses; } }
        string _command = string.Empty;
        /// <summary>
        /// Название комманды ТУ
        /// </summary>
        public string Command { get { return _command; } set { _command = value; } }
        string _description = string.Empty;
        /// <summary>
        /// Описание
        /// </summary>
        public string NameTest { get { return _description; } }
        /// <summary>
        /// сценария работает с подтреждением или нет
        /// </summary>
        public bool Confirmation { get; set; }
        /// <summary>
        ///время ожидания
        /// </summary>
        public int TimeWait { get; set; }
        #endregion

        public ScriptTest(string recordtype, string label, string namestation, string dann, string description, string confirmation, List<ScriptTest> scripts)
        {
            Analis(recordtype, label, namestation, dann, description, confirmation, scripts);
        }

        /// <summary>
        /// анализируем полученные данные
        /// </summary>
        private void Analis(string recordtype, string label, string namestation, string dann, string description, string confirmation, List<ScriptTest> scripts)
        {
            try
            {
                //анализируем тип записи
                switch (recordtype.Trim())
                {
                    case RecordType.section:
                        _recordtype = ViewTypeRecord.section;
                        break;
                    case RecordType.execute:
                        _recordtype = ViewTypeRecord.execute;
                        break;
                    default :
                        return;
                }
                //анализируем метку строки
                _label = label.Trim();
                //анализируем название станции
                _namestation = namestation.Trim();
                //если метка исполнительная
                if (_recordtype == ViewTypeRecord.execute)
                {
                    //анализируем описание импульсов контроля
                    dann = dann.Trim();
                    string[] imps = dann.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string imp in imps)
                    {
                        string[] values = imp.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Length == 2)
                        {
                            string metka = values[0].Trim();
                            if (metka == ViewControl.label || metka == ViewControl.command)
                            {
                                if (metka == ViewControl.label)
                                {
                                    ScriptTest labelscript = FindLabelInfo(values[1].Trim(), scripts);
                                    if (labelscript != null)
                                        Impulses.Add(new ImpulsLabel() { LabelPlay = labelscript });
                                }
                                else
                                    _command = values[1].Trim();
                            }
                            else
                            {
                                if (metka == ViewControl.activ || metka == ViewControl.pasiv || metka == ViewControl.notcontrol)
                                {
                                    if (Impulses.Count > 0)
                                    {
                                        if ((Impulses[Impulses.Count - 1] is ImpulsGroup))
                                        {
                                            (Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Add(new Impuls(values[1].Trim(), TypeImpuls.ts) { State = GetState(metka) });
                                        }
                                        else
                                        {
                                            Impulses.Add(new ImpulsGroup());
                                            (Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Add(new Impuls(values[1].Trim(), TypeImpuls.ts) { State = GetState(metka) });
                                        }
                                    }
                                    else
                                    {
                                        Impulses.Add(new ImpulsGroup());
                                        (Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Add(new Impuls(values[1].Trim(), TypeImpuls.ts) {State = GetState(metka) });
                                    }
                                }
                            }
                        }
                        else if (values.Length == 1)
                        {
                            if (Impulses.Count > 0)
                            {
                                if (Impulses[Impulses.Count - 1] is ImpulsLabel)
                                {
                                    ScriptTest labelscript = FindLabelInfo(values[0].Trim(), scripts);
                                    if (labelscript != null)
                                        Impulses.Add(new ImpulsLabel() { LabelPlay = labelscript });
                                }
                                else if (Impulses[Impulses.Count - 1] is ImpulsGroup)
                                {
                                    if ((Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Count > 0)
                                    {
                                        StateControl state = (Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses[(Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Count - 1].State;
                                        (Impulses[Impulses.Count - 1] as ImpulsGroup).Impulses.Add(new Impuls(values[0].Trim(), TypeImpuls.ts) {State = state });
                                    }
                                }
                            }
                        }
                    }
                }
                //анализируем описание теста
                _description = description.Trim();
                //анализируем подтверждение
                confirmation = confirmation.Trim();
                int buffer = 0;
                if (int.TryParse(confirmation, out buffer))
                    TimeWait = int.Parse(confirmation);
                else Confirmation = true;
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
        }

        private ScriptTest FindLabelInfo(string label, List<ScriptTest> scripts)
        {
            int index_remove = 0;
            int index_find = -1;
            for (int i = 0; i < scripts.Count; i++)
            {
                if (scripts[i].LabelStr == label)
                    index_find = i;
                //
                if (scripts[i].LabelStr == LabelDel)
                    index_remove = i;
            }
            //
            if (index_find == -1)
                throw new Exception(String.Format("Ошибка метка {0} - не объявлена", label));
            else
            {
                if (index_find <= index_remove)
                    throw new Exception(String.Format("Ошибка метка {0} - объявлена в строке под номером {1}, раньше метки обнуления в строке {2}", label, (index_find+1), (index_remove+1)));
                else 
                {
                    return scripts[index_find];
                }
            }
        }

        private StateControl GetState(string impuls_value)
        {
            if (impuls_value != null)
            {
                switch (impuls_value)
                {
                    case ViewControl.activ:
                        return StateControl.activ;
                    case ViewControl.pasiv:
                        return StateControl.pasiv;
                    case ViewControl.notcontrol:
                        return StateControl.notconrol;
                    default:
                        return StateControl.notconrol;
                }
            }
            else
            {
                return StateControl.notconrol;
            }
        }
    }
}
