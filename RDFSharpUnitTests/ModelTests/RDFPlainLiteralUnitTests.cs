using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDFSharp.Model;
using Xunit;

namespace RDFSharpTests.ModelTests
{
    public static class RDFPlainLiteralUnitTests
    {
        [Fact]
        public static void CreatingPlainLiteralWithValidValueTest()
        {
            //Egyszerű literális "normális" alapértékkel
            var plainLiteral = new RDFPlainLiteral("VIK");
            
            Assert.Equal("VIK", plainLiteral.Value);
            Assert.Equal("", plainLiteral.Language);
        }

        [Fact]
        public static void CreatingPlainLiteralWithoutValueTest()
        {
            //Egyszerű literális null alapértékkel
            var plainLiteral = new RDFPlainLiteral(null);
            Assert.Equal("", plainLiteral.Value);
            Assert.Equal("", plainLiteral.Language);
        }

        [Fact]
        public static void CreatingPlainLiteralWithoutLanguageTest()
        {
            //Egyszerű literális nyelv érték hozzáadás nélkül
            var plainLiteral = new RDFPlainLiteral("C'est la vie!");

            Assert.Equal("C'est la vie!", plainLiteral.Value);
            Assert.Equal("", plainLiteral.Language);
        }

        [Fact]
        public static void CreatingPlainLiteralWithLanguageTest()
        {
            //Egyszerű literális rendes nyelvi értékkel
            var plainLiteral = new RDFPlainLiteral("C'est la vie!", "fr");

            Assert.Equal("C'est la vie!", plainLiteral.Value);
            Assert.Equal("FR", plainLiteral.Language);
            Assert.Equal("C'est la vie!@FR", plainLiteral.ToString());

        }

        [Fact]
        public static void CreatingPlainLiteralWithWrongLanguageTest()
        {
            //Egyszerű literális rossz nyelvi értékkel
            var plainLiteral = new RDFPlainLiteral("C'est la vie!", "WhatThePepperoni");
            Assert.Equal("C'est la vie!", plainLiteral.Value);
            Assert.Equal("", plainLiteral.Language);
        }
    }
}
