using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SCADA.Common.Enums;
using SCADA.Common.ImpulsClient;

namespace TESTER.Models
{
    public class DataStationView
    {
        public  WrapPanel Panel { get; private set; }

        public int CurrentStation { get; set; } = -1;

        public StatesControl CommonState { get; set; }

        public bool IsShow { get; set; }

        /// <summary>
        /// коллекция импульсов станции
        /// </summary>
        public Dictionary<string, IDictionary<TypeImpuls, IList<Button>>> Collectionbuttons { get; } = new Dictionary<string, IDictionary<TypeImpuls, IList<Button>>>();

        public DataStationView(WrapPanel panel)
        {
            Panel = panel;
        }


        public bool IsFindButton(string nameImp, Button button)
        {
            if (Collectionbuttons.ContainsKey(nameImp))
            {
                if (Collectionbuttons[nameImp].ContainsKey(TypeImpuls.ts))
                {
                    if (Collectionbuttons[nameImp][TypeImpuls.ts].Contains(button))
                        return true;
                }
            }
            //
            return false;
        }


        public void SetVisiblity(Impulse impuls, Visibility visibility)
        {
            if (Collectionbuttons.ContainsKey(impuls.Name))
            {
                if (Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                {
                    foreach (var button in Collectionbuttons[impuls.Name][impuls.Type])
                    {
                        if (button.Visibility != visibility)
                            button.Visibility = visibility;
                    }
                }
            }
        }

    }
}
