using System;
using System.Collections.Generic;
using System.Linq;

using TESTER.Enums;

namespace TESTER
{
    class ServerImpuls
    {
        Dictionary<int, Stations> CollectionStations = new Dictionary<int, Stations>();

        public  ServerImpuls(Dictionary<int, Stations> collectionctations)
        {
            CollectionStations = collectionctations;
        }

        private void InsertMassiv(List<byte> sourse, byte[] info)
        {
            foreach (byte bit in info)
                sourse.Add(bit);
        }

        private byte[] BitStafing(byte[] massiv, ref int addbit)
        {
            List<byte> answer = new List<byte>();
            foreach (byte bit in massiv)
            {
                if (bit == 125)
                {
                    answer.Add(bit);
                    answer.Add(93);
                    addbit++;
                    continue;
                }
                //
                if (bit == 126)
                {
                    answer.Add(125);
                    answer.Add(94);
                    addbit++;
                    continue;
                }

                answer.Add(bit);
            }
            return answer.ToArray();
        }

        int Lenght_massiv(int Count_impuls, int lenghtstationame, int kountimpuls)
        {
            if (Count_impuls % 4 != 0)
            {
                return Count_impuls / 4 + 8 + lenghtstationame + kountimpuls;
            }
            else
            {
                return Count_impuls / 4 + 7 + lenghtstationame + kountimpuls;
            }
        }

        byte[] Formirovanie_massiv_byte(int numberstation, int Count_impuls, ref int index)
        {
            try
            {
                int addbit = 0;
                //Код станции шестизначный
                byte[] Index_Station = BitStafing(BitConverter.GetBytes((Int32)numberstation), ref addbit);
                //Количество импульсов телесигнализации
                byte[] Kol_Impuls = BitStafing(BitConverter.GetBytes((Int16)Count_impuls), ref addbit);
                int Count_massiv = Lenght_massiv(Count_impuls, Index_Station.Length, Kol_Impuls.Length);
                List<byte> massiv_byte = new List<byte>();
                //Число задействованных байт
                byte[] Count_byte = BitStafing(BitConverter.GetBytes((Int16)(Count_massiv - 1 - addbit)), ref addbit);
                //Заполнение массива данными справочной информацией
                massiv_byte.Add(126);
                massiv_byte.Add(0);
                massiv_byte.Add(1);
                massiv_byte.Add(0);
                InsertMassiv(massiv_byte, Count_byte);
                InsertMassiv(massiv_byte, Index_Station);
                InsertMassiv(massiv_byte, Kol_Impuls);
                massiv_byte.Add(0);
                massiv_byte.Add(0); index = massiv_byte.Count - 1;
                //количество байт занимаемых информацией
                int count = (Count_impuls % 4 != 0) ? (Count_impuls / 4 + 1) : (Count_impuls / 4);
                //
                for (int i = 1; i <= count; i++)
                {
                    if (i != (count))
                        massiv_byte.Add(170);
                    else if (i == (count))
                    {
                        switch (Count_impuls % 4)
                        {
                            case 0:
                                {
                                    massiv_byte.Add(170);
                                    break;
                                }
                            case 1:
                                {
                                    massiv_byte.Add(2);
                                    break;
                                }
                            case 2:
                                {
                                    massiv_byte.Add(10);
                                    break;
                                }
                            case 3:
                                {
                                    massiv_byte.Add(42);
                                    break;
                                }
                        }
                    }
                }
                massiv_byte.Add(126);
                //	
                return massiv_byte.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        int Perevod_Binary(int[] massiv_binary)
        {
            double answer = 0;
            for (int i = 0; i < massiv_binary.Length; i++)
            {
                answer += (massiv_binary[i] * Math.Pow(2, ((massiv_binary.Length - 1) - i)));
            }
            return (int)answer;
        }

        int[] Perevod_Desatin(int Byte)
        {
            int[] massiv_binary = new int[8];
            int index = 7;
            while (Byte != 0)
            {
                massiv_binary[index] = Byte % 2;
                Byte /= 2;
                index--;
            }
            return massiv_binary;
        }

        public  byte[] OutPutData(IList<int> stationsNumner)
        {
            //
            var massiv_byte = new List<byte>();
            foreach (int number in stationsNumner)
            {
                try
                {
                    int indexOff = 0;
                    int numberImpuls = 1;
                    //Формирование ответа по умолчанию когда все импульсы пасcивны
                    if (CollectionStations.ContainsKey(number))
                    {
                        byte[] massiv = Formirovanie_massiv_byte(number, CollectionStations[number].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts).ToList().Count, ref indexOff);
                        foreach (var impuls in CollectionStations[number].CollectionImpulses.Where(x => x.Type == TypeImpuls.ts))
                        {
                            int number_byte = numberImpuls / 4;
                            int bit = numberImpuls % 4;
                            if (bit != 0)
                                number_byte++;
                            //
                            byte answer_byte = (byte)Perevod_Binary(Preobrasovanie_massiva_binary(Perevod_Desatin((byte)massiv[indexOff + number_byte]), bit, impuls.State));
                            massiv[indexOff + number_byte] = answer_byte;
                            //
                            numberImpuls++;
                        }
                        AddStatioByte(ref massiv_byte, massiv);
                    }
                }
                catch { }
            }
            return massiv_byte.ToArray();
        }

        int[] Preobrasovanie_massiva_binary(int[] massiv, int bit, StateControl value_impuls)
        {
            int value = 1;
            switch (value_impuls)
            {
                case StateControl.activ:
                    value = 11;
                    break;
                case StateControl.pasiv:
                    value = 10;
                    break;
                case StateControl.notconrol:
                    value = 1;
                    break;
            }
            //
            switch (bit)
            {
                case 0:
                    {
                        switch (value)
                        {
                            case 0:
                                {
                                    massiv[0] = 0; massiv[1] = 0;
                                    break;
                                }
                            case 1:
                                {
                                    massiv[0] = 0; massiv[1] = 1;
                                    break;
                                }
                            case 10:
                                {
                                    massiv[0] = 1; massiv[1] = 0;
                                    break;
                                }
                            case 11:
                                {
                                    massiv[0] = 1; massiv[1] = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 1:
                    {
                        switch (value)
                        {
                            case 0:
                                {
                                    massiv[6] = 0; massiv[7] = 0;
                                    break;
                                }
                            case 1:
                                {
                                    massiv[6] = 0; massiv[7] = 1;
                                    break;
                                }
                            case 10:
                                {
                                    massiv[6] = 1; massiv[7] = 0;
                                    break;
                                }
                            case 11:
                                {
                                    massiv[6] = 1; massiv[7] = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 2:
                    {
                        switch (value)
                        {
                            case 0:
                                {
                                    massiv[4] = 0; massiv[5] = 0;
                                    break;
                                }
                            case 1:
                                {
                                    massiv[4] = 0; massiv[5] = 1;
                                    break;
                                }
                            case 10:
                                {
                                    massiv[4] = 1; massiv[5] = 0;
                                    break;
                                }
                            case 11:
                                {
                                    massiv[4] = 1; massiv[5] = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 3:
                    {
                        switch (value)
                        {
                            case 0:
                                {
                                    massiv[2] = 0; massiv[3] = 0;
                                    break;
                                }
                            case 1:
                                {
                                    massiv[2] = 0; massiv[3] = 1;
                                    break;
                                }
                            case 10:
                                {
                                    massiv[2] = 1; massiv[3] = 0;
                                    break;
                                }
                            case 11:
                                {
                                    massiv[2] = 1; massiv[3] = 1;
                                    break;
                                }
                        }
                        break;
                    }
            }
            return massiv;
        }

        void AddStatioByte(ref List<byte> massivall, byte[] massivst)
        {
            foreach (byte b in massivst)
            {
                massivall.Add(b);
            }
        }
    }
}
