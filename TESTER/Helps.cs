using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace TESTER
{
    public class Helps
    {

        public static bool Find(string nameImpuls, string filter)
        {
            string[] filters = filter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string f in filters)
            {
                if (FindElement(nameImpuls, f))
                    return true;
            }
            //
            return false;
        }

        private static bool FindElement(string nameImpuls, string filter)
        {
            if (Regex.IsMatch(nameImpuls, filter))
                return true;
            //
            return false;
            //StringBuilder part_filter = new StringBuilder(string.Empty);
            //int currentChar = 0;
            //int countChar = 0;
            //bool currentStar = false;
            //bool isStar = false;
            ////
            //int index = -1;
            //for (int i = 0; i < filter.Length; i++)
            //{
            //    if (filter[i] != '*')
            //    {
            //        currentChar++;
            //        countChar++;
            //    }
            //    //
            //    switch (filter[i])
            //    {
            //        case '*':
            //            {
            //                currentStar = true;
            //                isStar = true;
            //                part_filter.Clear();
            //            }
            //            break;
            //        case '?':
            //            {
            //                part_filter.Clear();
            //                if ((!isStar && countChar != currentChar) || currentChar > nameImpuls.Length)
            //                    return false;
            //                if (((i == (filter.Length - 1)) && currentChar < nameImpuls.Length))
            //                    return false;
            //            }
            //            break;
            //        default:
            //            {
            //                index = -1;
            //                part_filter.Append(filter[i]);
            //                if ((index = nameImpuls.IndexOf(part_filter.ToString(0, part_filter.Length), currentChar - part_filter.Length)) != -1)
            //                {
            //                    if (part_filter.Length - 1 == i)
            //                        continue;
            //                    if ((currentChar - part_filter.Length) > index || (!currentStar && (index > currentChar - 1)) /*)*/ )
            //                        return false;
            //                    currentChar = index + part_filter.Length;
            //                    //if (i == 0 && index != 0)
            //                    //    return false;
            //                    //
            //                    if (!isStar && countChar != currentChar)
            //                        return false;
            //                    if (i == (filter.Length - 1))
            //                    {
            //                        if (currentChar < nameImpuls.Length)
            //                            return false;
            //                    }
            //                }
            //                else
            //                    return false;
            //                //
            //                currentStar = false;
            //            }
            //            break;
            //    }
            //}
            //
            //return true;
        }

    }
}
