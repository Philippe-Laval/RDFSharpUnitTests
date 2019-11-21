using RDFSharp.Model;
using RDFSharpTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDFSharpTests.ModelTests
{
    public static class GraphUnitTests
    {
        [Fact]
        public static void CreatingEmptyGraphTest()
        {
            RDFGraph waltdisney = new RDFGraph();
            Assert.True(waltdisney.TriplesCount == 0);
        }

        [Fact]
        public static void CreatingSimpleGraphTest()
        {
            RDFGraph waltdisney = GraphBuilder.WaltDisneyGraphBuild();
            var graphEnum = waltdisney.TriplesEnumerator;

            graphEnum.MoveNext();
            Assert.Equal("http://www.waltdisney.com/mickey_mouse", graphEnum.Current.Subject.ToString());
            Assert.Equal("http://xmlns.com/foaf/0.1/age", graphEnum.Current.Predicate.ToString());
            Assert.Equal("85^^http://www.w3.org/2001/XMLSchema#integer", graphEnum.Current.Object.ToString());

            graphEnum.MoveNext();
            Assert.Equal("http://www.waltdisney.com/donald_duck", graphEnum.Current.Subject.ToString());
            Assert.Equal("http://xmlns.com/foaf/0.1/name", graphEnum.Current.Predicate.ToString());
            Assert.Equal("Donald Duck", graphEnum.Current.Object.ToString());
        }
    }
}
