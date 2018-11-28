using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TESTER
{
    /// <summary>
    /// описание контроля импульса одиночного
    /// </summary>
   public  class Impuls 
    {
        /// <summary>
        /// Название импульса
        /// </summary>
        public string Name_Impuls { get; set; }
        /// <summary>
        /// Значение импульса
        /// </summary>
        public StateControl Value_Impuls { get; set; }

        public Impuls()
        {
            Name_Impuls = string.Empty;
        }
    }

    /// <summary>
    /// описание контроля импульса группового (при использовании метки)
    /// </summary>
    class ImpulsLabel : Script
    {
        /// <summary>
        /// ссылка на сценарий метки
        /// </summary>
        public ScriptTest LabelPlay { get; set; }
    }

    /// <summary>
    /// описание контроля импульса группового (без использования меток)
    /// </summary>
    class ImpulsGroup : Script
    {
        List<Impuls> _impulses = new List<Impuls>();
        /// <summary>
        /// Коллекция импульсов контроля при использован
        /// </summary>
        public List<Impuls> Impulses { get { return _impulses; } set { _impulses = value; } }
    }

    public interface Script { }
}
