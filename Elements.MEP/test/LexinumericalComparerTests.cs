using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Elements;

namespace Elements.MEP.Tests
{
    public class LexiNumericalComparerTests
    {
        [Fact]
        public void CompareStrings()
        {
            LexiNumericalComparer c = new LexiNumericalComparer();

            var a = "section 10";
            var b = "section 2";
            Assert.Equal(1, c.Compare(a, b));

            a = "a-5, b-99";
            b = "a-5, b-101";
            Assert.Equal(-1, c.Compare(a, b));

            a = "5bx1";
            b = "5bc4";
            Assert.Equal(1, c.Compare(a, b));

            a = "RS-1,RS-2";
            b = "RS-1,RS-2";
            Assert.Equal(0, c.Compare(a, b));
        }

        [Fact]
        public void OrderStrings()
        {
            var strings = new List<string>
            {
                "RS-10", "RA-5", "RS-2,RS-3", "RS-2,RS-4"
            };

            var ordered = strings.OrderBy(s => s, new LexiNumericalComparer()).ToList();
            Assert.Equal(strings[1], ordered[0]);
            Assert.Equal(strings[2], ordered[1]);
            Assert.Equal(strings[3], ordered[2]);
            Assert.Equal(strings[0], ordered[3]);
        }
    }
}
