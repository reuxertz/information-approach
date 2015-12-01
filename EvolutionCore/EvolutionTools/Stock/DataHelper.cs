using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoStockTools
{
    public static class DataHelper
    {
        public static string[][] RearrangeColumns(string[][] data, int[] columnIndexByOrder)
        {
            string[][] newData = new string[data.Length][];

            for (int i = 0; i < newData.Length; i++)
            {
                var curRow = data[i].ToArray<string>();
                var curNewRow = new string[columnIndexByOrder.Length];

                for (int j = 0; j < columnIndexByOrder.Length; j++)
                    curNewRow[j] = curRow[columnIndexByOrder[j]];

                newData[i] = curNewRow;
            }

            return newData;
        }

        public static void SplitDataByRowIndex(string[][] data, int rowIndex, out string[][] data1, out string[][] data2)
        {
            data1 = new string[rowIndex][];
            data2 = new string[data.Length - rowIndex][];

            for (int i = 0; i < data1.Length; i++)
                data1[i] = data[i].ToArray<string>();

            for (int i = 0; i < data2.Length; i++)
                data2[i] = data[i + rowIndex].ToArray<string>();
        }

        public static double[][] ConvertStringDataToDouble(string[][] data)
        {
            double[][] newData = new double[data.Length][];

            for (int i = 0; i < newData.Length; i++)
            {
                newData[i] = new double[data[i].Length];

                for (int j = 0; j < newData[i].Length; j++)
                {
                    string s = data[i][j];

                    if (s == "-")
                        newData[i][j] = -1.0;
                    else
                        newData[i][j] = Convert.ToDouble(s);
                }
            }

            return newData;
        }
    }
}
