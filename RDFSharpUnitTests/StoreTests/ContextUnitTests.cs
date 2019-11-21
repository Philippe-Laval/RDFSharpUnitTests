using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Linq;
using Xunit;

namespace RDFSharpTests.StoreTests
{
    public static class ContextUnitTests
    {
        [Fact]
        public static void CreatingEmptyContextTest()
        {
            RDFContext cont = new RDFContext();
            Assert.NotNull(cont);
            Assert.Equal(RDFNamespaceRegister.DefaultNamespace.NamespaceUri, cont.Context);
            Assert.Equal("https://rdfsharp.codeplex.com/", cont.Context.ToString());
        }

        [Fact]
        public static void CreatingContextFromURITest()
        {
            RDFContext cont = new RDFContext(new Uri("http://www.wikipedia.com/"));

            Assert.Equal("http://www.wikipedia.com/", cont.ToString()); 
        }

        [Fact]
        public static void CreatingInvalidContextTest()
        {
            Assert.Throws<RDFStoreException>( () => new RDFContext("not a uri"));
        }
    }
}
