using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace TESTER
{
    struct NameState
    {
        public const string activ = "Все активны";
        public const string pasiv = "Все пассивны";
        public const string notcontrol = "Все нет контроля";
        public const string sever = "Данные с сервера";
    }

    public enum StateControl
    {
        notconrol = 0,
        activ = 1,
        pasiv = 2,
    }

    class Stations
    {
        /// <summary>
        /// Название станции
        /// </summary>
        public string NameStation { get; set; }
        /// <summary>
        /// все импульсы активны
        /// </summary>
        public string IsAllActive { get; set; }
        private IList<Impuls> collectionImpulses = new List<Impuls>();
        /// <summary>
        /// коллекция импульсов ТС/ТУ
        /// </summary>
        public IList<Impuls> CollectionImpulses
        {
            get
            {
                return collectionImpulses;
            }
            set
            {
                collectionImpulses = value;
            }
        }

        public Stations()
        {
            IsAllActive = string.Empty;
        }
    }
}
