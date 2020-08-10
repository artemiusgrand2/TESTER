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

        int countImpulsTs;

        int countImpulsTu;

        int countReceiveTs;

        int notcontrolcountImpulsTs;

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

        public int CountImpulsTs
        {
            get
            {
                return countImpulsTs;
            }

            set
            {
                if (value != countImpulsTs)
                {
                    countImpulsTs = value;
                    OnPropertyChanged("CountImpulsTs");
                }
            }
        }

        public int CountImpulsTu
        {
            get
            {
                return countImpulsTu;
            }

            set
            {
                if (value != countImpulsTu)
                {
                    countImpulsTu = value;
                    OnPropertyChanged("CountImpulsTu");
                }
            }
        }

        public int CountReceiveTs
        {
            get
            {
                return countReceiveTs;
            }

            set
            {
                if (value != countReceiveTs)
                {
                    countReceiveTs = value;
                    OnPropertyChanged("CountReceiveTs");
                }
            }
        }

        public int NotcontrolCountImpuls
        {
            get
            {
                return notcontrolcountImpulsTs;
            }

            set
            {
                if (value != notcontrolcountImpulsTs)
                {
                    notcontrolcountImpulsTs = value;
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
