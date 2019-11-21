using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Linq;
using Xunit;

namespace RDFSharpTests.StoreTests
{
    public static class QuadrupleUnitTests
    {
        [Fact]
        public static void CreatingSimpleQuadrupleTest()
        {
            var cont = new RDFContext("http://www.wikipedia.com/");
            var subj = new RDFResource("http://www.waltdisney.com/mickey_mouse");
            var pred = new RDFResource("http://xmlns.com/foaf/0.1/age");
            var lit = new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT);
            var quad = new RDFQuadruple(cont, subj, pred, lit);

            Assert.NotNull(quad); 
        }

        [Fact]
        public static void GetQuadrupleContextTest()
        {
            RDFQuadruple quad = new RDFQuadruple(
                new RDFContext("http://www.wikipedia.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.Equal("http://www.wikipedia.com/", quad.Context.ToString());
        }

        [Fact]
        public static void GetQuadrupleSubjectTest()
        {
            RDFQuadruple quad = new RDFQuadruple(
                new RDFContext("http://www.wikipedia.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.Equal("http://www.waltdisney.com/mickey_mouse", quad.Subject.ToString());
        }

        [Fact]
        public static void GetQuadruplePredicateTest()
        {
            RDFQuadruple quad = new RDFQuadruple(
                new RDFContext("http://www.wikipedia.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.Equal("http://xmlns.com/foaf/0.1/age", quad.Predicate.ToString());
        }

        [Fact]
        public static void GetQuadrupleLiteralTest()
        {
            RDFQuadruple quad = new RDFQuadruple(
                new RDFContext("http://www.wikipedia.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.Equal("85^^http://www.w3.org/2001/XMLSchema#int", quad.Object.ToString());
        }

    }
}
