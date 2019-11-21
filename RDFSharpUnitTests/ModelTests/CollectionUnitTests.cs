using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RDFSharpTests.ModelTests
{
    public static class CollectionUnitTests
    {
        #region AddTests
        [Fact]
        public static void AddNullTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            RDFResource resource = null;
            //AddItem method ignores the null items
            collection.AddItem(resource);
            Assert.Equal(0, collection.ItemsCount);
        }

        [Fact]
        public static void AddResourceItemTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            var resource = new RDFResource("https://www.index.hu");
            collection.AddItem(resource);
            Assert.Equal(1, collection.ItemsCount);
        }

        [Fact]
        public static void EmptyCollectionTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            Assert.Equal(0, collection.ItemsCount);
        }

        [Fact]
        public static void DuplicatedItemsTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            var item1 = new RDFResource("https://index.hu");
            var item2 = new RDFResource("https://index.hu");

            collection.AddItem(item1);
            collection.AddItem(item2);
            Assert.Equal(1, collection.ItemsCount);
        }
        #endregion

        #region RemoveTests
        [Fact]
        public static void RemoveResourceItemTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            var resource = new RDFResource("https://www.index.hu");
            collection.AddItem(resource);

            collection.RemoveItem(resource);
            Assert.Equal(RDFVocabulary.RDF.NIL, collection.ReificationSubject);
            Assert.Equal(0, collection.ItemsCount);
        }

        [Fact]
        public static void RemoveResourceItemTest_NotSameObject()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            var resource1 = new RDFResource("https://www.index.hu");
            collection.AddItem(resource1);

            // Not the same object but their are equals
            var resource2 = new RDFResource("https://www.index.hu");

            collection.RemoveItem(resource2);
            Assert.Equal(RDFVocabulary.RDF.NIL, collection.ReificationSubject);
            Assert.Equal(0, collection.ItemsCount);
        }

        [Fact]
        public static void ClearItemsTest()
        {
            var collection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
            var resource1 = new RDFResource("https://www.index.hu");
            var resource2 = new RDFResource("https://www.google.com");
            collection.AddItem(resource1);
            collection.AddItem(resource2);

            collection.ClearItems();
            Assert.Equal(RDFVocabulary.RDF.NIL, collection.ReificationSubject);
            Assert.Equal(0, collection.ItemsCount);
        }
        #endregion
    }
}
