using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Twenty.Utils
{
    public static class CalcUtils
    {
        private static readonly int sr_charA = Convert.ToInt32('a');
 
        private static readonly Dictionary<int, string> sr_units = new()
        {
            {0, ""},
            {1, "K"},
            {2, "M"},
            {3, "B"},
            {4, "T"}
        };
 
        public static string FormatNumber(int value)
        {
            if (value < 1000)
                return value.ToString();

            var n = (int)Mathf.Log(value, 1000);
            var m = value / Mathf.Pow(1000, n);
            var unit = "";
 
            if (n < sr_units.Count)
            {
                unit = sr_units[n];
            }
            else
            {
                var unitInt = n - sr_units.Count;
                var secondUnit = unitInt % 26;
                var firstUnit = unitInt / 26;
                unit = Convert.ToChar(firstUnit + sr_charA).ToString().ToUpper() + Convert.ToChar(secondUnit + sr_charA).ToString().ToUpper();
            }
            
            return (Mathf.Floor(m * 100) / 100).ToString("F1", CultureInfo.CurrentCulture) + unit;
        }
    }
}