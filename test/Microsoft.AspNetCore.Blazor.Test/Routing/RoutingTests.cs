using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    public class RoutingTests
    {
        [Fact]
        public void Parse_SingleLiteral()
        {
            // Arrange
            var expected = new ExpectedTemplateBuilder().Literal("awesome");

            // Act
            var actual = TemplateRouteParser.ParseTemplate("awesome");

            // Assert
            Assert.Equal(expected, actual, RouteTemplateTestComparer.Instance);
        }

        [Fact]
        public void Parse_SingleParameter()
        {
            // Arrange
            var template = "{p}";

            var expected = new ExpectedTemplateBuilder().Parameter("p");

            // Act
            var actual = TemplateRouteParser.ParseTemplate(template);

            // Assert
            Assert.Equal(expected, actual, RouteTemplateTestComparer.Instance);
        }

        [Fact]
        public void Parse_MultipleLiterals()
        {
            // Arrange
            var template = "awesome/cool/super";

            var expected = new ExpectedTemplateBuilder().Literal("awesome").Literal("cool").Literal("super");

            // Act
            var actual = TemplateRouteParser.ParseTemplate(template);

            // Assert
            Assert.Equal(expected, actual, RouteTemplateTestComparer.Instance);
        }

        [Fact]
        public void Parse_MultipleParameters()
        {
            // Arrange
            var template = "{p1}/{p2}/{p3}";

            var expected = new ExpectedTemplateBuilder().Parameter("p1").Parameter("p2").Parameter("p3");

            // Act
            var actual = TemplateRouteParser.ParseTemplate(template);

            // Assert
            Assert.Equal(expected, actual, RouteTemplateTestComparer.Instance);
        }

        [Fact]
        public void InvalidTemplate_WithRepeatedParameter()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => TemplateRouteParser.ParseTemplate("{p1}/literal/{p1}"));

            var expectedMessage = "Invalid template '{p1}/literal/{p1}'. The parameter 'Microsoft.AspNetCore.Blazor.Routing.TemplateSegment' appears multiple times.";

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData("p}", "Invalid template 'p}'. Missing '{' in parameter segment 'p}'.")]
        [InlineData("{p", "Invalid template '{p'. Missing '}' in parameter segment '{p'.")]
        [InlineData("Literal/p}", "Invalid template 'Literal/p}'. Missing '{' in parameter segment 'p}'.")]
        [InlineData("Literal/{p", "Invalid template 'Literal/{p'. Missing '}' in parameter segment '{p'.")]
        [InlineData("p}/Literal", "Invalid template 'p}/Literal'. Missing '{' in parameter segment 'p}'.")]
        [InlineData("{p/Literal", "Invalid template '{p/Literal'. Missing '}' in parameter segment '{p'.")]
        [InlineData("Another/p}/Literal", "Invalid template 'Another/p}/Literal'. Missing '{' in parameter segment 'p}'.")]
        [InlineData("Another/{p/Literal", "Invalid template 'Another/{p/Literal'. Missing '}' in parameter segment '{p'.")]

        public void InvalidTemplate_WithMismatchedBraces(string template, string expectedMessage)
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => TemplateRouteParser.ParseTemplate(template));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData("{*}", "Invalid template '{*}'. The character '*' in parameter segment '{*}' is not allowed.")]
        [InlineData("{?}", "Invalid template '{?}'. The character '?' in parameter segment '{?}' is not allowed.")]
        [InlineData("{{}", "Invalid template '{{}'. The character '{' in parameter segment '{{}' is not allowed.")]
        [InlineData("{}}", "Invalid template '{}}'. The character '}' in parameter segment '{}}' is not allowed.")]
        [InlineData("{=}", "Invalid template '{=}'. The character '=' in parameter segment '{=}' is not allowed.")]
        [InlineData("{.}", "Invalid template '{.}'. The character '.' in parameter segment '{.}' is not allowed.")]
        [InlineData("{:}", "Invalid template '{:}'. The character ':' in parameter segment '{:}' is not allowed.")]
        public void ParseRouteParameter_ThrowsIf_ParameterContainsSpecialCharacters(string template, string expectedMessage)
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => TemplateRouteParser.ParseTemplate(template));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void InvalidTemplate_InvalidParameterNameWithEmptyNameThrows()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => TemplateRouteParser.ParseTemplate("{a}/{}/{z}"));

            var expectedMessage = "Invalid template '{a}/{}/{z}'. Empty parameter name in segment '{}' is not allowed.";

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void InvalidTemplate_ConsecutiveSeparatorsSlashSlashThrows()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => TemplateRouteParser.ParseTemplate("{a}//{z}"));

            var expectedMessage = "Invalid template '{a}//{z}'. Empty segments are not allowed.";

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void CanMatchRootTemplate()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/").Build();
            var context = new RouteContext("/");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
        }

        [Fact]
        public void CanMatchLiteralTemplate()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/literal").Build();
            var context = new RouteContext("/literal/");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
        }

        [Fact]
        public void CanMatchTemplateWithMultipleLiterals()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/some/awesome/route/").Build();
            var context = new RouteContext("/some/awesome/route");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
        }

        [Fact]
        public void RouteMatchingIsCaseInsensitive()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/some/AWESOME/route/").Build();
            var context = new RouteContext("/Some/awesome/RouTe");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
        }

        [Fact]
        public void DoesNotMatchIfSegmentsDontMatch()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/some/AWESOME/route/").Build();
            var context = new RouteContext("/some/brilliant/route");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.Null(context.Handler);
        }

        [Theory]
        [InlineData("/some")]
        [InlineData("/some/awesome/route/with/extra/segments")]
        public void DoesNotMatchIfDifferentNumberOfSegments(string path)
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/some/awesome/route/").Build();
            var context = new RouteContext(path);

            // Act
            routeTable.Route(context);

            // Assert
            Assert.Null(context.Handler);
        }

        [Theory]
        [InlineData("/value1", "value1")]
        [InlineData("/value2/", "value2")]
        public void CanMatchParameterTemplate(string path,string expectedValue)
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/{parameter}").Build();
            var context = new RouteContext(path);

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
            Assert.Single(context.Parameters, p => p.Key == "parameter" && p.Value == expectedValue);
        }

        [Fact]
        public void CanMatchTemplateWithMultipleParameters()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder().AddRoute("/{some}/awesome/{route}/").Build();
            var context = new RouteContext("/an/awesome/path");

            var expectedParameters = new Dictionary<string, string>
            {
                ["some"] = "an",
                ["route"] = "path"
            };

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
            Assert.Equal(expectedParameters, context.Parameters);
        }

        [Fact]
        public void PrefersLiteralTemplateOverTemplateWithParameters()
        {
            // Arrange
            var routeTable = new TestRouteTableBuilder()
                .AddRoute("/an/awesome/path")
                .AddRoute("/{some}/awesome/{route}/").Build();
            var context = new RouteContext("/an/awesome/path");

            // Act
            routeTable.Route(context);

            // Assert
            Assert.NotNull(context.Handler);
            Assert.Null(context.Parameters);
        }

        [Fact]
        public void PrefersShorterRoutesOverLongerRoutes()
        {
            // Arrange & Act
            var handler = typeof(int);
            var routeTable = new TestRouteTableBuilder()
                .AddRoute("/an/awesome/path")
                .AddRoute("/an/awesome/", handler).Build();

            // Act
            Assert.Equal("an/awesome", routeTable.Routes[0].Template.TemplateText);
        }

        [Fact]
        public void ProducesAStableOrderForNonAmbiguousRoutes()
        {
            // Arrange & Act
            var handler = typeof(int);
            var routeTable = new TestRouteTableBuilder()
                .AddRoute("/an/awesome/", handler)
                .AddRoute("/a/brilliant/").Build();

            // Act
            Assert.Equal("a/brilliant", routeTable.Routes[0].Template.TemplateText);
        }

        [Theory]
        [InlineData("/literal", "/Literal/")]
        [InlineData("/{parameter}", "/{parameter}/")]
        [InlineData("/literal/{parameter}", "/Literal/{something}")]
        [InlineData("/{parameter}/literal/{something}", "{param}/Literal/{else}")]
        public void DetectsAmbigousRoutes(string left, string right)
        {
            // Arrange
            var expectedMessage = $@"The following routes are ambiguous:
'{left.Trim('/')}' in '{typeof(object).FullName}'
'{right.Trim('/')}' in '{typeof(object).FullName}'
";
            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => new TestRouteTableBuilder()
                .AddRoute(left)
                .AddRoute(right).Build());

            Assert.Equal(expectedMessage, exception.Message);
        }

        private class TestRouteTableBuilder
        {
            IList<(string, Type)> _routeTemplates = new List<(string, Type)>();
            Type _handler = typeof(object);

            public TestRouteTableBuilder AddRoute(string template, Type handler = null)
            {
                _routeTemplates.Add((template, handler ?? _handler));
                return this;
            }

            public RouteTable Build() => new RouteTable(_routeTemplates
                .Select(rt => new RouteEntry(TemplateRouteParser.ParseTemplate(rt.Item1), rt.Item2))
                .OrderBy(id => id, RouteTable.RoutePrecedence)
                .ToArray());
        }

        private class ExpectedTemplateBuilder
        {
            public IList<TemplateSegment> Segments { get; set; } = new List<TemplateSegment>();

            public ExpectedTemplateBuilder Literal(string value)
            {
                Segments.Add(new TemplateSegment(value, isParameter: false));
                return this;
            }

            public ExpectedTemplateBuilder Parameter(string value)
            {
                Segments.Add(new TemplateSegment(value, isParameter: true));
                return this;
            }

            public RouteTemplate Build() => new RouteTemplate(string.Join('/', Segments), Segments.ToArray());

            public static implicit operator RouteTemplate(ExpectedTemplateBuilder builder) => builder.Build();
        }

        private class RouteTemplateTestComparer : IEqualityComparer<RouteTemplate>
        {
            public static RouteTemplateTestComparer Instance { get; } = new RouteTemplateTestComparer();

            public bool Equals(RouteTemplate x, RouteTemplate y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if ((x == null) != (y == null))
                {
                    return false;
                }

                if (x.Segments.Length != y.Segments.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Segments.Length; i++)
                {
                    var xSegment = x.Segments[i];
                    var ySegment = y.Segments[i];
                    if (xSegment.IsParameter != ySegment.IsParameter)
                    {
                        return false;
                    }
                    if (!string.Equals(xSegment.Value, ySegment.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(RouteTemplate obj) => 0;
        }
    }
}