using System;
using System.Text;

namespace Elements
{
    /// <summary>
    /// String utilities and extension methods. 
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Create a string A, B, C, ... AA, AB ... from an int value
        /// </summary>
        /// <param name="value">The value to turn into a character string</param>
        /// <returns>A string of Upper-case characters e.g. 1=A, 2=B, 27=AA</returns>
        public static string NumberToString(int value)
        {
            // Modified from https://forums.asp.net/t/1419722.aspx?generate+a+sequence+of+letters+in+C+
            StringBuilder sb = new StringBuilder();
            value++; // (so that 0 = A)
            do
            {
                value--;
                int remainder = 0;
                value = Math.DivRem(value, 26, out remainder);
                sb.Insert(0, Convert.ToChar('A' + remainder));

            } while (value > 0);

            return sb.ToString();
        }
    }
}
