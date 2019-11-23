using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

// Try to reproduce the example in the document :https://www.w3.org/TR/2013/REC-sparql11-query-20130321/

namespace RDFSharpTests.QueryTests
{
    public class SparqlQueryTests
    {
        private string GetPath(string relativePath)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            return Path.Combine(dirPath, relativePath);
        }

        [Fact]
        public void CheckWeCanLoadTestFile()
        {
            // The example data is in file "Test2.ttl"
            string filePath = GetPath(@"Files\Test2.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(1, graph.TriplesCount);
        }

        // 2 Making Simple Queries (Informative)
        // 2.1 Writing a Simple Query

        /*
@prefix dc: <http://purl.org/dc/elements/1.1/> .
@prefix : <http://example.org/book/> .
:book1  dc:title  "SPARQL Tutorial" . 
        */

        /*
SELECT ?title
WHERE
{
  <http://example.org/book/book1> <http://purl.org/dc/elements/1.1/title> ?title .
}         
         */

        /// <summary>
        /// 2.1 Writing a Simple Query
        /// </summary>
        [Fact]
        public void WritingSingleQuery_Detailled()
        {
            string filePath = GetPath(@"Files\Test2.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            RDFSelectQuery selectQuery = new RDFSelectQuery();


            // ?title
            var title = new RDFVariable("title");

            // <http://example.org/book/book1> <http://purl.org/dc/elements/1.1/title> ?title .
            var pattern = new RDFPattern(new RDFResource("http://example.org/book/book1"), new RDFResource("http://purl.org/dc/elements/1.1/title"), title);

            /*
WHERE
{
  <http://example.org/book/book1> <http://purl.org/dc/elements/1.1/title> ?title .
} 
             */
            var patternGroup = new RDFPatternGroup("PG1");
            patternGroup.AddPattern(pattern);

            selectQuery.AddPatternGroup(patternGroup);

            // SELECT ?title
            selectQuery.AddProjectionVariable(title);

            var sparqlCommand = selectQuery.ToString();

            #region Generated SPARQL command
            /*
             * Generates this sparql command

SELECT ?TITLE
WHERE {
  {
    <http://example.org/book/book1> <http://purl.org/dc/elements/1.1/title> ?TITLE .
  }
}

*/
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            // selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\WritingSingleQuery_Detailled.srq");

            #region Generated result file
            /*
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="?TITLE" />
  </head>
  <results>
    <result>
      <binding name="?TITLE">
        <literal>SPARQL Tutorial</literal>
      </binding>
    </result>
  </results>
</sparql>
            */
            #endregion

            Assert.Equal(1, selectQueryResult.SelectResultsCount);
            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0][0]);
        }


        /// <summary>
        /// 2.1 Writing a Simple Query
        /// </summary>
        [Fact]
        public void WritingSingleQuery_Short()
        {
            string filePath = GetPath(@"Files\Test2.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // Create variables
            var title = new RDFVariable("title");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(
                        new RDFResource("http://example.org/book/book1"),
                        new RDFResource("http://purl.org/dc/elements/1.1/title"),
                        title)))
                .AddProjectionVariable(title);

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\WritingSingleQuery_Short.srq");

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0][0]);
        }

        /// <summary>
        /// 2.2 Multiple Matches
        /// This is a basic graph pattern match; all the variables used in the query pattern must be bound in every solution.
        /// </summary>
        [Fact]
        public void MultipleMatches()
        {
            string filePath = GetPath(@"Files\Test3.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
             * Query
             
            PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
            SELECT ?name ?mbox
            WHERE
              { ?x foaf:name ?name .
                ?x foaf:mbox ?mbox } 
            
             */


            // Create variables
            var name = new RDFVariable("name");
            var mbox = new RDFVariable("mbox");
            var x = new RDFVariable("x");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, new RDFResource("http://xmlns.com/foaf/0.1/name"), name))
                .AddPattern(new RDFPattern(x, new RDFResource("http://xmlns.com/foaf/0.1/mbox"), mbox)))
                .AddProjectionVariable(name)
                .AddProjectionVariable(mbox);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\MultipleMatches.srq");

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("?NAME", selectQueryResult.SelectResults.Columns[0].ColumnName);
            Assert.Equal("?MBOX", selectQueryResult.SelectResults.Columns[1].ColumnName);

            Assert.Equal("Johnny Lee Outlaw", selectQueryResult.SelectResults.Rows[0][0]);
            Assert.Equal("mailto:jlow@example.com", selectQueryResult.SelectResults.Rows[0][1]);
            Assert.Equal("Peter Goodguy", selectQueryResult.SelectResults.Rows[1][0]);
            Assert.Equal("mailto:peter@example.org", selectQueryResult.SelectResults.Rows[1][1]);

            Assert.Equal("Johnny Lee Outlaw", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("mailto:jlow@example.com", selectQueryResult.SelectResults.Rows[0]["?MBOX"]);
            Assert.Equal("Peter Goodguy", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("mailto:peter@example.org", selectQueryResult.SelectResults.Rows[1]["?MBOX"]);
        }

        /// <summary>
        /// 2.2 Multiple Matches
        /// This is a basic graph pattern match; all the variables used in the query pattern must be bound in every solution.
        /// </summary>
        [Fact]
        public void MultipleMatches_WithNamespace()
        {
            string filePath = GetPath(@"Files\Test3.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
             * Query
             
            PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
            SELECT ?name ?mbox
            WHERE
              { ?x foaf:name ?name .
                ?x foaf:mbox ?mbox } 
            
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Create variables
            var name = new RDFVariable("name");
            var mbox = new RDFVariable("mbox");
            var x = new RDFVariable("x");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, new RDFResource(foafNs + "name"), name))
                .AddPattern(new RDFPattern(x, new RDFResource(foafNs + "mbox"), mbox)))
                .AddProjectionVariable(name)
                .AddProjectionVariable(mbox);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\MultipleMatches_WithNamespace.srq");

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("?NAME", selectQueryResult.SelectResults.Columns[0].ColumnName);
            Assert.Equal("?MBOX", selectQueryResult.SelectResults.Columns[1].ColumnName);

            Assert.Equal("Johnny Lee Outlaw", selectQueryResult.SelectResults.Rows[0][0]);
            Assert.Equal("mailto:jlow@example.com", selectQueryResult.SelectResults.Rows[0][1]);
            Assert.Equal("Peter Goodguy", selectQueryResult.SelectResults.Rows[1][0]);
            Assert.Equal("mailto:peter@example.org", selectQueryResult.SelectResults.Rows[1][1]);

            Assert.Equal("Johnny Lee Outlaw", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("mailto:jlow@example.com", selectQueryResult.SelectResults.Rows[0]["?MBOX"]);
            Assert.Equal("Peter Goodguy", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("mailto:peter@example.org", selectQueryResult.SelectResults.Rows[1]["?MBOX"]);
        }

        /// <summary>
        /// 2.3 Matching RDF Literals
        /// 2.3.1 Matching Literals with Language Tags
        /// </summary>
        [Fact]
        public void MatchingLiteralsWithLanguageTags()
        {
            string filePath = GetPath(@"Files\Test4.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // This following query has no solution because "cat" is not the same RDF literal as "cat"@en:
            // SELECT ?v WHERE { ?v ?p "cat" }

            // Create variables
            var v = new RDFVariable("v");
            var p = new RDFVariable("p");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(v, p, new RDFPlainLiteral("cat"))))
                .AddProjectionVariable(v);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\MatchingLiteralsWithLanguageTags.srq");

            Assert.Equal(0, selectQueryResult.SelectResultsCount);

            // but the query below will find a solution where variable v is bound to :x because the language tag is specified and matches the given data:
            // SELECT ?v WHERE { ?v ?p "cat"@en }

            selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(v, p, new RDFPlainLiteral("cat", "en"))))
                .AddProjectionVariable(v);

            sparqlCommand = selectQuery.ToString();

            selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);
            Assert.Equal("http://example.org/ns#x", selectQueryResult.SelectResults.Rows[0]["?V"]);
        }

        // Not possible to build a query with 42

        /// <summary>
        /// 2.3 Matching RDF Literals
        /// 2.3.2 Matching Literals with Numeric Types
        /// </summary>
        [Fact]
        public void MatchingLiteralsWithNumericTypes()
        {
            string filePath = GetPath(@"Files\Test4.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // This following query has no solution because "cat" is not the same RDF literal as "cat"@en:
            // SELECT ?v WHERE { ?v ?p 42 }

            // Create variables
            var v = new RDFVariable("v");
            var p = new RDFVariable("p");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(v, p, new RDFTypedLiteral("42", RDFModelEnums.RDFDatatypes.XSD_INTEGER))))
                .AddProjectionVariable(v);

            /*
SELECT ?V
WHERE {
  {
    ?V ?P "42"^^<http://www.w3.org/2001/XMLSchema#integer> .
  }
}
             */

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\MatchingLiteralsWithNumericTypes.srq");

            Assert.Equal(1, selectQueryResult.SelectResultsCount);
            Assert.Equal("http://example.org/ns#y", selectQueryResult.SelectResults.Rows[0]["?V"]);
        }



        /// <summary>
        /// 2.3 Matching RDF Literals
        /// 2.3.3 Matching Literals with Arbitrary Datatypes
        /// </summary>
        [Fact]
        public void MatchingLiteralsWithArbitraryDatatypes()
        {
            string filePath = GetPath(@"Files\Test4.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // SELECT ?v WHERE { ?v ?p "abc"^^<http://example.org/datatype#specialDatatype> }

            // Create variables
            var v = new RDFVariable("v");
            var p = new RDFVariable("p");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(v, p, new RDFTypedLiteral("abc", RDFModelEnums.RDFDatatypes.XSD_STRING))))
                .AddProjectionVariable(v);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\MatchingLiteralsWithArbitraryDatatypes.srq");

            // Fails since it is Not possible to build a litteral : "abc"^^<http://example.org/datatype#specialDatatype>

            Assert.Equal(1, selectQueryResult.SelectResultsCount);
            Assert.Equal("http://example.org/ns#z", selectQueryResult.SelectResults.Rows[0]["?V"]);

        }

        /// <summary>
        /// 2.4 Blank Node Labels in Query Results
        /// </summary>
        [Fact]
        public void BlankNodeLabelsInQueryResults()
        {
            string filePath = GetPath(@"Files\Test5.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
SELECT ?x ?name
WHERE  { ?x foaf:name ?name }             
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Create variables
            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, new RDFResource(foafNs + "name"), name)))
                .AddProjectionVariable(x)
                .AddProjectionVariable(name);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\BlankNodeLabelsInQueryResults.srq");

            Assert.Equal(2, selectQueryResult.SelectResultsCount);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("bnode:a", selectQueryResult.SelectResults.Rows[0]["?X"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("bnode:b", selectQueryResult.SelectResults.Rows[1]["?X"]);
        }

        /// <summary>
        /// 2.5 Creating Values with Expressions
        /// </summary>
        [Fact]
        public void CreatingValuesWithExpressions()
        {
            string filePath = GetPath(@"Files\Test6.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // Problem can not use concat or bind

            /*
Query:

PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
SELECT ( CONCAT(?G, " ", ?S) AS ?name )
WHERE  { ?P foaf:givenName ?G ; foaf:surname ?S }

Query:

PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
SELECT ?name
WHERE  { 
   ?P foaf:givenName ?G ; 
      foaf:surname ?S 
   BIND(CONCAT(?G, " ", ?S) AS ?name)
}   
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Create variables
            var g = new RDFVariable("G");
            var s = new RDFVariable("S");
            var p = new RDFVariable("P");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(p, new RDFResource(foafNs + "givenName"), g))
                .AddPattern(new RDFPattern(p, new RDFResource(foafNs + "surname"), s)))
                .AddProjectionVariable(g)
                .AddProjectionVariable(s)
                .AddProjectionVariable(name);

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\CreatingValuesWithExpressions.srq");


            //Assert.Equal(2, selectQueryResult.SelectResultsCount);
            //Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            //Assert.Equal("bnode:a", selectQueryResult.SelectResults.Rows[0]["?X"]);
            //Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            //Assert.Equal("bnode:b", selectQueryResult.SelectResults.Rows[1]["?X"]);
        }

        /// <summary>
        /// 2.6 Building RDF Graphs
        /// </summary>
        [Fact]
        public void BuildingRdfGraphs()
        {
            string filePath = GetPath(@"Files\Test7.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
PREFIX org:    <http://example.com/ns#>

CONSTRUCT { ?x foaf:name ?name }
WHERE  { ?x org:employeeName ?name }             
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");
            var orgNs = new RDFNamespace("org", "http://example.com/ns#");

            // Create variables
            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFConstructQuery constructQuery = new RDFConstructQuery()
                .AddPrefix(foafNs)
                .AddPrefix(orgNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, new RDFResource(orgNs + "employeeName"), name)))
                .AddTemplate(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name));

            var sparqlCommand = constructQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFConstructQueryResult constructQueryResult = constructQuery.ApplyToGraph(graph);

            Assert.Equal(2, constructQueryResult.ConstructResultsCount);

            Assert.Equal("bnode:a", constructQueryResult.ConstructResults.Rows[0][0]);
            Assert.Equal("bnode:b", constructQueryResult.ConstructResults.Rows[1][0]);

            Assert.Equal("http://xmlns.com/foaf/0.1/name", constructQueryResult.ConstructResults.Rows[0][1]);
            Assert.Equal("http://xmlns.com/foaf/0.1/name", constructQueryResult.ConstructResults.Rows[1][1]);

            Assert.Equal("Alice", constructQueryResult.ConstructResults.Rows[0][2]);
            Assert.Equal("Bob", constructQueryResult.ConstructResults.Rows[1][2]);

        }

        // 3 RDF Term Constraints (Informative)

        /// <summary>
        /// 3.1 Restricting the Value of Strings
        /// </summary>
        [Fact]
        public void RestrictingTheValueOfString()
        {
            string filePath = GetPath(@"Files\Test8.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
             
PREFIX  dc:  <http://purl.org/dc/elements/1.1/>
SELECT  ?title
WHERE   { ?x dc:title ?title
          FILTER regex(?title, "^SPARQL") 
        }
             
             */

            // CREATE NAMESPACE
            var dcfNs = RDFNamespaceRegister.GetByPrefix("dc");

            // Create variables
            var title = new RDFVariable("title");
            var x = new RDFVariable("x");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dcfNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, RDFVocabulary.DC.TITLE, title))
                .AddFilter(new RDFRegexFilter(title, new Regex(@"^SPARQL", RegexOptions.None))))
                .AddProjectionVariable(title);

            /*
PREFIX dc: <http://purl.org/dc/elements/1.1/>

SELECT ?TITLE
WHERE {
  {
    ?X dc:title ?TITLE .
    FILTER ( REGEX(STR(?TITLE), "^SPARQL") ) 
  }
}             
             */

            var sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0][0]);

            /*
            PREFIX  dc:  <http://purl.org/dc/elements/1.1/>
            SELECT  ?title
            WHERE   { ?x dc:title ?title
                      FILTER regex(?title, "web", "i" ) 
                    } 
            */

            selectQuery = new RDFSelectQuery()
                .AddPrefix(dcfNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, RDFVocabulary.DC.TITLE, title))
                .AddFilter(new RDFRegexFilter(title, new Regex(@"web", RegexOptions.IgnoreCase))))
                .AddProjectionVariable(title);

            sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("The Semantic Web", selectQueryResult.SelectResults.Rows[0][0]);
        }



        /// <summary>
        /// 3.2 Restricting Numeric Values
        /// </summary>
        [Fact]
        public void RestrictingNumericValues()
        {
            string filePath = GetPath(@"Files\Test8.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
    PREFIX  dc:  <http://purl.org/dc/elements/1.1/>
    PREFIX  ns:  <http://example.org/ns#>
    SELECT  ?title ?price
    WHERE   { ?x ns:price ?price .
              FILTER (?price < 30.5)
              ?x dc:title ?title . }         
             */

            // CREATE NAMESPACE
            var dcfNs = RDFNamespaceRegister.GetByPrefix("dc");
            var nsfNs = new RDFNamespace("ns", "http://example.org/ns#");

            // Create variables
            var title = new RDFVariable("title");
            var price = new RDFVariable("price");
            var x = new RDFVariable("x");

            // Attention si on prend un RDFPlainLiteral("30.5") cela ne marche pas

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dcfNs)
                .AddPrefix(nsfNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, new RDFResource(nsfNs + "price"), price))
                .AddFilter(new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan, price, new RDFTypedLiteral("30.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)))
                .AddPattern(new RDFPattern(x, RDFVocabulary.DC.TITLE, title)))
                .AddProjectionVariable(title)
                .AddProjectionVariable(price);

            /*

PREFIX dc: <http://purl.org/dc/elements/1.1/>
PREFIX ns: <http://example.org/ns#>

SELECT ?TITLE ?PRICE
WHERE {
  {
    ?X ns:price ?PRICE .
    ?X dc:title ?TITLE .
    FILTER ( ?PRICE < "30.5"^^<http://www.w3.org/2001/XMLSchema#float> ) 
  }
}
         
             */

            string sparqlCommand = selectQuery.ToString();

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("The Semantic Web", selectQueryResult.SelectResults.Rows[0][0]);
            var o = selectQueryResult.SelectResults.Rows[0][1];
            Assert.Equal("23^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0][1]);

            // Attention on devrait avoir 23
            Assert.Equal("23", selectQueryResult.SelectResults.Rows[0][1]);
        }

        // 6 Including Optional Values

        /// <summary>
        /// 6.1 Optional Pattern Matching
        /// </summary>
        [Fact]
        public void OptionalPatternMatching()
        {
            string filePath = GetPath(@"Files\Test9.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
SELECT ?name ?mbox
WHERE  { ?x foaf:name  ?name .
         OPTIONAL { ?x  foaf:mbox  ?mbox }
       }
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Create variables
            var name = new RDFVariable("name");
            var mbox = new RDFVariable("mbox");
            var x = new RDFVariable("x");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, mbox).Optional()))
                .AddProjectionVariable(name)
                .AddProjectionVariable(mbox);

            #region generated sparql query
            /*
            PREFIX foaf: <http://xmlns.com/foaf/0.1/>

            SELECT ?NAME ?MBOX
            WHERE {
              {
                ?X foaf:name ?NAME .
                OPTIONAL { ?X foaf:mbox ?MBOX } .
              }
            }
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(3, selectQueryResult.SelectResultsCount);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[2]["?NAME"]);

            Assert.Equal("mailto:alice@example.com", selectQueryResult.SelectResults.Rows[0]["?MBOX"]);
            Assert.Equal("mailto:alice@work.example", selectQueryResult.SelectResults.Rows[1]["?MBOX"]);
            Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[2]["?MBOX"]);
        }

        /// <summary>
        /// 6.2 Constraints in Optional Pattern Matching
        /// </summary>
        [Fact]
        public void ConstraintsInOptionalPatternMatching()
        {
            string filePath = GetPath(@"Files\Test10.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX  dc:  <http://purl.org/dc/elements/1.1/>
PREFIX  ns:  <http://example.org/ns#>
SELECT  ?title ?price
WHERE   { ?x dc:title ?title .
          OPTIONAL { ?x ns:price ?price . FILTER (?price < 30) }
        }
             */

            // CREATE NAMESPACE
            var dcNs = RDFNamespaceRegister.GetByPrefix("dc");
            var nsNs = new RDFNamespace("ns", "http://example.org/ns#");

            // Create variables
            var title = new RDFVariable("title");
            var price = new RDFVariable("price");
            var x = new RDFVariable("x");

            // Attention si on prend un RDFPlainLiteral("30") cela ne marche pas

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dcNs)
                .AddPrefix(nsNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.DC.TITLE, title)))
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(x, new RDFResource(nsNs + "price"), price))
                    .AddFilter(new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan, price, new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INT)))
                    .Optional())
                .AddProjectionVariable(title)
                .AddProjectionVariable(price);

            #region generated sparql query
            /*
PREFIX dc: <http://purl.org/dc/elements/1.1/>
PREFIX ns: <http://example.org/ns#>

SELECT ?TITLE ?PRICE
WHERE {
  {
    ?X dc:title ?TITLE .
  }
  OPTIONAL {
    {
      ?X ns:price ?PRICE .
      FILTER ( ?PRICE < "30"^^<http://www.w3.org/2001/XMLSchema#int> ) 
    }
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);


            Assert.Equal(2, selectQueryResult.SelectResultsCount);
            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0]["?TITLE"]);
            Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[0]["?PRICE"]);

            Assert.Equal("The Semantic Web", selectQueryResult.SelectResults.Rows[1]["?TITLE"]);
            Assert.Equal("23^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[1]["?PRICE"]);
        }

        /// <summary>
        /// 6.3 Multiple Optional Graph Patterns
        /// </summary>
        [Fact]
        public void MultipleOptionalGraphPatterns()
        {
            string filePath = GetPath(@"Files\Test11.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
SELECT ?name ?mbox ?hpage
WHERE  { ?x foaf:name  ?name .
         OPTIONAL { ?x foaf:mbox ?mbox } .
         OPTIONAL { ?x foaf:homepage ?hpage }
       }
             */

            // CREATE NAMESPACE
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Create variables
            var name = new RDFVariable("name");
            var mbox = new RDFVariable("mbox");
            var hpage = new RDFVariable("hpage");
            var x = new RDFVariable("x");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name)))
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, mbox))
                    .Optional())
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.HOMEPAGE, hpage))
                    .Optional())
                .AddProjectionVariable(name)
                .AddProjectionVariable(mbox)
                .AddProjectionVariable(hpage);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME ?MBOX ?HPAGE
WHERE {
  {
    ?X foaf:name ?NAME .
  }
  OPTIONAL {
    {
      ?X foaf:mbox ?MBOX .
    }
  }
  OPTIONAL {
    {
      ?X foaf:homepage ?HPAGE .
    }
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[0]["?MBOX"]);
            Assert.Equal("http://work.example.org/alice/", selectQueryResult.SelectResults.Rows[0]["?HPAGE"]);

            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("mailto:bob@work.example", selectQueryResult.SelectResults.Rows[1]["?MBOX"]);
            Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[1]["?HPAGE"]);
        }

        /// <summary>
        /// 7 Matching Alternatives
        /// </summary>
        [Fact]
        public void MatchingAlternatives()
        {
            Console.WriteLine($"Count: {RDFNamespaceRegister.NamespacesCount}");

            foreach (var t in RDFNamespaceRegister.Instance)
            {
                Console.WriteLine($"{t.NamespacePrefix}:{t.NamespaceUri}");
            }


            var dc10Ns = new RDFNamespace("dc10", "http://purl.org/dc/elements/1.0/");

            // On ne peut l'enregistrer car "http://purl.org/dc/elements/1.1/" existe déjà pour le prefix "dc"
            var dc11Ns = new RDFNamespace("dc11", "http://purl.org/dc/elements/1.1/");

            RDFNamespaceRegister.AddNamespace(dc10Ns);
            RDFNamespaceRegister.AddNamespace(dc11Ns);

            string filePath = GetPath(@"Files\Test12.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            // **** Problem parsing the file *****
            // we get node with predicate : {https://rdfsharp.codeplex.com/title}

            Assert.Equal(6, graph.TriplesCount);

            /*
PREFIX dc10:  <http://purl.org/dc/elements/1.0/>
PREFIX dc11:  <http://purl.org/dc/elements/1.1/>

SELECT ?title
WHERE  { { ?book dc10:title  ?title } UNION { ?book dc11:title  ?title } }
             */

            // CREATE NAMESPACE
            //var dcNs = RDFNamespaceRegister.GetByPrefix("dc"); // http://purl.org/dc/elements/1.1/


            // Create variables
            var title = new RDFVariable("title");
            var book = new RDFVariable("book");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dc10Ns)
                .AddPrefix(dc11Ns)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(book, new RDFResource(dc10Ns + "title"), title).UnionWithNext())
                    .AddPattern(new RDFPattern(book, new RDFResource(dc11Ns + "title"), title)))
                .AddProjectionVariable(title);

            #region generated sparql query
            /*
PREFIX dc10: <http://purl.org/dc/elements/1.0/>
PREFIX dc11: <http://purl.org/dc/elements/1.1/>

SELECT ?TITLE
WHERE {
  {
    { ?BOOK dc10:title ?TITLE }
    UNION
    { ?BOOK dc11:title ?TITLE }
  }
}

            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            //Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            //Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[0]["?MBOX"]);
            //Assert.Equal("http://work.example.org/alice/", selectQueryResult.SelectResults.Rows[0]["?HPAGE"]);

            //Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            //Assert.Equal("mailto:bob@work.example", selectQueryResult.SelectResults.Rows[1]["?MBOX"]);
            //Assert.Equal(DBNull.Value, selectQueryResult.SelectResults.Rows[1]["?HPAGE"]);
        }

        /// <summary>
        /// 7 Matching Alternatives
        /// </summary>
        [Fact]
        public void MatchingAlternatives_Tweak()
        {
            var dcNs = RDFNamespaceRegister.GetByPrefix("dc"); // http://purl.org/dc/elements/1.1/

            // CREATE NAMESPACE
            var dc10Ns = new RDFNamespace("dc10", "http://purl.org/dc/elements/1.0/");
            RDFNamespaceRegister.AddNamespace(dc10Ns);

            // **** Tweak to force the good loading with predicate http://purl.org/dc/elements/1.1/
            RDFNamespaceRegister.SetDefaultNamespace(dcNs);

            string filePath = GetPath(@"Files\Test12.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
PREFIX dc10:  <http://purl.org/dc/elements/1.0/>
PREFIX dc11:  <http://purl.org/dc/elements/1.1/>

SELECT ?title
WHERE  { { ?book dc10:title  ?title } UNION { ?book dc11:title  ?title } }
             */

            // Create variables
            var title = new RDFVariable("title");
            var book = new RDFVariable("book");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dc10Ns)
                .AddPrefix(dcNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(book, new RDFResource(dc10Ns + "title"), title).UnionWithNext())
                    .AddPattern(new RDFPattern(book, new RDFResource(dcNs + "title"), title)))
                .AddProjectionVariable(title);

            #region generated sparql query
            /*
PREFIX dc10: <http://purl.org/dc/elements/1.0/>
PREFIX dc11: <http://purl.org/dc/elements/1.1/>

SELECT ?TITLE
WHERE {
  {
    { ?BOOK dc10:title ?TITLE }
    UNION
    { ?BOOK dc11:title ?TITLE }
  }
}

            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("SPARQL Protocol Tutorial", selectQueryResult.SelectResults.Rows[0]["?TITLE"]);
            Assert.Equal("SPARQL (updated)", selectQueryResult.SelectResults.Rows[1]["?TITLE"]);
            Assert.Equal("SPARQL Query Language Tutorial", selectQueryResult.SelectResults.Rows[2]["?TITLE"]);
            Assert.Equal("SPARQL", selectQueryResult.SelectResults.Rows[3]["?TITLE"]);
        }

        /// <summary>
        /// 7 Matching Alternatives
        /// </summary>
        [Fact]
        public void MatchingAlternatives_Example2_Tweak()
        {
            var dcNs = RDFNamespaceRegister.GetByPrefix("dc"); // http://purl.org/dc/elements/1.1/

            // CREATE NAMESPACE
            var dc10Ns = new RDFNamespace("dc10", "http://purl.org/dc/elements/1.0/");
            RDFNamespaceRegister.AddNamespace(dc10Ns);

            // **** Tweak to force the good loading with predicate http://purl.org/dc/elements/1.1/
            RDFNamespaceRegister.SetDefaultNamespace(dcNs);

            string filePath = GetPath(@"Files\Test12.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
    PREFIX dc10:  <http://purl.org/dc/elements/1.0/>
    PREFIX dc11:  <http://purl.org/dc/elements/1.1/>

    SELECT ?x ?y
    WHERE  { { ?book dc10:title ?x } UNION { ?book dc11:title  ?y } }         
            */

            // Create variables
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");
            var book = new RDFVariable("book");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dc10Ns)
                .AddPrefix(dcNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(book, new RDFResource(dc10Ns + "title"), x).UnionWithNext())
                    .AddPattern(new RDFPattern(book, new RDFResource(dcNs + "title"), y)))
                .AddProjectionVariable(x)
                .AddProjectionVariable(y);

            #region generated sparql query
            /*
PREFIX dc10: <http://purl.org/dc/elements/1.0/>
PREFIX dc: <http://purl.org/dc/elements/1.1/>

SELECT ?X ?Y
WHERE {
  {
    { ?BOOK dc10:title ?X }
    UNION
    { ?BOOK dc:title ?Y }
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);
        }

        /// <summary>
        /// 7 Matching Alternatives
        /// </summary>
        [Fact]
        public void MatchingAlternatives_Example3_Tweak()
        {
            var dcNs = RDFNamespaceRegister.GetByPrefix("dc"); // http://purl.org/dc/elements/1.1/

            // CREATE NAMESPACE
            var dc10Ns = new RDFNamespace("dc10", "http://purl.org/dc/elements/1.0/");
            RDFNamespaceRegister.AddNamespace(dc10Ns);

            // **** Tweak to force the good loading with predicate http://purl.org/dc/elements/1.1/
            RDFNamespaceRegister.SetDefaultNamespace(dcNs);

            string filePath = GetPath(@"Files\Test12.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
    PREFIX dc10:  <http://purl.org/dc/elements/1.0/>
    PREFIX dc11:  <http://purl.org/dc/elements/1.1/>

    SELECT ?title ?author
    WHERE  { { ?book dc10:title ?title .  ?book dc10:creator ?author }
             UNION
             { ?book dc11:title ?title .  ?book dc11:creator ?author }
           }         
             */

            // Create variables
            var title = new RDFVariable("title");
            var author = new RDFVariable("author");
            var book = new RDFVariable("book");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(dc10Ns)
                .AddPrefix(dcNs)
                .AddPatternGroup(new RDFPatternGroup("PG1").UnionWithNext()
                    .AddPattern(new RDFPattern(book, new RDFResource(dc10Ns + "title"), title))
                    .AddPattern(new RDFPattern(book, new RDFResource(dc10Ns + "creator"), author)))
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(book, new RDFResource(dcNs + "title"), title))
                    .AddPattern(new RDFPattern(book, new RDFResource(dcNs + "creator"), author)))
                .AddProjectionVariable(title)
                .AddProjectionVariable(author);

            #region generated sparql query
            /*
PREFIX dc10: <http://purl.org/dc/elements/1.0/>
PREFIX dc: <http://purl.org/dc/elements/1.1/>

SELECT ?TITLE ?AUTHOR
WHERE {
  {
    {
      ?BOOK dc10:title ?TITLE .
      ?BOOK dc10:creator ?AUTHOR .
    }
    UNION
    {
      ?BOOK dc:title ?TITLE .
      ?BOOK dc:creator ?AUTHOR .
    }
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("SPARQL Protocol Tutorial", selectQueryResult.SelectResults.Rows[0]["?TITLE"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[0]["?AUTHOR"]);
            Assert.Equal("SPARQL Query Language Tutorial", selectQueryResult.SelectResults.Rows[1]["?TITLE"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[1]["?AUTHOR"]);
        }

        /// <summary>
        /// 8 Negation
        /// 8.1 Filtering Using Graph Patterns
        /// 8.1.1 Testing For the Absence of a Pattern
        /// </summary>
        [Fact]
        public void TestingForTheAbsenceOfPattern()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test13.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(3, graph.TriplesCount);

            /*
PREFIX  rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
PREFIX  foaf:   <http://xmlns.com/foaf/0.1/> 

SELECT ?person
WHERE 
{
    ?person rdf:type  foaf:Person .
    FILTER NOT EXISTS { ?person foaf:name ?name }
}      
             */

            var rdfNs = RDFNamespaceRegister.GetByPrefix("rdf");
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");


            // Create variables
            var person = new RDFVariable("person");
            var name = new RDFVariable("name");

            // FILTER NOT EXISTS { ?person foaf:name ?name }
            var notExistsFilter = new RDFNotExistsFilter(new RDFPattern(person, RDFVocabulary.FOAF.NAME, name));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(rdfNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(person, RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
                    .AddFilter(notExistsFilter))
                .AddProjectionVariable(person);


            #region generated sparql query
            /*
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?PERSON
WHERE {
  {
    ?PERSON rdf:type foaf:Person .
    FILTER ( NOT EXISTS { ?PERSON foaf:name ?NAME } ) 
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example/bob", selectQueryResult.SelectResults.Rows[0]["?PERSON"]);
        }

        /// <summary>
        /// 8.1.2 Testing For the Presence of a Pattern
        /// </summary>
        [Fact]
        public void TestingForThePresenceOfPattern()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test13.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(3, graph.TriplesCount);

            /*
PREFIX  rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
PREFIX  foaf:   <http://xmlns.com/foaf/0.1/> 

SELECT ?person
WHERE 
{
    ?person rdf:type  foaf:Person .
    FILTER EXISTS { ?person foaf:name ?name }
}     
             */

            var rdfNs = RDFNamespaceRegister.GetByPrefix("rdf");
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");


            // Create variables
            var person = new RDFVariable("person");
            var name = new RDFVariable("name");

            // FILTER NOT EXISTS { ?person foaf:name ?name }
            var notExistsFilter = new RDFExistsFilter(new RDFPattern(person, RDFVocabulary.FOAF.NAME, name));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(rdfNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(person, RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
                    .AddFilter(notExistsFilter))
                .AddProjectionVariable(person);


            #region generated sparql query
            /*
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?PERSON
WHERE {
  {
    ?PERSON rdf:type foaf:Person .
    FILTER ( EXISTS { ?PERSON foaf:name ?NAME } ) 
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example/alice", selectQueryResult.SelectResults.Rows[0]["?PERSON"]);
        }

        // 8.2 Removing Possible Solutions

        [Fact]
        public void RemovingPossibleSolutions()
        {
            string filePath = GetPath(@"Files\Test14.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            // NOT Implemented
            Assert.True(false);
        }

        // 8.3 Relationship and differences between NOT EXISTS and MINUS
        // 8.3.1 Example: Sharing of variables

        [Fact]
        public void SharingOfVariablesFILTER_NOT_EXISTS()
        {
            string filePath = GetPath(@"Files\Test15.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(1, graph.TriplesCount);

            /*
SELECT *
{ 
  ?s ?p ?o
  FILTER NOT EXISTS { ?x ?y ?z }
}             
             */

            // Create variables
            var s = new RDFVariable("s");
            var p = new RDFVariable("p");
            var o = new RDFVariable("o");
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");
            var z = new RDFVariable("z");

            // FILTER ( NOT EXISTS { ?X ?X ?Z } ) 
            var notExistsFilter = new RDFNotExistsFilter(new RDFPattern(x, x, z));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(s, p, o))
                    .AddFilter(notExistsFilter));
            // Note : Adding no projection variable => SELECT *

            #region generated sparql query
            /*
SELECT *
WHERE {
  {
    ?S ?P ?O .
    FILTER ( NOT EXISTS { ?X ?X ?Z } ) 
  }
}

            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            if (selectQueryResult.SelectResultsCount == 1)
            {
                // Got "http://example/a"
                var r = selectQueryResult.SelectResults.Rows[0][0];
            }

            // We should have 0 but get 1
            Assert.Equal(0, selectQueryResult.SelectResultsCount);
        }

        [Fact]
        public void SharingOfVariablesMINUS()
        {
            string filePath = GetPath(@"Files\Test15.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(1, graph.TriplesCount);

            /*
SELECT *
{ 
   ?s ?p ?o 
   MINUS 
     { ?x ?y ?z }
}             
             */

            // NOT Implemented
            Assert.True(false);
        }

        // 8.3.2 Example: Fixed pattern

        [Fact]
        public void FixedPatternFILTER_NOT_EXISTS()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test15.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(1, graph.TriplesCount);

            /*
    PREFIX : <http://example/>
    SELECT * 
    { 
      ?s ?p ?o 
      FILTER NOT EXISTS { :a :b :c }
    }         
             */

            // Create variables
            var s = new RDFVariable("s");
            var p = new RDFVariable("p");
            var o = new RDFVariable("o");

            // FILTER NOT EXISTS { :a :b :c }
            var notExistsFilter = new RDFNotExistsFilter(new RDFPattern(new RDFResource(exNs + "a"), new RDFResource(exNs + "b"), new RDFResource(exNs + "c")));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(s, p, o))
                    .AddFilter(notExistsFilter));

            #region generated sparql query
            /*
SELECT *
WHERE {
  {
    ?S ?P ?O .
    FILTER ( NOT EXISTS { ?X ?X ?Z } ) 
  }
}

            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            if (selectQueryResult.SelectResultsCount == 1)
            {
                // Got "http://example/a"
                var r = selectQueryResult.SelectResults.Rows[0][0];
            }

            // We should have 0 but get 1
            Assert.Equal(0, selectQueryResult.SelectResultsCount);

        }

        [Fact]
        public void FixedPatternFILTER_MINUS()
        {
            string filePath = GetPath(@"Files\Test15.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            /*
PREFIX : <http://example/>
SELECT * 
{ 
  ?s ?p ?o 
  MINUS { :a :b :c }
}             
             */

            Assert.Equal(1, graph.TriplesCount);

            // NOT Implemented
            Assert.True(false);
        }

        /// <summary>
        /// 8.3.3 Example: Inner FILTERs
        /// </summary>
        [Fact]
        public void InnerFILTERs__FILTER_NOT_EXISTS_FILTER()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test16.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
            PREFIX : <http://example.com/>
            SELECT * WHERE {
                    ?x :p ?n
                    FILTER NOT EXISTS {
                            ?x :q ?m .
                            FILTER(?n = ?m)
                    }
            } 
            */


            var x = new RDFVariable("x");
            var n = new RDFVariable("n");
            var m = new RDFVariable("m");

            // ?x :q ?m
            var pattern = new RDFPattern(x, new RDFResource(exNs + "q"), m);

            // {FILTER ( SAMETERM(?N, ?M) )}
            var sameTermFilter = new RDFSameTermFilter(n, m);

            // {  {
            // ?X < http://example/q> ?M .
            // FILTER(SAMETERM(?N, ? M))
            //}
            //}
            RDFPatternGroup pg2 = new RDFPatternGroup("PG2").AddPattern(pattern).AddFilter(sameTermFilter);

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, new RDFResource(exNs + "p"), n)));

            #region generated sparql query
            /*

            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // NOT possible to build the query
            Assert.True(false);
        }

        /// <summary>
        /// 8.3.3 Example: Inner FILTERs
        /// </summary>
        [Fact]
        public void InnerFILTERs__MINUS_FILTER()
        {
            // NOT possible to build the query
            Assert.True(false);
        }

        // 9 Property Paths
        // 9.1 Property Path Syntax
        // 9.2 Examples
        [Fact]
        public void PropertyPathSyntax_Alternatives_Test1()
        {
            /*
Alternatives: Match one or both possibilities
{ :book1 dc:title|rdfs:label ?displayString }
which could have writen:
{ :book1 <http://purl.org/dc/elements/1.1/title> | <http://www.w3.org/2000/01/rdf-schema#label> ?displayString }
             */

            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/book/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test17.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(2, graph.TriplesCount);

            var displayString = new RDFVariable("displayString");

            // { :book1 dc:title|rdfs:label ?displayString }
            // start => :book1
            // end => ?displayString

            // {<http://example.org/book/book1>  ?DISPLAYSTRING}
            var variablePropPath = new RDFPropertyPath(new RDFResource(exNs + "book1"), displayString);

            // dc:title|rdfs:label
            var altSteps = new List<RDFPropertyPathStep>();
            altSteps.Add(new RDFPropertyPathStep(RDFVocabulary.DC.TITLE));
            altSteps.Add(new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL));

            // {<http://example.org/book/book1> (<http://purl.org/dc/elements/1.1/title>|<http://www.w3.org/2000/01/rdf-schema#label>) ?DISPLAYSTRING}
            variablePropPath.AddAlternativeSteps(altSteps);

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath))
                .AddProjectionVariable(displayString);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/book/>

SELECT ?DISPLAYSTRING
WHERE {
  {
    ex:book1 (<http://purl.org/dc/elements/1.1/title>|<http://www.w3.org/2000/01/rdf-schema#label>) ?DISPLAYSTRING .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("the book1", selectQueryResult.SelectResults.Rows[0]["?DISPLAYSTRING"]);
        }

        [Fact]
        public void PropertyPathSyntax_Alternatives_Test2()
        {
            /*
Alternatives: Match one or both possibilities
{ ?book dc:title|rdfs:label ?displayString }
which could have writen:
{ ?book <http://purl.org/dc/elements/1.1/title> | <http://www.w3.org/2000/01/rdf-schema#label> ?displayString }
             */

            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/book/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test17.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(2, graph.TriplesCount);

            var book = new RDFVariable("book");
            var displayString = new RDFVariable("displayString");

            // { ?book dc:title|rdfs:label ?displayString }
            // start => ?book
            // end => ?displayString

            // {?BOOK ?DISPLAYSTRING}
            var variablePropPath = new RDFPropertyPath(book, displayString);

            // dc:title|rdfs:label
            var altSteps = new List<RDFPropertyPathStep>();
            altSteps.Add(new RDFPropertyPathStep(RDFVocabulary.DC.TITLE));
            altSteps.Add(new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL));

            // {?BOOK (<http://purl.org/dc/elements/1.1/title>|<http://www.w3.org/2000/01/rdf-schema#label>) ?DISPLAYSTRING}
            variablePropPath.AddAlternativeSteps(altSteps);

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath))
                .AddProjectionVariable(book)
                .AddProjectionVariable(displayString);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/book/>

SELECT ?BOOK ?DISPLAYSTRING
WHERE {
  {
    ?BOOK (<http://purl.org/dc/elements/1.1/title>|<http://www.w3.org/2000/01/rdf-schema#label>) ?DISPLAYSTRING .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            var b1 = selectQueryResult.SelectResults.Rows[0]["?BOOK"].ToString();
            var b2 = selectQueryResult.SelectResults.Rows[1]["?BOOK"].ToString();
            var dp1 = selectQueryResult.SelectResults.Rows[0]["?DISPLAYSTRING"].ToString();
            var dp2 = selectQueryResult.SelectResults.Rows[1]["?DISPLAYSTRING"].ToString();

            if (b1 == "book1")
            {
                Assert.Equal("book2", b2);
                Assert.Equal("the book2", dp2);

                Assert.Equal("the book1", dp1);
            }
            else
            {
                Assert.Equal("http://example.org/book/book2", b1);
                Assert.Equal("the book2", dp1);

                Assert.Equal("http://example.org/book/book1", b2);
                Assert.Equal("the book1", dp2);
            }
        }

        
    }
}
