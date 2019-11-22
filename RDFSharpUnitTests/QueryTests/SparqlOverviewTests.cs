using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

// Try to reproduce the example in the document : https://www.w3.org/TR/sparql11-overview/

namespace RDFSharpTests.QueryTests
{
    public class SparqlOverviewTests
    {
        private string GetPath(string relativePath)
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            return Path.Combine(dirPath, relativePath);
        }


        /// <summary>
        /// https://www.w3.org/TR/sparql11-overview/
        /// 1 Introduction
        /// 1.1 Example
        /// </summary>
        [Fact]
        public void RDFGraphFromFileTest()
        {
            // The example data is in file "Test1.ttl"
            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile( RDFModelEnums.RDFFormats.Turtle, filePath);

            Assert.Equal(11, graph.TriplesCount);
        }

        /// <summary>
        /// https://www.w3.org/TR/sparql11-overview/
        /// 2 SPARQL 1.1 Query Language
        /// </summary>
        [Fact]
        public void Test1()
        {
            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            RDFSelectQuery selectQuery = new RDFSelectQuery();

            // PREFIX foaf: <http://xmlns.com/foaf/0.1/>
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"));

            // ?NAME
            var name = new RDFVariable("name");
            // ?FRIEND
            var friend = new RDFVariable("friend");
            // ?PERSON
            var person = new RDFVariable("person");

            // ?PERSON foaf:name ?NAME .
            var person_foaf_name_name = new RDFPattern(person, RDFVocabulary.FOAF.NAME, name);
            // ?PERSON foaf:knows ?FRIEND .
            var person_foaf_knows_friend = new RDFPattern(person, RDFVocabulary.FOAF.KNOWS, friend);

// WHERE {
//  {
//    ?PERSON foaf:name? NAME .
//    ?PERSON foaf:knows? FRIEND .
//  }
// }
            var pg1 = new RDFPatternGroup("PG1");
            pg1.AddPattern(person_foaf_name_name);
            pg1.AddPattern(person_foaf_knows_friend);

            selectQuery.AddPatternGroup(pg1);

            // SELECT ?PERSON ?NAME ?FRIEND
            selectQuery.AddProjectionVariable(person);
            selectQuery.AddProjectionVariable(name);
            selectQuery.AddProjectionVariable(friend);

            var sparqlCommand = selectQuery.ToString();

            #region Generated SPARQL command
            /*
             * Generates this sparql command

PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?PERSON ?NAME ?FRIEND
WHERE {
  {
    ?PERSON foaf:name ?NAME .
    ?PERSON foaf:knows ?FRIEND .
  }
}

*/
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\Test1.srq");

            #region Generated result file
            /*
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?PERSON" />
                <variable name="?NAME" />
                <variable name="?FRIEND" />
              </head>
              <results>
                <result>
                  <binding name="?PERSON">
                    <uri>http://example.org/alice#me</uri>
                  </binding>
                  <binding name="?NAME">
                    <literal>Alice</literal>
                  </binding>
                  <binding name="?FRIEND">
                    <uri>http://example.org/bob#me</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?PERSON">
                    <uri>http://example.org/alice#me</uri>
                  </binding>
                  <binding name="?NAME">
                    <literal>Alice</literal>
                  </binding>
                  <binding name="?FRIEND">
                    <uri>http://example.org/charlie#me</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?PERSON">
                    <uri>http://example.org/alice#me</uri>
                  </binding>
                  <binding name="?NAME">
                    <literal>Alice</literal>
                  </binding>
                  <binding name="?FRIEND">
                    <uri>http://example.org/snoopy</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?PERSON">
                    <uri>http://example.org/bob#me</uri>
                  </binding>
                  <binding name="?NAME">
                    <literal>Bob</literal>
                  </binding>
                  <binding name="?FRIEND">
                    <uri>http://example.org/alice#me</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?PERSON">
                    <uri>http://example.org/charlie#me</uri>
                  </binding>
                  <binding name="?NAME">
                    <literal>Charlie</literal>
                  </binding>
                  <binding name="?FRIEND">
                    <uri>http://example.org/alice#me</uri>
                  </binding>
                </result>
              </results>
            </sparql> 
            */
            #endregion

            Assert.Equal(5, selectQueryResult.SelectResultsCount);
        }

        [Fact]
        public void Test2()
        {
            string filePath = GetPath(@"Files\Test1.ttl");
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, filePath);

            RDFSelectQuery selectQuery = new RDFSelectQuery();
            selectQuery.AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"));

            var name = new RDFVariable("name");
            var friend = new RDFVariable("friend");
            var count = new RDFVariable("count");
            var person = new RDFVariable("person");

            // ?person foaf:name ?name .
            var person_foaf_name_name = new RDFPattern(person, RDFVocabulary.FOAF.NAME, name);
            // ?person foaf:knows ?friend . 
            var person_foaf_knows_friend = new RDFPattern(person, RDFVocabulary.FOAF.KNOWS, friend);

            var pg1 = new RDFPatternGroup("PG1");
            pg1.AddPattern(person_foaf_name_name);
            pg1.AddPattern(person_foaf_knows_friend);

            selectQuery.AddPatternGroup(pg1);


            selectQuery.AddProjectionVariable(name);
            //selectQuery.AddProjectionVariable(count);

            // GROUP BY ?PERSON ?NAME
            // var gm = new RDFGroupByModifier(new List<RDFVariable>() { person, name });
            // GROUP BY ?NAME
            var gm = new RDFGroupByModifier(new List<RDFVariable>() { name });
            // (COUNT(?friend) AS ?count)
            gm.AddAggregator(new RDFCountAggregator(friend, count));

            selectQuery.AddModifier(gm);


            var sparqlCommand = selectQuery.ToString();

            #region Target query in the SPARQL 1.1 documentation
            /*
             * Target query in the document
             * 
          PREFIX foaf: <http://xmlns.com/foaf/0.1/>
          SELECT ?name (COUNT(?friend) AS ?count)
          WHERE { 
              ?person foaf:name ?name . 
              ?person foaf:knows ?friend . 
          } GROUP BY ?person ?name
          */
            #endregion

            #region Managed to build this command
            /*
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?NAME (COUNT(?FRIEND) AS ?COUNT)
WHERE {
  {
    ?PERSON foaf:name ?NAME .
    ?PERSON foaf:knows ?FRIEND .
  }
}
GROUP BY ?NAME             
             */
            #endregion

            #region Managed to build this command
            /*
             * Managed to build this command
             
PREFIX foaf: <http://xmlns.com/foaf/0.1/>

SELECT ?PERSON ?NAME (COUNT(?FRIEND) AS ?COUNT)
WHERE {
  {
    ?PERSON foaf:name ?NAME .
    ?PERSON foaf:knows ?FRIEND .
  }
}
GROUP BY ?PERSON ?NAME
             */
            #endregion

            // APPLY SELECT QUERY TO GRAPH
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToGraph(graph);

            // EXPORT SELECT QUERY RESULTS TO SPARQL XML FORMAT (FILE)
            //selectQueryResult.ToSparqlXmlResult(@"C:\TEMP\Test2.srq");

            #region Got result
            /*
             * Got result
              
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="?NAME" />
    <variable name="?COUNT" />
  </head>
  <results>
    <result>
      <binding name="?NAME">
        <literal>Alice</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">3</literal>
      </binding>
    </result>
    <result>
      <binding name="?NAME">
        <literal>Bob</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">1</literal>
      </binding>
    </result>
    <result>
      <binding name="?NAME">
        <literal>Charlie</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">1</literal>
      </binding>
    </result>
  </results>
</sparql>             
 
             */
            #endregion

            #region Got result
            /*
             *  
             
<?xml version="1.0" encoding="utf-8"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
  <head>
    <variable name="?PERSON" />
    <variable name="?NAME" />
    <variable name="?COUNT" />
  </head>
  <results>
    <result>
      <binding name="?PERSON">
        <uri>http://example.org/alice#me</uri>
      </binding>
      <binding name="?NAME">
        <literal>Alice</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">3</literal>
      </binding>
    </result>
    <result>
      <binding name="?PERSON">
        <uri>http://example.org/bob#me</uri>
      </binding>
      <binding name="?NAME">
        <literal>Bob</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">1</literal>
      </binding>
    </result>
    <result>
      <binding name="?PERSON">
        <uri>http://example.org/charlie#me</uri>
      </binding>
      <binding name="?NAME">
        <literal>Charlie</literal>
      </binding>
      <binding name="?COUNT">
        <literal datatype="http://www.w3.org/2001/XMLSchema#decimal">1</literal>
      </binding>
    </result>
  </results>
</sparql>             

             */
            #endregion

            #region Target result from SPARQL 1.1 documentation
            /*
             * Target result
             * 3 Different query results formats supported by SPARQL 1.1 (XML, JSON, CSV, TSV)
             
<?xml version="1.0"?>
<sparql xmlns="http://www.w3.org/2005/sparql-results#">
 <head>
   <variable name="name"/>
   <variable name="count"/>
 </head>
 <results>
   <result>
     <binding name="name">
       <literal>Alice</literal>
     </binding>
     <binding name="count">
       <literal datatype="http://www.w3.org/2001/XMLSchema#integer">3</literal>
     </binding>
   </result>
   <result>
     <binding name="name">
       <literal>Bob</literal>
     </binding>
     <binding name="count">
       <literal datatype="http://www.w3.org/2001/XMLSchema#integer">1</literal>
     </binding>
   </result>
   <result>
     <binding name="name">
       <literal>Charlie</literal>
     </binding>
     <binding name="count">
       <literal datatype="http://www.w3.org/2001/XMLSchema#integer">1</literal>
     </binding>
   </result>
 </results>
</sparql>             
             */
            #endregion

            Assert.Equal(3, selectQueryResult.SelectResultsCount);
        }

    }
}
