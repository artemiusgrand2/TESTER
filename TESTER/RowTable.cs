using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TESTER
{
    public class RowTable : INotifyPropertyChanged
    {

        #region Переменные

        int station;

        string name;

        int countImpuls;

        int notcontrolcountImpuls;

        #endregion


        public int Station
        {
            get
            {
                return station;
            }

            set
            {
                if (value != station)
                {
                    station = value;
                    OnPropertyChanged("Station");
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }


        public int CountImpuls
        {
            get
            {
                return countImpuls;
            }

            set
            {
                if (value != countImpuls)
                {
                    countImpuls = value;
                    OnPropertyChanged("CountImpuls");
                }
            }
        }

        public int NotcontrolCountImpuls
        {
            get
            {
                return notcontrolcountImpuls;
            }

            set
            {
                if (value != notcontrolcountImpuls)
                {
                    notcontrolcountImpuls = value;
                    OnPropertyChanged("NotcontrolCountImpuls");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
