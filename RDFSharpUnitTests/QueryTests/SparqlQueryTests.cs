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

        #region 2 Making Simple Queries (Informative)

        #region 2.1 Writing a Simple Query

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
        #endregion

        #region 2.2 Multiple Matches
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

        #endregion

        #region 2.3 Matching RDF Literals

        #region 2.3.1 Matching Literals with Language Tags
        /// <summary>
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

        #endregion

        // Not possible to build a query with 42

        #region 2.3.2 Matching Literals with Numeric Types

        /// <summary>
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

        #endregion

        #region 2.3.3 Matching Literals with Arbitrary Datatypes
        /// <summary>
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

        #endregion

        #endregion

        #region 2.4 Blank Node Labels in Query Results

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

        #endregion

        #region 2.5 Creating Values with Expressions

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

        #endregion

        #region 2.6 Building RDF Graphs

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

        #endregion

        #endregion

        #region 3 RDF Term Constraints (Informative)

        #region 3.1 Restricting the Value of Strings
        /// <summary>
        /// 
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

        #endregion

        #region 3.2 Restricting Numeric Values

        /// <summary>
        /// 
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

            // We get
            Assert.Equal("23^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0][1]);

            // Attention on devrait avoir 23
            Assert.Equal("23", selectQueryResult.SelectResults.Rows[0][1]);
        }

        #endregion

        #endregion

        #region 4 SPARQL Syntax
        #endregion

        #region 5 Graph Patterns
        #endregion

        #region 6 Including Optional Values

        #region 6.1 Optional Pattern Matching

        /// <summary>
        /// 
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

        #endregion

        #region 6.2 Constraints in Optional Pattern Matching

        /// <summary>
        /// 
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

        #endregion

        #region 6.3 Multiple Optional Graph Patterns

        /// <summary>
        /// 
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

        #endregion

        #endregion

        #region 7 Matching Alternatives

        /// <summary>
        /// 
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
        /// 
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
        /// 
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
        ///
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

        #endregion

        #region 8 Negation

        #region 8.1 Filtering Using Graph Patterns

        #region 8.1.1 Testing For the Absence of a Pattern

        /// <summary>
        /// 
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

        #endregion

        #region 8.1.2 Testing For the Presence of a Pattern

        /// <summary>
        /// 
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

        #endregion

        #endregion

        #region 8.2 Removing Possible Solutions

        [Fact]
        public void RemovingPossibleSolutions()
        {
            string filePath = GetPath(@"Files\Test14.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
PREFIX :       <http://example/>
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>

SELECT DISTINCT ?s
WHERE {
   ?s ?p ?o .
   MINUS {
      ?s foaf:givenName "Bob" .
   }
}
             */

            // MINUS NOT Implemented
            Assert.True(false);
        }

        #endregion

        #region 8.3 Relationship and differences between NOT EXISTS and MINUS

        #region 8.3.1 Example: Sharing of variables

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

            // MINUS NOT Implemented
            Assert.True(false);
        }

        #endregion

        #region 8.3.2 Example: Fixed pattern

        /// <summary>
        /// Not possible to build the filter : FILTER NOT EXISTS { :a :b :c }
        /// </summary>
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

            // Not possible to build : FILTER NOT EXISTS { :a :b :c }
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

        #endregion

        #region 8.3.3 Example: Inner FILTERs

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        [Fact]
        public void InnerFILTERs__MINUS_FILTER()
        {
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

            // NOT possible to build the query
            Assert.True(false);
        }

        #endregion

        #endregion

        #endregion

        #region 9 Property Paths

        #region 9.1 Property Path Syntax
        #endregion

        #region 9.2 Examples

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

        [Fact]
        public void PropertyPathSyntax_AddSequenceStep_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
             Sequence: Find the name of any people that Alice knows.

  {
    ?x foaf:mbox <mailto:alice@example.org> .
    ?x foaf:knows/foaf:name ?name .
  }
             */

            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var name = new RDFVariable("name");
            var x = new RDFVariable("x");


            // ?x foaf:knows/foaf:name ?name
            var variablePropPath = new RDFPropertyPath(x, name)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.NAME));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, new RDFResource("mailto:alice@example.org")))
                    .AddPropertyPath(variablePropPath))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>

SELECT ?NAME
WHERE {
  {
    ?X <http://xmlns.com/foaf/0.1/mbox> <mailto:alice@example.org> .
    ?X <http://xmlns.com/foaf/0.1/knows>/<http://xmlns.com/foaf/0.1/name> ?NAME .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(3, selectQueryResult.SelectResultsCount);

            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Charlie", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Snoopy@EN", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
        }

        [Fact]
        public void PropertyPathSyntax_AddSequenceStep_Test2()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
             Sequence: Find the names of people 2 "foaf:knows" links away.

  { 
    ?x foaf:mbox <mailto:alice@example> .
    ?x foaf:knows/foaf:knows/foaf:name ?name .
  }

This is the same as the SPARQL query:

  SELECT ?x ?name 
  {
     ?x  foaf:mbox <mailto:alice@example> .
     ?x  foaf:knows [ foaf:knows [ foaf:name ?name ]]. 
  }

or, with explicit variables:

  SELECT ?x ?name
  {
    ?x  foaf:mbox <mailto:alice@example> .
    ?x  foaf:knows ?a1 .
    ?a1 foaf:knows ?a2 .
    ?a2 foaf:name ?name .
  }         
             */

            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var name = new RDFVariable("name");
            var x = new RDFVariable("x");


            // ?x foaf:knows/foaf:knows/foaf:name ?name
            var variablePropPath = new RDFPropertyPath(x, name)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.NAME));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, new RDFResource("mailto:alice@example.org")))
                    .AddPropertyPath(variablePropPath))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>

SELECT ?NAME
WHERE {
  {
    ?X <http://xmlns.com/foaf/0.1/mbox> <mailto:alice@example.org> .
    ?X <http://xmlns.com/foaf/0.1/knows>/<http://xmlns.com/foaf/0.1/knows>/<http://xmlns.com/foaf/0.1/name> ?NAME .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
        }

        [Fact]
        public void PropertyPathSyntax_AddSequenceStep_Test3()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
        Filtering duplicates: Because someone Alice knows may well know Alice, the example above may include Alice herself. This could be avoided with:

 { ?x foaf:mbox <mailto:alice@example.org> .
   ?x foaf:knows/foaf:knows ?y .
   FILTER ( ?x != ?y )
   ?y foaf:name ?name 
 }
        */

            string filePath = GetPath(@"Files\Test18.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(16, graph.TriplesCount);

            var name = new RDFVariable("name");
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");


            // ?x foaf:knows/foaf:knows/foaf:name ?y
            var variablePropPath = new RDFPropertyPath(x, y)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS));

            // {FILTER ( ?X != ?Y )}
            var filter1 = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, x, y);
            // FILTER ( !( SAMETERM(?X, ?Y) ) ) 
            var filter2 = new RDFBooleanNotFilter(new RDFSameTermFilter(x, y));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    // ?x foaf:mbox <mailto:alice@example.org>
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, new RDFResource("mailto:alice@example.org")))
                    .AddPropertyPath(variablePropPath)
                    // FILTER ( ?x != ?y )
                    .AddFilter(filter1)
                    .AddPattern(new RDFPattern(y, RDFVocabulary.FOAF.NAME, name)))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>

SELECT ?NAME
WHERE {
  {
    ?X <http://xmlns.com/foaf/0.1/mbox> <mailto:alice@example.org> .
    ?X <http://xmlns.com/foaf/0.1/knows>/<http://xmlns.com/foaf/0.1/knows> ?Y .
    ?Y <http://xmlns.com/foaf/0.1/name> ?NAME .
    FILTER ( ?X != ?Y ) 
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("Milou@FR", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Snowy@EN", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
        }

        [Fact]
        public void InversePropertyPaths_AddSequenceStep_Inverse_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
              Inverse Property Paths: These two are the same query: the second is just reversing the property direction which swaps the roles of subject and object.

  { ?x foaf:mbox <mailto:alice@example.org> }

  { <mailto:alice@example.org> ^foaf:mbox ?x }
             */

            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var name = new RDFVariable("name");
            var x = new RDFVariable("x");

            // <mailto:alice@example.org> ^foaf:mbox ?x
            var variablePropPath1 = new RDFPropertyPath(new RDFResource("mailto:alice@example.org"), x)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.MBOX).Inverse());

            // ?x foaf:knows/foaf:name ?name
            var variablePropPath2 = new RDFPropertyPath(x, name)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.NAME));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath1)
                    .AddPropertyPath(variablePropPath2))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>

SELECT ?NAME
WHERE {
  {
    <mailto:alice@example.org> ^<http://xmlns.com/foaf/0.1/mbox> ?X .
    ?X <http://xmlns.com/foaf/0.1/knows>/<http://xmlns.com/foaf/0.1/name> ?NAME .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(3, selectQueryResult.SelectResultsCount);

            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Charlie", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Snoopy@EN", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
        }

        [Fact]
        public void InversePropertyPaths_AddSequenceStep_Inverse_Test2()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
         Inverse Path Sequence: Find all the people who know someone ?x knows.

  {
    ?x foaf:knows/^foaf:knows ?y .  
    FILTER(?x != ?y)
  }

which is equivalent to (?gen1 is a system generated variable):

  {
    ?x foaf:knows ?gen1 .
    ?y foaf:knows ?gen1 .  
    FILTER(?x != ?y)
  }
         */

            string filePath = GetPath(@"Files\Test18.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(16, graph.TriplesCount);

            var name = new RDFVariable("name");
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");

            // {FILTER ( ?X != ?Y )}
            var filter1 = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, x, y);

            // ?x foaf:knows/^foaf:knows ?y
            var variablePropPath1 = new RDFPropertyPath(x, y)
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS))
                .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.FOAF.KNOWS).Inverse());



            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath1)
                    .AddFilter(filter1))
                .AddProjectionVariable(x)
                .AddProjectionVariable(y);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>

SELECT ?X ?Y
WHERE {
  {
    ?X <http://xmlns.com/foaf/0.1/knows>/^<http://xmlns.com/foaf/0.1/knows> ?Y .
    FILTER ( ?X != ?Y ) 
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example.org/bob#me", selectQueryResult.SelectResults.Rows[0]["?X"]);
            Assert.Equal("http://example.org/charlie#me", selectQueryResult.SelectResults.Rows[0]["?Y"]);

            Assert.Equal("http://example.org/charlie#me", selectQueryResult.SelectResults.Rows[1]["?X"]);
            Assert.Equal("http://example.org/bob#me", selectQueryResult.SelectResults.Rows[1]["?Y"]);

            Assert.Equal("http://example.org/snoopy", selectQueryResult.SelectResults.Rows[2]["?X"]);
            Assert.Equal("http://example.org/tintin", selectQueryResult.SelectResults.Rows[2]["?Y"]);

            Assert.Equal("http://example.org/tintin", selectQueryResult.SelectResults.Rows[3]["?X"]);
            Assert.Equal("http://example.org/snoopy", selectQueryResult.SelectResults.Rows[3]["?Y"]);
        }

        [Fact]
        public void PropertyPath_star_plus_NotAvailable()
        {
            /*
    Arbitrary length match: Find the names of all the people that can be reached from Alice by foaf:knows:

      {
        ?x foaf:mbox <mailto:alice@example> .
        ?x foaf:knows+/foaf:name ?name .
      }
             */

            /*
             Alternatives in an arbitrary length path:

              { ?ancestor (ex:motherOf|ex:fatherOf)+ <#me> }

            Arbitrary length path match: Some forms of limited inference are possible as well. For example, for RDFS, all types and supertypes of a resource:

              { <http://example/thing> rdf:type/rdfs:subClassOf* ?type }

            All resources and all their inferred types:

              { ?x rdf:type/rdfs:subClassOf* ?type }

            Subproperty:

              { ?x ?p ?v . ?p rdfs:subPropertyOf* :property }

            Negated Property Paths: Find nodes connected but not by rdf:type (either way round):

              { ?x !(rdf:type|^rdf:type) ?y }
            */

            // PropertyPathNotAvailable : +, * or ?
            Assert.True(false);
        }

        #endregion

        #region 9.3 Property Paths and Equivalent Patterns

        [Fact]
        public void PropertyPathsAndEquivalentPatterns_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
         Query:

PREFIX :   <http://example/>
SELECT * 
{  ?s :item/:price ?x . }
            */

            string filePath = GetPath(@"Files\Test19.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var s = new RDFVariable("s");
            var x = new RDFVariable("x");

            // ?s :item/:price ?x
            var variablePropPath = new RDFPropertyPath(s, x)
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource(exNs + "item")))
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource(exNs + "price")));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath));

            #region generated sparql query
            /*
PREFIX ex: <http://example/>

SELECT *
WHERE {
  {
    ?S ex:item/ex:price ?X .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example/order", selectQueryResult.SelectResults.Rows[0]["?S"]);
            Assert.Equal("5^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0]["?X"]);

            Assert.Equal("http://example/order", selectQueryResult.SelectResults.Rows[1]["?S"]);
            Assert.Equal("5^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[1]["?X"]);

        }

        [Fact]
        public void PropertyPathsAndEquivalentPatterns_Test2()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
         PREFIX :   <http://example/>
SELECT * 
{  ?s :item ?_a .
   ?_a :price ?x . }
            */

            string filePath = GetPath(@"Files\Test19.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var s = new RDFVariable("s");
            var x = new RDFVariable("x");
            var _a = new RDFVariable("_a");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(s, new RDFResource(exNs + "item"), _a))
                    .AddPattern(new RDFPattern(_a, new RDFResource(exNs + "price"), x)));

            #region generated sparql query
            /*
PREFIX ex: <http://example/>

SELECT *
WHERE {
  {
    ?S ex:item ?_A .
    ?_A ex:price ?X .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example/order", selectQueryResult.SelectResults.Rows[0]["?S"]);
            Assert.Equal("http://example/z1", selectQueryResult.SelectResults.Rows[0]["?_A"]);
            Assert.Equal("5^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0]["?X"]);

            Assert.Equal("http://example/order", selectQueryResult.SelectResults.Rows[1]["?S"]);
            Assert.Equal("http://example/z2", selectQueryResult.SelectResults.Rows[1]["?_A"]);
            Assert.Equal("5^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[1]["?X"]);

        }

        [Fact]
        public void PropertyPathsAndEquivalentPatterns_Test3()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
         The equivalance to graphs patterns is particularly significant when query also involves an aggregation operation. The total cost of the order can be found with

  PREFIX :   <http://example/>
  SELECT (sum(?x) AS ?total)
  { 
    :order :item/:price ?x
  }
            */

            string filePath = GetPath(@"Files\Test19.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var x = new RDFVariable("x");
            var total = new RDFVariable("total");

            // :order :item/:price ?x
            var variablePropPath = new RDFPropertyPath(new RDFResource(exNs + "order"), x)
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource(exNs + "item")))
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource(exNs + "price")));

            var gm = new RDFGroupByModifier(new List<RDFVariable> { x });
            gm.AddAggregator(new RDFSumAggregator(x, total));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPropertyPath(variablePropPath))
                .AddModifier(gm)
                .AddProjectionVariable(total);

            #region generated sparql query
            /*
PREFIX ex: <http://example/>

SELECT ?X (SUM(?X) AS ?TOTAL)
WHERE {
  {
    ex:order ex:item/ex:price ?X .
  }
}
GROUP BY ?X
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // can not create a (SUM(?X) AS ?TOTAL) without the GROUP BY ?X
            Assert.True(false);
        }

        #endregion

        #region 9.4 Arbitrary Length Path Matching

        #endregion

        #endregion

        #region 10 Assignment

        #region 10.1 BIND: Assigning to Variables

        #endregion

        #region 10.2 VALUES: Providing inline data

        #region 10.2.1 VALUES syntax
        /*
            VALUES (?x ?y) {
                (:uri1 1)
                (:uri2 UNDEF)
            }

            Optionally, when there is a single variable and some values:

VALUES ?z { "abc" "def" }

which is the same as using the general form:

VALUES (?z) { ("abc") ("def") }

        */
        #endregion

        #region 10.2.2 VALUES Examples

        [Fact]
        public void VALUES_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/book/");
            RDFNamespaceRegister.AddNamespace(exNs);

            var nsNs = new RDFNamespace("ns", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test20.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            /*
PREFIX dc:   <http://purl.org/dc/elements/1.1/> 
PREFIX :     <http://example.org/book/> 
PREFIX ns:   <http://example.org/ns#> 

SELECT ?book ?title ?price
{
   VALUES ?book { :book1 :book3 }
   ?book dc:title ?title ;
         ns:price ?price .
} 
             */

            var book = new RDFVariable("book");
            var title = new RDFVariable("title");
            var price = new RDFVariable("price");

            //Declare the following SPARQL values:
            /*
            VALUES (?book) {
            (":book1" ":book3")
            }
            */

            // VALUES ?BOOK { ex:book1 ex:book3 }
            RDFValues myValues = new RDFValues()
            .AddColumn(new RDFVariable("book"), new List<RDFPatternMember>()
                {
                    new RDFResource(exNs + "book1"),
                    new RDFResource(exNs + "book3")
                });

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(
                    new RDFPatternGroup("PG1")
                        .AddPattern(new RDFPattern(book, RDFVocabulary.DC.TITLE, title))
                        .AddPattern(new RDFPattern(book, new RDFResource(nsNs + "price"), price))
                        .AddValues(myValues)
                    )
                .AddProjectionVariable(book)
                .AddProjectionVariable(title)
                .AddProjectionVariable(price);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/book/>

SELECT ?BOOK ?TITLE ?PRICE
WHERE {
  {
    ?BOOK <http://purl.org/dc/elements/1.1/title> ?TITLE .
    ?BOOK <http://example.org/ns#price> ?PRICE .
    VALUES ?BOOK { ex:book1 ex:book3 } .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example.org/book/book1", selectQueryResult.SelectResults.Rows[0]["?BOOK"]);
            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0]["?TITLE"]);
            Assert.Equal("42^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0]["?PRICE"]);
        }

        [Fact]
        public void VALUES_Test2()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/book/");
            RDFNamespaceRegister.AddNamespace(exNs);

            var nsNs = new RDFNamespace("ns", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test20.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            /*
    PREFIX dc:   <http://purl.org/dc/elements/1.1/> 
    PREFIX :     <http://example.org/book/> 
    PREFIX ns:   <http://example.org/ns#> 

    SELECT ?book ?title ?price
    {
       ?book dc:title ?title ;
             ns:price ?price .
       VALUES (?book ?title)
       { (UNDEF "SPARQL Tutorial")
         (:book2 UNDEF)
       }
    }         
             */

            var book = new RDFVariable("book");
            var title = new RDFVariable("title");
            var price = new RDFVariable("price");

            //Declare the following SPARQL values:

            /* 
             * Generated

             VALUES (?BOOK ?TITLE) {
                ( UNDEF "SPARQL Tutorial" )
                ( <http://example.org/book/book2> UNDEF )
             }
             */
            RDFValues myValues = new RDFValues()
            .AddColumn(new RDFVariable("book"), new List<RDFPatternMember>()
                {
                    null, //UNDEF
                    new RDFResource(exNs + "book2")

                })
             .AddColumn(new RDFVariable("title"),
                new List<RDFPatternMember>()
                {
                    new RDFPlainLiteral("SPARQL Tutorial"),
                    null //UNDEF
                });

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(
                    new RDFPatternGroup("PG1")
                        .AddPattern(new RDFPattern(book, RDFVocabulary.DC.TITLE, title))
                        .AddPattern(new RDFPattern(book, new RDFResource(nsNs + "price"), price))
                        .AddValues(myValues)
                    )
                .AddProjectionVariable(book)
                .AddProjectionVariable(title)
                .AddProjectionVariable(price);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/book/>

SELECT ?BOOK ?TITLE ?PRICE
WHERE {
  {
    ?BOOK <http://purl.org/dc/elements/1.1/title> ?TITLE .
    ?BOOK <http://example.org/ns#price> ?PRICE .
    VALUES (?BOOK ?TITLE) {
      ( UNDEF "SPARQL Tutorial" )
      ( ex:book2 UNDEF )
    } .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://example.org/book/book1", selectQueryResult.SelectResults.Rows[0]["?BOOK"]);
            Assert.Equal("SPARQL Tutorial", selectQueryResult.SelectResults.Rows[0]["?TITLE"]);
            Assert.Equal("42^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[0]["?PRICE"]);

            Assert.Equal("http://example.org/book/book2", selectQueryResult.SelectResults.Rows[1]["?BOOK"]);
            Assert.Equal("The Semantic Web", selectQueryResult.SelectResults.Rows[1]["?TITLE"]);
            Assert.Equal("23^^http://www.w3.org/2001/XMLSchema#integer", selectQueryResult.SelectResults.Rows[1]["?PRICE"]);

            /*
             book	title	price
<http://example.org/book/book1>	"SPARQL Tutorial"	42
<http://example.org/book/book2>	"The Semantic Web"	23
             */
        }

        #endregion

        #endregion

        #endregion

        #region 11 Aggregates

        #region 11.1 Aggregate Example

        [Fact]
        public void Aggregate_Example()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://books.example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
         PREFIX : <http://books.example/>
SELECT (SUM(?lprice) AS ?totalPrice)
WHERE {
  ?org :affiliates ?auth .
  ?auth :writesBook ?book .
  ?book :price ?lprice .
}
GROUP BY ?org
HAVING (SUM(?lprice) > 10)
            */

            string filePath = GetPath(@"Files\Test21.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var lprice = new RDFVariable("lprice");
            var totalPrice = new RDFVariable("totalPrice");
            var org = new RDFVariable("org");
            var auth = new RDFVariable("auth");
            var book = new RDFVariable("book");

            var gm = new RDFGroupByModifier(new List<RDFVariable> { org });
            gm.AddAggregator(new RDFSumAggregator(lprice, totalPrice)
                .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("10", RDFModelEnums.RDFDatatypes.XSD_INTEGER)
            ));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(org, new RDFResource(exNs + "affiliates"), auth))
                    .AddPattern(new RDFPattern(auth, new RDFResource(exNs + "writesBook"), book))
                    .AddPattern(new RDFPattern(book, new RDFResource(exNs + "price"), lprice))
                    )
                .AddModifier(gm)
                .AddProjectionVariable(totalPrice);

            #region generated sparql query
            /*
PREFIX ex: <http://books.example/>

SELECT ?ORG (SUM(?LPRICE) AS ?TOTALPRICE)
WHERE {
  {
    ?ORG ex:affiliates ?AUTH .
    ?AUTH ex:writesBook ?BOOK .
    ?BOOK ex:price ?LPRICE .
  }
}
GROUP BY ?ORG
HAVING ((SUM(?LPRICE) > "10"^^<http://www.w3.org/2001/XMLSchema#integer>))
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("21^^http://www.w3.org/2001/XMLSchema#double", selectQueryResult.SelectResults.Rows[0]["?TOTALPRICE"]);
        }

        #endregion

        #region 11.2 GROUP BY

        [Fact]
        public void Aggregate_GROUP_BY()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test22.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(8, graph.TriplesCount);

            /*
        SELECT (AVG(?y) AS ?avg)
WHERE {
 ?a :x ?x ;
    :y ?y .
}
GROUP BY ?x
        */

            var a = new RDFVariable("a");
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");
            var avg = new RDFVariable("avg");

            var gm = new RDFGroupByModifier(new List<RDFVariable> { x });
            gm.AddAggregator(new RDFAvgAggregator(y, avg));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(a, new RDFResource(exNs + "x"), x))
                    .AddPattern(new RDFPattern(a, new RDFResource(exNs + "y"), y))
                    )
                .AddModifier(gm)
                .AddProjectionVariable(x)
                .AddProjectionVariable(avg);

            #region generated sparql query
            /*
PREFIX ex: <http://example/>

SELECT ?X (AVG(?Y) AS ?AVG)
WHERE {
  {
    ?A ex:x ?X .
    ?A ex:y ?Y .
  }
}
GROUP BY ?X
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("1.5^^http://www.w3.org/2001/XMLSchema#double", selectQueryResult.SelectResults.Rows[0]["?AVG"]);
            Assert.Equal("8.5^^http://www.w3.org/2001/XMLSchema#double", selectQueryResult.SelectResults.Rows[1]["?AVG"]);
        }

        #endregion

        #region 11.3 HAVING

        [Fact]
        void Aggregate_HAVING()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://data.example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
PREFIX : <http://data.example/>
SELECT (AVG(?size) AS ?asize)
WHERE {
  ?x :size ?size
}
GROUP BY ?x
HAVING(AVG(?size) > 10)
             */

            string filePath = GetPath(@"Files\Test23.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            var size = new RDFVariable("size");
            var asize = new RDFVariable("asize");
            var x = new RDFVariable("x");

            var gm = new RDFGroupByModifier(new List<RDFVariable> { x });
            gm.AddAggregator(new RDFAvgAggregator(size, asize)
                .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("10", RDFModelEnums.RDFDatatypes.XSD_INTEGER)
            ));

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, new RDFResource(exNs + "size"), size))
                    )
                .AddModifier(gm)
                .AddProjectionVariable(x)
                .AddProjectionVariable(asize);

            #region generated sparql query
            /*
PREFIX ex: <http://data.example/>

SELECT ?X (AVG(?SIZE) AS ?ASIZE)
WHERE {
  {
    ?X ex:size ?SIZE .
  }
}
GROUP BY ?X
HAVING ((AVG(?SIZE) > "10"^^<http://www.w3.org/2001/XMLSchema#integer>))
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("http://data.example/object2", selectQueryResult.SelectResults.Rows[0]["?X"]);
            Assert.Equal("15^^http://www.w3.org/2001/XMLSchema#double", selectQueryResult.SelectResults.Rows[0]["?ASIZE"]);
        }

        #endregion

        #region 11.4 Aggregate Projection Restrictions

        // Not possible to build : MIN(?y) * 2 AS ?min

        #endregion

        #region 11.5 Aggregate Example (with errors)

        // Not possible to build : ((MIN(?p) + MAX(?p)) / 2 AS ?c)

        #endregion

        #endregion

        #region 12 Subqueries

        [Fact]
        void Subqueries()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://people.example/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            /*
PREFIX : <http://people.example/>
SELECT ?y ?minName
WHERE {
  :alice :knows ?y .
  {
    SELECT ?y (MIN(?name) AS ?minName)
    WHERE {
      ?y :name ?name .
    } GROUP BY ?y
  }
}             
             */

            string filePath = GetPath(@"Files\Test24.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);


            var y = new RDFVariable("y");
            var name = new RDFVariable("name");
            var minName = new RDFVariable("minName");

            var gm = new RDFGroupByModifier(new List<RDFVariable> { y });
            gm.AddAggregator(new RDFMinAggregator(name, minName, RDFQueryEnums.RDFMinMaxAggregatorFlavors.String));

            // Select query
            RDFSelectQuery subquery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(y, new RDFResource(exNs + "name"), name))
                    )
                .AddModifier(gm);

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFResource(exNs + "alice"), new RDFResource(exNs + "knows"), y))
                )
                .AddSubQuery(subquery)
                .AddProjectionVariable(y)
                .AddProjectionVariable(minName);

            #region generated sparql query
            /*
PREFIX ex: <http://people.example/>

SELECT ?Y ?MINNAME
WHERE {
  {
    ex:alice ex:knows ?Y .
  }
  {
    SELECT ?Y (MIN(?NAME) AS ?MINNAME)
    WHERE {
      {
        ?Y ex:name ?NAME .
      }
    }
    GROUP BY ?Y
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            /*
y	    minName
:bob	"B. Bar"
:carol	"C. Baz"
             */

            Assert.Equal("http://people.example/bob", selectQueryResult.SelectResults.Rows[0]["?Y"]);
            Assert.Equal("B. Bar", selectQueryResult.SelectResults.Rows[0]["?MINNAME"]);

            Assert.Equal("http://people.example/carol", selectQueryResult.SelectResults.Rows[1]["?Y"]);
            Assert.Equal("C. Baz", selectQueryResult.SelectResults.Rows[1]["?MINNAME"]);
        }

        #endregion

        #region 13 RDF Dataset

        #region 13.1 Examples of RDF Datasets
        #endregion

        #region 13.2 Specifying RDF Datasets

        // not possible to build: FROM    <http://example.org/foaf/aliceFoaf>
        // not possible to build: FROM NAMED <http://example.org/alice>
        // Not possible to build : GRAPH ?g { ?x foaf:mbox ?mbox }

        #endregion

        #region 13.3 Querying the Dataset
        #endregion

        #endregion

        #region 14 Basic Federated Query
        #endregion

        #region 15 Solution Sequences and Modifiers

        #region 15.1 ORDER BY

        [Fact]
        void ORDER_BY_ASC_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
    PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

    SELECT ?name
    WHERE { ?x foaf:name ?name }
    ORDER BY ?name
             */

            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddModifier(new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.ASC))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
ORDER BY ASC(?NAME)
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Charlie", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
            Assert.Equal("Snoopy@EN", selectQueryResult.SelectResults.Rows[3]["?NAME"]);

        }

        [Fact]
        void ORDER_BY_DESC_Test1()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
    PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

    SELECT ?name
    WHERE { ?x foaf:name ?name }
    ORDER BY DESC(?name)
             */

            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddModifier(new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
ORDER BY DESC(?NAME)
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("Snoopy@EN", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Charlie", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[3]["?NAME"]);
        }

        [Fact]
        void ORDER_BY_DESC_Test2()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
    PREFIX     :    <http://example.org/ns#>
    PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

    SELECT ?name
    WHERE { ?x foaf:name ?name ; :empId ?emp }
    ORDER BY DESC(?emp)
            */

            string filePath = GetPath(@"Files\Test25.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(8, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");
            var emp = new RDFVariable("emp");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                    .AddPattern(new RDFPattern(x, new RDFResource(exNs + "empId"), emp))
                )
                .AddModifier(new RDFOrderByModifier(emp, RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/ns#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
    ?X ex:empId ?EMP .
  }
}
ORDER BY DESC(?EMP)
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Charles", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[3]["?NAME"]);
        }


        [Fact]
        void ORDER_BY_Test3()
        {
            // CREATE NAMESPACE
            var exNs = new RDFNamespace("ex", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
    PREFIX     :    <http://example.org/ns#>
    PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

    SELECT ?name
    WHERE { ?x foaf:name ?name ; :empId ?emp }
    ORDER BY ?name DESC(?emp)

            */

            string filePath = GetPath(@"Files\Test25.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(8, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");
            var emp = new RDFVariable("emp");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(exNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                    .AddPattern(new RDFPattern(x, new RDFResource(exNs + "empId"), emp))
                )
                .AddModifier(new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.ASC))
                .AddModifier(new RDFOrderByModifier(emp, RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX ex: <http://example.org/ns#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
    ?X ex:empId ?EMP .
  }
}
ORDER BY ASC(?NAME) DESC(?EMP)
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(4, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
            Assert.Equal("Charles", selectQueryResult.SelectResults.Rows[3]["?NAME"]);
        }

        #endregion

        #region 15.2 Projection

        [Fact]
        void Projection_Test1()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
    PREFIX foaf:       <http://xmlns.com/foaf/0.1/>
SELECT ?name
WHERE
 { ?x foaf:name ?name }

            */

            string filePath = GetPath(@"Files\Test26.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Bob", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
        }

        #endregion

        #region 15.3 Duplicate Solutions

        [Fact]
        void Projection_Test2()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
SELECT ?name WHERE { ?x foaf:name ?name }

            */

            string filePath = GetPath(@"Files\Test27.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
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
            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[2]["?NAME"]);
        }

        #region 15.3.1 DISTINCT

        [Fact]
        void Projection_DISTINCT()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
SELECT DISTINCT ?name WHERE { ?x foaf:name ?name }
            */

            string filePath = GetPath(@"Files\Test27.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddModifier(new RDFDistinctModifier())
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT DISTINCT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(1, selectQueryResult.SelectResultsCount);

            Assert.Equal("Alice", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
        }

        #endregion

        #region 15.3.2 REDUCED

        // Not available

        #endregion

        #endregion

        #region 15.4 OFFSET

        [Fact]
        void Projection_OFFSET_LIMIT()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Using LIMIT and OFFSET to select different subsets of the query solutions will not be useful unless the order is made predictable by using ORDER BY.

            /*
    PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

    SELECT  ?name
    WHERE   { ?x foaf:name ?name }
    ORDER BY ?name
    LIMIT   2
    OFFSET  1
             */

            string filePath = GetPath(@"Files\Test28.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(10, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddModifier(new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.ASC))
                .AddModifier(new RDFLimitModifier(2))
                .AddModifier(new RDFOffsetModifier(1))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
ORDER BY ASC(?NAME)
LIMIT 2
OFFSET 1
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);

            Assert.Equal("Bernard", selectQueryResult.SelectResults.Rows[0]["?NAME"]);
            Assert.Equal("Charlie", selectQueryResult.SelectResults.Rows[1]["?NAME"]);
        }

        #endregion

        #region 15.5 LIMIT

        [Fact]
        void Projection_LIMIT()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            // Using LIMIT and OFFSET to select different subsets of the query solutions will not be useful unless the order is made predictable by using ORDER BY.

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>

SELECT ?name
WHERE { ?x foaf:name ?name }
LIMIT 20
             */

            string filePath = GetPath(@"Files\Test28.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(10, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddModifier(new RDFLimitModifier(2))
                .AddProjectionVariable(name);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}
LIMIT 2
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);
        }

        #endregion

        #endregion

        #region 16 Query Forms

        #region 16.1 SELECT

        #region 16.1.1 Projection

        [Fact]
        void SELECT_Query()
        {
            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
SELECT ?nameX ?nameY ?nickY
WHERE
  { ?x foaf:knows ?y ;
       foaf:name ?nameX .
    ?y foaf:name ?nameY .
    OPTIONAL { ?y foaf:nick ?nickY }
  }
             */

            string filePath = GetPath(@"Files\Test29.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var nameX = new RDFVariable("nameX");
            var nameY = new RDFVariable("nameY");
            var nickY = new RDFVariable("nickY");
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");

            // Select query
            RDFSelectQuery selectQuery = new RDFSelectQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.KNOWS, y))
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, nameX))
                    .AddPattern(new RDFPattern(y, RDFVocabulary.FOAF.NAME, nameY))
                    .AddPattern(new RDFPattern(y, RDFVocabulary.FOAF.NICK, nickY).Optional())
                )
                .AddProjectionVariable(nameX)
                .AddProjectionVariable(nameY)
                .AddProjectionVariable(nickY);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAMEX ?NAMEY ?NICKY
WHERE {
  {
    ?X foaf:knows ?Y .
    ?X foaf:name ?NAMEX .
    ?Y foaf:name ?NAMEY .
    OPTIONAL { ?Y foaf:nick ?NICKY } .
  }
}
            */
            string sparqlCommand = selectQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            Assert.Equal(2, selectQueryResult.SelectResultsCount);
        }

        #endregion

        #region 16.1.2 SELECT Expressions

        // Not possible

        #endregion

        #endregion

        #region 16.2 CONSTRUCT

        [Fact]
        void CONSTRUCT_Query_Test1()
        {
            var vcardNs = new RDFNamespace("vcard", "http://www.w3.org/2001/vcard-rdf/3.0#");
            RDFNamespaceRegister.AddNamespace(vcardNs);

            //RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
PREFIX vcard:   <http://www.w3.org/2001/vcard-rdf/3.0#>
CONSTRUCT   { <http://example.org/person#Alice> vcard:FN ?name }
WHERE       { ?x foaf:name ?name }
             */

            string filePath = GetPath(@"Files\Test30.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(2, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Select query
            RDFConstructQuery constructQuery = new RDFConstructQuery()
                .AddPrefix(vcardNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, name))
                )
                .AddTemplate(new RDFPattern(new RDFResource("http://example.org/person#Alice"), new RDFResource(vcardNs + "FN"), name));

            #region generated sparql query
            /*
PREFIX vcard: <http://www.w3.org/2001/vcard-rdf/3.0#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

CONSTRUCT
{
  <http://example.org/person#Alice> vcard:FN ?NAME .
}
WHERE {
  {
    ?X foaf:name ?NAME .
  }
}             
             */
            string sparqlCommand = constructQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFConstructQueryResult constructQueryResult = constructQuery.ApplyToGraph(graph);

            Assert.Equal(1, constructQueryResult.ConstructResultsCount);

            /* <http://example.org/person#Alice> vcard:FN "Alice" */
            Assert.Equal("http://example.org/person#Alice", constructQueryResult.ConstructResults.Rows[0][0]);
            Assert.Equal("http://www.w3.org/2001/vcard-rdf/3.0#FN", constructQueryResult.ConstructResults.Rows[0][1]);
            Assert.Equal("Alice", constructQueryResult.ConstructResults.Rows[0][2]);
        }

        #region 16.2.1 Templates with Blank Nodes

        [Fact]
        void CONSTRUCT_Query_Test2()
        {
            var vcardNs = new RDFNamespace("vcard", "http://www.w3.org/2001/vcard-rdf/3.0#");
            RDFNamespaceRegister.AddNamespace(vcardNs);

            //RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
PREFIX vcard:   <http://www.w3.org/2001/vcard-rdf/3.0#>

CONSTRUCT { ?x  vcard:N _:v .
            _:v vcard:givenName ?gname .
            _:v vcard:familyName ?fname }
WHERE
 {
    { ?x foaf:firstname ?gname } UNION  { ?x foaf:givenname   ?gname } .
    { ?x foaf:surname   ?fname } UNION  { ?x foaf:family_name ?fname } .
 }
             */

            string filePath = GetPath(@"Files\Test31.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            var x = new RDFVariable("x");
            var gname = new RDFVariable("gname");
            var fname = new RDFVariable("fname");

            // Select query
            RDFConstructQuery constructQuery = new RDFConstructQuery()
                .AddPrefix(vcardNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.FIRSTNAME, gname).UnionWithNext())
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.GIVEN_NAME, gname))
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.SURNAME, fname).UnionWithNext())
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.FAMILY_NAME, fname))
                )
                .AddTemplate(new RDFPattern(x, new RDFResource(vcardNs + "N"), new RDFResource("_:v")))
                .AddTemplate(new RDFPattern(new RDFResource("_:v"), new RDFResource(vcardNs + "givenName"), gname))
                .AddTemplate(new RDFPattern(new RDFResource("_:v"), new RDFResource(vcardNs + "familyName"), fname));

            #region generated sparql query
            /*
PREFIX vcard: <http://www.w3.org/2001/vcard-rdf/3.0#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

CONSTRUCT
{
  ?X vcard:N _:v .
  _:v vcard:givenName ?GNAME .
  _:v vcard:familyName ?FNAME .
}
WHERE {
  {
    { ?X foaf:firstName ?GNAME }
    UNION
    { ?X foaf:givenName ?GNAME }
  }
  {
    { ?X foaf:surname ?FNAME }
    UNION
    { ?X foaf:familyName ?FNAME }
  }
}        
             */
            string sparqlCommand = constructQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFConstructQueryResult constructQueryResult = constructQuery.ApplyToGraph(graph);

            /* 
@prefix vcard: <http://www.w3.org/2001/vcard-rdf/3.0#> .

_:v1 vcard:N         _:x .
_:x vcard:givenName  "Alice" .
_:x vcard:familyName "Hacker" .

_:v2 vcard:N         _:z .
_:z vcard:givenName  "Bob" .
_:z vcard:familyName "Hacker" .
*/

            Assert.Equal(6, constructQueryResult.ConstructResultsCount);
        }

        #endregion

        #region 16.2.2 Accessing Graphs in the RDF Dataset
        // Not possible to build such query
        #endregion

        #region 16.2.3 Solution Modifiers and CONSTRUCT

        [Fact]
        void CONSTRUCT_Query_Test3()
        {
            var siteNs = new RDFNamespace("site", "http://example.org/stats#");
            RDFNamespaceRegister.AddNamespace(siteNs);

            //RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
PREFIX site: <http://example.org/stats#>

CONSTRUCT { [] foaf:name ?name }
WHERE
{ [] foaf:name ?name ;
     site:hits ?hits .
}
ORDER BY desc(?hits)
LIMIT 2
             */

            string filePath = GetPath(@"Files\Test32.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            var hits = new RDFVariable("hits");
            var name = new RDFVariable("name");

            var t = new RDFOrderByModifier(name, RDFQueryEnums.RDFOrderByFlavors.ASC);

            // Select query
            RDFConstructQuery constructQuery = new RDFConstructQuery()
                .AddPrefix(siteNs)
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFResource("_:"), RDFVocabulary.FOAF.NAME, name))
                    .AddPattern(new RDFPattern(new RDFResource("_:"), new RDFResource(siteNs + "hits"), hits))
                )
                // Not possible
                //.AddModifier(t);
                .AddModifier(new RDFLimitModifier(2))
                .AddTemplate(new RDFPattern(new RDFResource("_:"), RDFVocabulary.FOAF.NAME, name));

            #region generated sparql query
            /*
PREFIX site: <http://example.org/stats#>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

CONSTRUCT
{
  _: foaf:name ?NAME .
}
WHERE {
  {
    _: foaf:name ?NAME .
    _: site:hits ?HITS .
  }
}
LIMIT 2        
             */
            string sparqlCommand = constructQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFConstructQueryResult constructQueryResult = constructQuery.ApplyToGraph(graph);

            /* 
@prefix foaf: <http://xmlns.com/foaf/0.1/> .
_:x foaf:name "Alice" .
_:y foaf:name "Eve" .
*/

            Assert.Equal(2, constructQueryResult.ConstructResultsCount);
        }

        #endregion

        #endregion

        #region 16.3 ASK

        [Fact]
        void ASK_Test1()
        {
            var vcardNs = new RDFNamespace("vcard", "http://www.w3.org/2001/vcard-rdf/3.0#");
            RDFNamespaceRegister.AddNamespace(vcardNs);

            //RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
ASK  { ?x foaf:name  "Alice" }
             */

            string filePath = GetPath(@"Files\Test33.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Ask query
            RDFAskQuery askQuery = new RDFAskQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")))
                );

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

ASK
WHERE {
  {
    ?X foaf:name "Alice" .
  }
}          
             */
            string sparqlCommand = askQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFAskQueryResult askQueryResult = askQuery.ApplyToGraph(graph);

            Assert.True(askQueryResult.AskResult);
        }

        [Fact]
        void ASK_Test2()
        {
            var vcardNs = new RDFNamespace("vcard", "http://www.w3.org/2001/vcard-rdf/3.0#");
            RDFNamespaceRegister.AddNamespace(vcardNs);

            //RDFNamespaceRegister.SetDefaultNamespace(exNs);

            var foafNs = RDFNamespaceRegister.GetByPrefix("foaf");

            /*
PREFIX foaf:    <http://xmlns.com/foaf/0.1/>
ASK  { ?x foaf:name  "Alice" ;
          foaf:mbox  <mailto:alice@work.example> }
            */

            string filePath = GetPath(@"Files\Test33.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(4, graph.TriplesCount);

            var x = new RDFVariable("x");
            var name = new RDFVariable("name");

            // Ask query
            RDFAskQuery askQuery = new RDFAskQuery()
                .AddPrefix(foafNs)
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")))
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, new RDFResource("mailto:alice@work.example")))
                );

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

ASK
WHERE {
  {
    ?X foaf:name "Alice" .
    ?X foaf:mbox <mailto:alice@work.example> .
  }
}       
             */
            string sparqlCommand = askQuery.ToString();
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFAskQueryResult askQueryResult = askQuery.ApplyToGraph(graph);

            Assert.False(askQueryResult.AskResult);
        }


        #endregion

        #region 16.4 DESCRIBE (Informative)

        #region 16.4.1 Explicit IRIs

        [Fact]
        void DESCRIBE_Test1()
        {
            var exNs = new RDFNamespace("vcard", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test25.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(8, graph.TriplesCount);

            /*
             DESCRIBE <http://example.org/>
             */

            RDFDescribeQuery describeQuery = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("http://example.org/ns#"));

            #region generated sparql query
            /*
DESCRIBE <http://example.org/ns#>
WHERE {
}      
             */
            string sparqlCommand = describeQuery.ToString();
            #endregion

            RDFDescribeQueryResult describeResult = describeQuery.ApplyToGraph(graph);

            // no result
        }

        #endregion

        #region 16.4.2 Identifying Resources

        [Fact]
        void DESCRIBE_Test2()
        {
            var exNs = new RDFNamespace("vcard", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test30.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(2, graph.TriplesCount);

            /*
PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
DESCRIBE ?x
WHERE    { ?x foaf:mbox <mailto:alice@example.org> }
             */

            var x = new RDFVariable("x");

            RDFDescribeQuery describeQuery = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.MBOX, new RDFResource("mailto:alice@example.org")))
                )
                .AddDescribeTerm(x);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

DESCRIBE ?X
WHERE {
  {
    ?X foaf:mbox <mailto:alice@example.org> .
  }
} 
             */
            string sparqlCommand = describeQuery.ToString();
            #endregion

            RDFDescribeQueryResult describeResult = describeQuery.ApplyToGraph(graph);

            Assert.Equal(2, describeResult.DescribeResultsCount);
        }

        [Fact]
        void DESCRIBE_Test3()
        {
            var exNs = new RDFNamespace("vcard", "http://example.org/ns#");
            RDFNamespaceRegister.AddNamespace(exNs);

            RDFNamespaceRegister.SetDefaultNamespace(exNs);

            string filePath = GetPath(@"Files\Test27.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(6, graph.TriplesCount);

            /*
    PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
    DESCRIBE ?x
    WHERE    { ?x foaf:name "Alice" }         
             */

            var x = new RDFVariable("x");

            RDFDescribeQuery describeQuery = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.NAME, new RDFPlainLiteral("Alice")))
                )
                .AddDescribeTerm(x);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

DESCRIBE ?X
WHERE {
  {
    ?X foaf:name "Alice" .
  }
}
             */
            string sparqlCommand = describeQuery.ToString();
            #endregion

            RDFDescribeQueryResult describeResult = describeQuery.ApplyToGraph(graph);

            Assert.Equal(6, describeResult.DescribeResultsCount);
        }

        [Fact]
        void DESCRIBE_Test4()
        {
            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);

            /*
    PREFIX foaf:   <http://xmlns.com/foaf/0.1/>
    DESCRIBE ?x ?y <http://example.org/>
    WHERE    {?x foaf:knows ?y}         
             */

            var x = new RDFVariable("x");
            var y = new RDFVariable("y");

            RDFDescribeQuery describeQuery = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.KNOWS, y))
                )
                .AddDescribeTerm(x)
                .AddDescribeTerm(y);

            #region generated sparql query
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

DESCRIBE ?X ?Y
WHERE {
  {
    ?X foaf:knows ?Y .
  }
} 
             */
            string sparqlCommand = describeQuery.ToString();
            #endregion

            RDFDescribeQueryResult describeResult = describeQuery.ApplyToGraph(graph);

            Assert.Equal(54, describeResult.DescribeResultsCount);
        }


        #endregion

        #region 16.4.3 Descriptions of Resources
        #endregion

        #endregion

        #endregion

        #region 17 Expressions and Testing Values
        #endregion

        #region 19 SPARQL Grammar
        #endregion
    }
}
