using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TESTER.Enums;

namespace TESTER.Models
{
    public class DataStationView
    {
        public  WrapPanel Panel { get; private set; }

        public int CurrentStation { get; set; } = -1;

        public bool IsShow { get; set; }

        /// <summary>
        /// коллекция импульсов станции
        /// </summary>
        public Dictionary<string, IDictionary<TypeImpuls, IList<Button>>> Collectionbuttons { get; } = new Dictionary<string, IDictionary<TypeImpuls, IList<Button>>>();

        public DataStationView(WrapPanel panel)
        {
            Panel = panel;
        }


        public bool IsFindButton(Button button)
        {
            var name = button.Content.ToString();
            if (Collectionbuttons.ContainsKey(name))
            {
                if (Collectionbuttons[name].ContainsKey(TypeImpuls.ts))
                {
                    if (Collectionbuttons[name][TypeImpuls.ts].Contains(button))
                        return true;
                }
            }
            //
            return false;
        }


        public void SetVisiblity(Impuls impuls, Visibility visibility)
        {
            if (Collectionbuttons.ContainsKey(impuls.Name))
            {
                if (Collectionbuttons[impuls.Name].ContainsKey(impuls.Type))
                {
                    foreach (var button in Collectionbuttons[impuls.Name][impuls.Type])
                    {
                        button.Visibility = visibility;
                    }
                }
            }
        }

    }
}
