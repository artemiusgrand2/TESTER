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
        activ = 2,
        pasiv = 1,
        notconrol = 0
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
        private Dictionary<int, Impuls> _collectionImpulses = new Dictionary<int, Impuls>();
        /// <summary>
        /// коллекция управляющих импульсов
        /// </summary>
        public Dictionary<int, Impuls> CollectionImpulses
        {
            get
            {
                return _collectionImpulses;
            }
            set
            {
                _collectionImpulses = value;
            }
        }

        public Stations()
        {
            IsAllActive = string.Empty;
        }
    }
}
