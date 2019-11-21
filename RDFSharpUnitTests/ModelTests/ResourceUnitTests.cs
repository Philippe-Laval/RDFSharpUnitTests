using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDFSharp.Model;
using Xunit;

namespace RDFSharpTests.ModelTests
{
    /*
     By creating an RDFResource a NODE
       or instance of a URI it returns true,
       if the given parameter corresponds to a resource
    */

    public static class ResourceUnitTests
    {
        [Fact]
        public static void CreatingResourceWithValidURITest()
        {
            // From a URI specified as a resource string
            var validURI = new RDFResource("https://www.youtube.com/");
            Assert.Equal("https://www.youtube.com/", validURI.URI.ToString());
            Assert.False(validURI.IsBlank);
        }

        [Fact]
        public static void CreatingBlankResourceTest()
        {
            // We can also create a blank node (the URI will be "bnode:" + Guid)
            var validURI = new RDFResource();
            Assert.StartsWith("bnode:", validURI.URI.ToString());
            Assert.True(validURI.IsBlank);
        }

        [Fact]
        public static void CreatingResourceWithInValidURITest()
        {
            Assert.Throws<RDFModelException>(() => new RDFResource("invalid input which is not a URI"));
        }
    }
}
