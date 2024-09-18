using System;
using Maheke.Gov.Domain;
using Xunit;

namespace Maheke.Gov.Test.Domain
{
    public class OptionTest
    {
        [Fact]
        public void TestCreateOption()
        {
            var option = new Option("FOR");

            Assert.Equal("FOR", option.Name);
            Assert.Throws<ArgumentNullException>(() => new Option(""));
            Assert.Throws<ArgumentNullException>(() => new Option("      "));
            Assert.Throws<ArgumentNullException>(() => new Option(null));
        }
    }
}
