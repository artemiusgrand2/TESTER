using System;
using System.Collections.Generic;

namespace TESTER
{
    public class Stations
    {
        /// <summary>
        /// Название станции
        /// </summary>
        public string NameStation { get; set; }

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

    }
}
