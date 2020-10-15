using System;
using System.Collections.Generic;
using SCADA.Common.ImpulsClient;

namespace TESTER
{

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
        List<Impulse> _impulses = new List<Impulse>();
        /// <summary>
        /// Коллекция импульсов контроля при использован
        /// </summary>
        public List<Impulse> Impulses { get { return _impulses; } set { _impulses = value; } }
    }

    public interface Script { }
}
