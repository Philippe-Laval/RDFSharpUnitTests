using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RDFSharp.Store;
using RDFSharp.Model;
using RDFSharpTests.Utils;

namespace RDFSharpTests.StoreTests
{
    public static class MemoryStoreUnitTests
    {

        [Fact]
        public static void CreateSimpleStoreTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();
            Assert.Equal(3, rdfms.QuadruplesCount);
        }

        [Fact]
        public static void ContainsQuadrupleTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();

            RDFQuadruple contain = new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.True(rdfms.ContainsQuadruple(contain));
        }
        [Fact]
        public static void RemovingQuadrupleTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();

            RDFQuadruple contain = new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));
            rdfms.RemoveQuadruple(contain);

            Assert.False(rdfms.ContainsQuadruple(contain));
        }

        [Fact]
        public static void StoreDifferenceTest()
        {
            var mem1 = StoreBuilder.CreateStore();
            var mem2 = StoreBuilder.CreateStore();

            var result = mem1.DifferenceWith(mem2);

            Assert.Empty(result);
        }
        [Fact]
        public static void StoreUnitTest()
        {
            var mem1 = StoreBuilder.CreateStore();
            var mem2 = StoreBuilder.CreateStore();

            var result = mem1.UnionWith(mem2);

            Assert.True(result.Equals(mem1));
        }

        [Fact]
        public static void ClearingStoreTest()
        {
            var store = StoreBuilder.CreateStore();

            store.ClearQuadruples();

            Assert.Empty(store);
        }

        [Fact]
        public static void MergingGraphIntoStoreTest()
        {
            var mem = new RDFMemoryStore();
            var graph = GraphBuilder.WaltDisneyGraphBuild();

            mem.MergeGraph(graph);

            /*
             * A memorystore és a gráf első elemének vizsgálatával
             * megállapítjuk, hogy sikeres volt-e a merge, mivel üres memorystore-ba mergeltünk
            */
            Assert.True(mem.First().Predicate.Equals(graph.First().Predicate));
        }

        [Fact]
        public static void MergingGraphIntoStoreTest_AvoidsDuplicateInsertions()
        {
            var store1 = new RDFMemoryStore();
            var graph1 = GraphBuilder.WaltDisneyGraphBuild();
            var graph2 = GraphBuilder.WaltDisneyGraphBuild();

            var store2 = new RDFMemoryStore();

            store1.MergeGraph(graph1);
            store1.MergeGraph(graph2);

            store2.MergeGraph(graph1);

            var result = store1.DifferenceWith(store2);

            Assert.Empty(result);
        }

        [Fact]
        public static void RemovingQuadrupleByContextTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();

            RDFQuadruple contain = new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            rdfms.RemoveQuadruplesByContext(new RDFContext("http://www.waltdisney.com/"));

            Assert.False(rdfms.ContainsQuadruple(contain));
        }

        [Fact]
        public static void RemovingQuadrupleBySubjectTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();
            RDFQuadruple contain = (new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT))
            );

            rdfms.RemoveQuadruplesBySubject(new RDFResource("http://www.waltdisney.com/mickey_mouse"));

            Assert.False(rdfms.ContainsQuadruple(contain));
        }

        [Fact]
        public static void RemovingQuadrupleByPredicateTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();
            RDFQuadruple contain = (new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT))
            );

            rdfms.RemoveQuadruplesByPredicate(new RDFResource("http://xmlns.com/foaf/0.1/age"));

            Assert.False(rdfms.ContainsQuadruple(contain));
        }

        [Fact]
        public static void RemovingQuadrupleByLiteralTest()
        {
            RDFMemoryStore rdfms = StoreBuilder.CreateStore();

            RDFQuadruple contain = new RDFQuadruple(
                new RDFContext("http://www.waltdisney.com/"),
                new RDFResource("http://www.waltdisney.com/mickey_mouse"),
                new RDFResource("http://xmlns.com/foaf/0.1/age"),
                new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            rdfms.RemoveQuadruplesByLiteral(new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INT));

            Assert.False(rdfms.ContainsQuadruple(contain));
        }

        
    }
}
