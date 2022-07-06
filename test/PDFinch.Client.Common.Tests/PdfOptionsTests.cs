using System;
using NUnit.Framework;

namespace PDFinch.Client.Common.Tests
{
    internal class PdfOptionsTests
    {
        [Test]
        public void ToQueryString_UsesAllKnownProperties()
        {
            // Arrange
            var classUnderTest = new PdfOptions
            {
                GrayScale = true,
                Landscape = false,
                MarginBottom = 1,
                MarginLeft = 2,
                MarginRight = 3,
                MarginTop = 4,
            };
            
            // Act
            var queryString = classUnderTest.ToQueryString();

            // Assert
            const StringComparison ic = StringComparison.CurrentCultureIgnoreCase;
            
            Assert.IsTrue(queryString.Contains("GrayScale=true", ic));
            Assert.IsTrue(queryString.Contains("Landscape=false", ic));
            Assert.IsTrue(queryString.Contains("MarginBottom=1", ic));
            Assert.IsTrue(queryString.Contains("MarginLeft=2", ic));
            Assert.IsTrue(queryString.Contains("MarginRight=3", ic));
            Assert.IsTrue(queryString.Contains("MarginTop=4", ic));
        }
    }
}
