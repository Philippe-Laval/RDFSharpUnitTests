using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharpTests.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit;

// https://www.w3.org/TR/sparql11-overview/
// https://www.w3.org/TR/rdf-sparql-query/


namespace RDFSharpTests.QueryTests
{
    public class SelectQueryTests
    {
        private readonly RDFGraph graph;
        private readonly RDFResource donaldduck;
        private readonly RDFResource mickeyMouse;
        private readonly RDFResource pluto;

        public SelectQueryTests()
        {
            graph = GraphBuilder2.WaltDisneyGraphBuild();
            donaldduck = GraphBuilder2.donaldDuck;
            mickeyMouse = GraphBuilder2.mickeyMouse;
            pluto = GraphBuilder2.pluto;
        }

        [Fact]
        public void SelectDogs()
        {
            // CREATE SELECT QUERY
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("dc"));
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"));

            var x = new RDFVariable("x");
            var y = new RDFVariable("y");

            RDFPatternMember predicate = GraphBuilder2.dogOf;

            var x_dogOf_y = new RDFPattern(x, predicate, y);

            // CREATE PATTERN GROUP FROM A LIST OF PATTERNS
            var patterns = new List<RDFPattern>() { x_dogOf_y };
            var pg1 = new RDFPatternGroup("PG1", patterns);

            // ADD PATTERN GROUPS TO QUERY
            selectQuery.AddPatternGroup(pg1);

            selectQuery.AddProjectionVariable(x);
            selectQuery.AddProjectionVariable(y);

            // APPLY SELECT QUERY TO GRAPH

            var sparqlCommand = selectQuery.ToString();

            /*
             * Generates this sparql command
             
PREFIX dc: <http://purl.org/dc/elements/1.1/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?X ?Y
WHERE {
  {
    ?X dc:dogOf ?Y .
  }
}             
             */

            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\select_results.srq");

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            DataRow row = selectQueryResult.SelectResults.Rows[0];
            var dog = row[0].ToString();
            var person = row[1].ToString();
            Assert.Equal(pluto.URI.ToString(), dog);
            Assert.Equal(mickeyMouse.URI.ToString(), person);


            /*
             * Generates this file content
             
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="?X" />
    <variable name="?Y" />
  </head>
  <results>
    <result>
      <binding name="?X">
        <uri>https://en.wikipedia.org/wiki/Pluto_(Disney)</uri>
      </binding>
      <binding name="?Y">
        <uri>https://en.wikipedia.org/wiki/Mickey_Mouse</uri>
      </binding>
    </result>
  </results>
</sparql>             
             */
        }

        //var patterns = new List<RDFPattern>() { x_dogOf_y };
        //var pg1 = new RDFPatternGroup("PG1", patterns);
        //patterns = new List<RDFPattern>() { x_catOf_y };
        //var pg2 = new RDFPatternGroup("PG2", patterns);
        // ADD PATTERN GROUPS TO QUERY
        //selectQuery.AddPatternGroup(pg1);
        //selectQuery.AddPatternGroup(pg2);

        [Fact]
        public void SelectDogsOrCats_UnionWithNext_Test()
        {
            // CREATE SELECT QUERY
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("dc"));
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"));

            var x = new RDFVariable("x");
            var y = new RDFVariable("y");

            RDFPatternMember dogOfPredicate = GraphBuilder2.dogOf;
            RDFPatternMember catOfPredicate = GraphBuilder2.catOf;

            var x_dogOf_y = new RDFPattern(x, dogOfPredicate, y);
            var x_catOf_y = new RDFPattern(x, catOfPredicate, y);

            //var orFilter = new RDFBooleanOrFilter(x_dogOf_y, x_catOf_y);

            // CREATE PATTERN GROUP FROM A LIST OF PATTERNS
            var pg1 = new RDFPatternGroup("PG1");
            pg1.AddPattern(x_dogOf_y.UnionWithNext());
            pg1.AddPattern(x_catOf_y);

            selectQuery.AddPatternGroup(pg1);

            selectQuery.AddProjectionVariable(x);
            selectQuery.AddProjectionVariable(y);

            selectQuery.AddModifier(new RDFOrderByModifier(x, RDFQueryEnums.RDFOrderByFlavors.ASC));

            

            var sparqlCommand = selectQuery.ToString();

            /*
             * Generates this sparql command

PREFIX dc: <http://purl.org/dc/elements/1.1/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?X ?Y
WHERE {
  {
    { ?X dc:dogOf ?Y }
    UNION
    { ?X dc:catOf ?Y }
  }
}
ORDER BY ASC(?X)
*/

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\select_results.srq");

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            //DataRow row = selectQueryResult.SelectResults.Rows[0];
            //var dog = row[0].ToString();
            //var person = row[1].ToString();
            //Assert.Equal(pluto.URI.ToString(), dog);
            //Assert.Equal(mickeyMouse.URI.ToString(), person);


            /*
             * Generates this file content
             
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="?X" />
    <variable name="?Y" />
  </head>
  <results>
    <result>
      <binding name="?X">
        <uri>https://en.wikipedia.org/wiki/Figaro_(Disney)</uri>
      </binding>
      <binding name="?Y">
        <uri>https://en.wikipedia.org/wiki/Minnie_Mouse</uri>
      </binding>
    </result>
    <result>
      <binding name="?X">
        <uri>https://en.wikipedia.org/wiki/Pluto_(Disney)</uri>
      </binding>
      <binding name="?Y">
        <uri>https://en.wikipedia.org/wiki/Mickey_Mouse</uri>
      </binding>
    </result>
  </results>
</sparql>            
             */
        }


    }
}
