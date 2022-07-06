using System;
using NUnit.Framework;

namespace PDFinch.Client.Common.Tests
{
    public class PdfClientOptionsListTests
    {
        [Test]
        public void Constructor_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ = new PdfClientOptionsList(Array.Empty<PdfClientOptions>()));
        }
    }
}
