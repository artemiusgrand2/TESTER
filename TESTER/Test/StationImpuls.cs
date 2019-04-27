using System;
using System.Collections.Generic;

namespace TESTER
{
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
