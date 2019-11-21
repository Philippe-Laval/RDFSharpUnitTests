using RDFSharp.Store;
using RDFSharpTests.Utils;
using System;
using System.Linq;
using Xunit;

namespace RDFSharpTests.StoreTests
{
    public static class FederationUnitTests
    {
        [Fact]
        public static void AddStoreToFederationTest()
        {
            RDFFederation fed = StoreBuilder.CreateFederation();
            Assert.Equal(1, fed.StoresCount);
        }

        [Fact]
        public static void ClearStoresFromFederationTest()
        {
            RDFFederation fed = StoreBuilder.CreateFederation();
            fed.ClearStores();
            Assert.Empty(fed);
        }

        [Fact]
        public static void RemoveStoreFromFederationTest()
        {
            RDFFederation fed = StoreBuilder.CreateFederation();
            RDFMemoryStore mem = StoreBuilder.CreateStore();

            fed.RemoveStore(mem);
            Assert.DoesNotContain(mem, fed);
        }

        [Fact]
        public static void FederationEqualityTest()
        {
            RDFFederation fed1 = StoreBuilder.CreateFederation();
            RDFFederation fed2 = StoreBuilder.CreateFederation();
            //The storeIDs are still not equals, so these to federtaions never will be the same
            Assert.False(fed1.Equals(fed2));
        }

        [Fact]
        public static void EmptyConstructorFederation()
        {
            RDFFederation fed = new RDFFederation();
            Assert.NotNull(fed);
        }

        [Fact]
        public static void AddingTwoExactStoresToFederationTest()
        {
            RDFFederation fed = new RDFFederation();
            fed.AddStore(StoreBuilder.CreateStore());
            fed.AddStore(StoreBuilder.CreateStore());
            Assert.NotEqual(fed.First().StoreID, fed.Last().StoreID);
        }
    }
}
