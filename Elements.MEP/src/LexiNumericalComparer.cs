using System;
using System.Collections.Generic;
using System.Text;

namespace Elements
{
    /// <summary>
    /// String lexicographical comparer that additionally compares integer numbers arithmetically.
    /// </summary>
    public class LexiNumericalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int i = 0;
            int j = 0;
            for (; i < x.Length && j < y.Length; i++, j++)
            {
                if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
                {
                    int xStep = 1;
                    int yStep = 1;
                    while (i + xStep < x.Length && char.IsDigit(x[i + xStep]))
                    {
                        xStep++;
                    }
                    while (j + yStep < y.Length && char.IsDigit(y[j + yStep]))
                    {
                        yStep++;
                    }

                    int xNumber = int.Parse(x.Substring(i, xStep));
                    int yNumber = int.Parse(y.Substring(j, yStep));
                    var c = xNumber.CompareTo(yNumber);
                    if (c != 0)
                    {
                        return c;
                    }
                    else
                    {
                        i += xStep;
                        j += yStep;
                    }
                }
                else
                {
                    var c = x[i].CompareTo(y[j]);
                    if (c != 0)
                    {
                        return c > 0 ? 1 : -1;
                    }
                }
            }

            if (i >= x.Length)
            {
                if (j >= y.Length)
                {
                    return 0;
                }
                return -1;
            }
            return 1;
        }
    }
}
