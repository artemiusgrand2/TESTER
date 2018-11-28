using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TESTER
{
    class CommandTU
    {
        /// <summary>
        /// Номер станции
        /// </summary>
        public int StationNumber { get; set; }
        /// <summary>
        /// Название команды ТУ
        /// </summary>
        public string NameTU { get; set; }
        /// <summary>
        /// Выполняется ли сейчас команда
        /// </summary>
        public bool isRun { get; set; }
    }
}
