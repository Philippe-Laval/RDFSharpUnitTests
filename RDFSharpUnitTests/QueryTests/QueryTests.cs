using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharpTests.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RDFSharpTests.QueryTests
{
    //Contains 27 tests
    public class QueryTests
    {
        private readonly RDFGraph graph;
        private readonly RDFResource donaldduck;

        public QueryTests()
        {
            graph = GraphBuilder.WaltDisneyGraphBuild();
            donaldduck = new RDFResource("http://www.waltdisney.com/donald_duck");
        }

        [Fact]
        public void CreateVariableTest()
        {
            //Arrange
            var x = new RDFVariable("x");

            //Assert
            Assert.Equal("?X", x.VariableName);
        }

        [Fact]
        public void CreatePatternGroupTest()
        {
            //Arrange
            var pg1 = new RDFPatternGroup("pg1");

            //Assert
            Assert.Equal("PG1", pg1.PatternGroupName);
        }

        [Fact]
        public void CreateTriplePatternTest()
        {
            //Arrange
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");
            var dogOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "dogOf");

            //Act
            var y_dogOf_x = new RDFPattern(y, dogOf, x);

            //Assert
            Assert.Equal(y, y_dogOf_x.Subject);
            Assert.Equal(dogOf, y_dogOf_x.Predicate);
            Assert.Equal(x, y_dogOf_x.Object);
        }

        [Fact]
        public void CreateQuadruplePatternTest()
        {
            //Arrange
            var x = new RDFVariable("x");
            var y = new RDFVariable("y");
            var n = new RDFVariable("n");
            var dogOf = new RDFResource(RDFVocabulary.DC.BASE_URI + "dogOf");

            //Act
            var n_y_dogOf_x = new RDFPattern(n, y, dogOf, x);

            //Assert
            Assert.Equal(n, n_y_dogOf_x.Context);
            Assert.Equal(y, n_y_dogOf_x.Subject);
            Assert.Equal(dogOf, n_y_dogOf_x.Predicate);
            Assert.Equal(x, n_y_dogOf_x.Object);
        }

        [Fact]
        public void CreateEmptySelectQueryTest()
        {
            //Arrange
            var selectQuery = new RDFSelectQuery();

            //Assert
            Assert.NotEqual(0, selectQuery.QueryMemberID);
        }

        [Fact]
        public void CreateSelectQueryResultTest()
        {
            //Arrange
            var selectQuery = new RDFSelectQuery();

            //Act
            var selectQueryResult = selectQuery.ApplyToGraph(graph);

            //Assert
            Assert.Equal(0, selectQueryResult.SelectResultsCount);
        }

        [Fact]
        public void CreateEmptyAskQueryTest()
        {
            //Arrange
            var askQuery = new RDFAskQuery();

            //Assert
            Assert.NotEqual(0, askQuery.QueryMemberID);
        }

        [Fact]
        public void CreateAskQueryResultTest()
        {
            //Arrange
            var askQuery = new RDFAskQuery();

            //Act
            var askQueryResult = askQuery.ApplyToGraph(graph);

            //Assert
            Assert.False(askQueryResult.AskResult);
        }

        [Fact]
        public void CreateEmptyConstructQueryTest()
        {
            //Arrange
            var constructQuery = new RDFConstructQuery();

            //Assert
            Assert.NotEqual(0, constructQuery.QueryMemberID);
        }

        [Fact]
        public void CreateConstructQueryResultTest()
        {
            //Arrange
            var constructQuery = new RDFConstructQuery();

            //Act
            var constructQueryResult = constructQuery.ApplyToGraph(graph);

            //Assert
            Assert.Equal(0, constructQueryResult.ConstructResultsCount);
        }

        [Fact]
        public void CreateEmptyDescribeQueryTest()
        {
            //Arrange
            var describeQuery = new RDFDescribeQuery();
            
            //Assert
            Assert.NotEqual(0, describeQuery.QueryMemberID);
        }

        [Fact]
        public void CreateDescribeQueryResultTest()
        {
            //Arrange
            var describeQuery = new RDFDescribeQuery();

            //Act
            var describeQueryResult = describeQuery.ApplyToGraph(graph);

            //Assert
            Assert.Equal(0, describeQueryResult.DescribeResultsCount);
        }

        //<--12
        [Fact]
        public void CreateSameTermFilter()
        {
            //Arrange
            var x = new RDFVariable("x");
            var filter = new RDFSameTermFilter(x, donaldduck);
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void CreateLangMatchesFilter()
        {
            //Arrange
            var n = new RDFVariable("n");
            var filter = new RDFLangMatchesFilter(n, "it-IT");
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void CreateComparisonFilter()
        {
            //Arrange
            var y = new RDFVariable("y");
            var filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan, y, new RDFPlainLiteral("25"));
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void CreateIsLiteralFilter()
        {
            //Arrange
            var x = new RDFVariable("x");
            var filter = new RDFIsLiteralFilter(x);
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void CreateIsNumericFilter()
        {
            //Arrange
            var x = new RDFVariable("x");
            var filter = new RDFIsUriFilter(x);
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void CreateRegexFilter()
        {
            //Arrange
            var n = new RDFVariable("n");
            var filter = new RDFRegexFilter(n, new Regex(@"Mouse", RegexOptions.IgnoreCase));
            var pg1 = new RDFPatternGroup("PG1");

            //Act
            pg1.AddFilter(filter);

            //Assert
            Assert.Contains(filter.ToString(), pg1.ToString());
        }

        [Fact]
        public void AddOrderByModifierDescToQueryTest()
        {
            //Arrange
            var x = new RDFVariable("x");
            var query = new RDFSelectQuery();
            var modifier = new RDFOrderByModifier(x, RDFQueryEnums.RDFOrderByFlavors.DESC);

            //Act
            query.AddModifier(modifier);

            //Assert
            Assert.Contains($"order by {modifier}".ToUpper(), query.ToString().ToUpper());
        }

        [Fact]
        public void AddOrderByModifierAscToQueryTest()
        {
            //Arrange
            var n = new RDFVariable("n");
            var query = new RDFSelectQuery();
            var modifier = new RDFOrderByModifier(n, RDFQueryEnums.RDFOrderByFlavors.ASC);

            //Act
            query.AddModifier(modifier);

            //Assert
            Assert.Contains($"order by {modifier}".ToUpper(), query.ToString().ToUpper());
        }

        [Fact]
        public void AddDistinctModifierToQueryTest()
        {
            //Arrange
            var query = new RDFSelectQuery();
            var modifier = new RDFDistinctModifier();

            //Act
            query.AddModifier(modifier);

            //Assert
            Assert.Contains("distinct".ToUpper(), query.ToString().ToUpper());
        }

        [Fact]
        public void AddLimitModifierToQueryTest()
        {
            //Arrange
            var query = new RDFSelectQuery();
            var modifier = new RDFLimitModifier(100);

            //Act
            query.AddModifier(modifier);

            //Assert
            Assert.Contains(modifier.ToString(), query.ToString());
        }

        [Fact]
        public void AddOffsetModifierToQueryTest()
        {
            //Arrange
            var query = new RDFSelectQuery();
            var modifier = new RDFOffsetModifier(25);

            //Act
            query.AddModifier(modifier);

            //Assert
            Assert.Contains(modifier.ToString(), query.ToString());
        }

        [Fact]
        public void SelectQueryResultTest()
        {
            //Arrange
            var x = new RDFVariable("x");
            var q = new RDFSelectQuery();
            var patternGroup = new RDFPatternGroup("PatternGroup");
            patternGroup.AddPattern(new RDFPattern(x, RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            q.AddPatternGroup(patternGroup);

            //Act
            var selectResult = q.ApplyToGraph(graph);

            //Assert
            Assert.Equal(2, selectResult.SelectResultsCount);
        }

        [Fact]
        public void AskQueryResultTrueTest()
        {
            //Arrange
            var y = new RDFVariable("y");
            var askQuery = new RDFAskQuery();
            var patternGroup = new RDFPatternGroup("PatternGroup");
            patternGroup.AddPattern(new RDFPattern(donaldduck, y, new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            askQuery.AddPatternGroup(patternGroup);

            //Act
            var result = askQuery.ApplyToGraph(graph);

            //Assert
            Assert.True(result.AskResult);
        }

        [Fact]
        public void AskQueryResultFalseTest()
        {
            //Arrange
            var y = new RDFVariable("y");
            var askQuery = new RDFAskQuery();
            var patternGroup = new RDFPatternGroup("PatternGroup");
            patternGroup.AddPattern(new RDFPattern(donaldduck, y, new RDFTypedLiteral("90", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
            askQuery.AddPatternGroup(patternGroup);

            //Act
            var result = askQuery.ApplyToGraph(graph);

            //Assert
            Assert.False(result.AskResult);
        }

        [Fact]
        public void DescribeQueryResultTest()
        {
            //Arrange
            var query = new RDFDescribeQuery();

            //Act
            query.AddDescribeTerm(new RDFResource("http://www.waltdisney.com/donald_duck"));
            var result = query.ApplyToGraph(graph);

            //Assert
            Assert.Equal(2,result.DescribeResultsCount);
        }

    }
}