// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test
{
    public class JsonUtilTest
    {
        // It's not useful to have a complete set of behavior specifications for
        // what the JSON serializer/deserializer does in all cases here. We merely
        // expose a simple wrapper over a third-party library that maintains its
        // own specs and tests.
        //
        // We should only add tests here to cover behaviors that Blazor itself
        // depends on.

        [Theory]
        [InlineData(null, "null")]
        [InlineData("My string", "\"My string\"")]
        [InlineData(123, "123")]
        [InlineData(123.456f, "123.456")]
        [InlineData(123.456d, "123.456")]
        [InlineData(true, "true")]
        public void CanSerializePrimitivesToJson(object value, string expectedJson)
        {
            Assert.Equal(expectedJson, JsonUtil.Serialize(value));
        }

        [Theory]
        [InlineData("null", null)]
        [InlineData("\"My string\"", "My string")]
        [InlineData("123", 123L)] // Would also accept 123 as a System.Int32, but Int64 is fine as a default
        [InlineData("123.456", 123.456d)]
        [InlineData("true", true)]
        public void CanDeserializePrimitivesFromJson(string json, object expectedValue)
        {
            Assert.Equal(expectedValue, JsonUtil.Deserialize<object>(json));
        }

        [Fact]
        public void CanSerializeClassToJson()
        {
            // Arrange
            var person = new Person
            {
                Id = 1844,
                Name = "Athos",
                Pets = new[] { "Aramis", "Porthos", "D'Artagnan" },
                Hobby = Hobbies.Swordfighting,
                Nicknames = new List<string> { "Comte de la Fère", "Armand" },
                BirthInstant = new DateTimeOffset(1825, 8, 6, 18, 45, 21, TimeSpan.FromHours(-6)),
                Age = new TimeSpan(7665, 1, 30, 0),
                Allergies = new Dictionary<string, object> { { "Ducks", true }, { "Geese", false } },
            };

            // Act/Assert
            Assert.Equal(
                "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\",\"allergies\":{\"Ducks\":true,\"Geese\":false}}",
                JsonUtil.Serialize(person));
        }

        [Fact]
        public void CanDeserializeClassFromJson()
        {
            // Arrange
            var json = "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\",\"allergies\":{\"Ducks\":true,\"Geese\":false}}";

            // Act
            var person = JsonUtil.Deserialize<Person>(json);

            // Assert
            Assert.Equal(1844, person.Id);
            Assert.Equal("Athos", person.Name);
            Assert.Equal(new[] { "Aramis", "Porthos", "D'Artagnan" }, person.Pets);
            Assert.Equal(Hobbies.Swordfighting, person.Hobby);
            Assert.Equal(new[] { "Comte de la Fère", "Armand" }, person.Nicknames);
            Assert.Equal(new DateTimeOffset(1825, 8, 6, 18, 45, 21, TimeSpan.FromHours(-6)), person.BirthInstant);
            Assert.Equal(new TimeSpan(7665, 1, 30, 0), person.Age);
            Assert.Equal(new Dictionary<string, object> { { "Ducks", true }, { "Geese", false } }, person.Allergies);
        }

        [Fact]
        public void CanDeserializeWithCaseInsensitiveKeys()
        {
            // Arrange
            var json = "{\"ID\":1844,\"NamE\":\"Athos\"}";

            // Act
            var person = JsonUtil.Deserialize<Person>(json);

            // Assert
            Assert.Equal(1844, person.Id);
            Assert.Equal("Athos", person.Name);
        }

        [Fact]
        public void DeserializationPrefersPropertiesOverFields()
        {
            // Arrange
            var json = "{\"member1\":\"Hello\"}";

            // Act
            var person = JsonUtil.Deserialize<PrefersPropertiesOverFields>(json);

            // Assert
            Assert.Equal("Hello", person.Member1);
            Assert.Null(person.member1);
        }

        [Fact]
        public void CanSerializeStructToJson()
        {
            // Arrange
            var commandResult = new SimpleStruct
            {
                StringProperty = "Test",
                BoolProperty = true,
                NullableIntProperty = 1
            };
            
            // Act
            var result = JsonUtil.Serialize(commandResult);
            
            // Assert
            Assert.Equal("{\"stringProperty\":\"Test\",\"boolProperty\":true,\"nullableIntProperty\":1}", result);
        }

        [Fact]
        public void CanDeserializeStructFromJson()
        {
            // Arrange
            var json = "{\"stringProperty\":\"Test\",\"boolProperty\":true,\"nullableIntProperty\":1}";

            //Act
            var simpleError = JsonUtil.Deserialize<SimpleStruct>(json);

            // Assert
            Assert.Equal("Test", simpleError.StringProperty);
            Assert.True(simpleError.BoolProperty);
            Assert.Equal(1, simpleError.NullableIntProperty);
        }

        [Fact]
        public void RejectsTypesWithAmbiguouslyNamedProperties()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                JsonUtil.Deserialize<ClashingProperties>("{}");
            });

            Assert.Equal($"The type '{typeof(ClashingProperties).FullName}' contains multiple public properties " +
                $"with names case-insensitively matching '{nameof(ClashingProperties.PROP1).ToLowerInvariant()}'. " +
                $"Such types cannot be used for JSON deserialization.",
                ex.Message);
        }

        [Fact]
        public void RejectsTypesWithAmbiguouslyNamedFields()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                JsonUtil.Deserialize<ClashingFields>("{}");
            });

            Assert.Equal($"The type '{typeof(ClashingFields).FullName}' contains multiple public fields " +
                $"with names case-insensitively matching '{nameof(ClashingFields.Field1).ToLowerInvariant()}'. " +
                $"Such types cannot be used for JSON deserialization.",
                ex.Message);
        }

        [Fact]
        public void NonEmptyConstructorThrowsUsefulException()
        {
            // Arrange
            var json = "{\"Property\":1}";
            var type = typeof(NonEmptyConstructorPoco);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                JsonUtil.Deserialize<NonEmptyConstructorPoco>(json); 
            });

            // Assert
            Assert.Equal(
                $"Cannot deserialize JSON into type '{type.FullName}' because it does not have a public parameterless constructor.", 
                exception.Message);
        }

        [Fact]
        public void SupportsInternalCustomSerializer()
        {
            // Arrange/Act
            var json = JsonUtil.Serialize(new WithCustomSerializer());

            // Asssert
            Assert.Equal("{\"key1\":\"value1\",\"key2\":123}", json);
        }

        // Test cases based on https://github.com/JamesNK/Newtonsoft.Json/blob/122afba9908832bd5ac207164ee6c303bfd65cf1/Src/Newtonsoft.Json.Tests/Utilities/StringUtilsTests.cs#L41
        // The only difference is that our logic doesn't have to handle space-separated words,
        // because we're only use this for camelcasing .NET member names
        [Theory]
        [InlineData("urlValue", "URLValue")]
        [InlineData("url", "URL")]
        [InlineData("id", "ID")]
        [InlineData("i", "I")]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("person", "Person")]
        [InlineData("xPhone", "xPhone")]
        [InlineData("xPhone", "XPhone")]
        [InlineData("x_Phone", "X_Phone")]
        [InlineData("x__Phone", "X__Phone")]
        [InlineData("isCIA", "IsCIA")]
        [InlineData("vmQ", "VmQ")]
        [InlineData("xml2Json", "Xml2Json")]
        [InlineData("snAkEcAsE", "SnAkEcAsE")]
        [InlineData("snA__kEcAsE", "SnA__kEcAsE")]
        [InlineData("snA__ kEcAsE", "SnA__ kEcAsE")]
        [InlineData("already_snake_case_ ", "already_snake_case_ ")]
        [InlineData("isJSONProperty", "IsJSONProperty")]
        [InlineData("shoutinG_CASE", "SHOUTING_CASE")]
        [InlineData("9999-12-31T23:59:59.9999999Z", "9999-12-31T23:59:59.9999999Z")]
        [InlineData("hi!! This is text. Time to test.", "Hi!! This is text. Time to test.")]
        [InlineData("building", "BUILDING")]
        [InlineData("buildingProperty", "BUILDINGProperty")]
        public void CamelCaseWorks(string expectedOutput, string input)
        {
            Assert.Equal(expectedOutput, input.ToCamelCase());
        }

        class NonEmptyConstructorPoco
        {
            public NonEmptyConstructorPoco(int parameter) {}

            public int Property { get; set; }
        }

        struct SimpleStruct
        {
            public string StringProperty { get; set; }
            public bool BoolProperty { get; set; }
            public int? NullableIntProperty { get; set; }
        }

        class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string[] Pets { get; set; }
            public Hobbies Hobby { get; set; }
            public IList<string> Nicknames { get; set; }
            public DateTimeOffset BirthInstant { get; set; }
            public TimeSpan Age { get; set; }
            public IDictionary<string, object> Allergies { get; set; }
        }

        enum Hobbies { Reading = 1, Swordfighting = 2 }

        class WithCustomSerializer : ICustomJsonSerializer
        {
            public object ToJsonPrimitive()
            {
                return new Dictionary<string, object>
                {
                    { "key1", "value1" },
                    { "key2", 123 },
                };
            }
        }

#pragma warning disable 0649
        class ClashingProperties
        {
            public string Prop1 { get; set; }
            public int PROP1 { get; set; }
        }

        class ClashingFields
        {
            public string Field1;
            public int field1;
        }

        class PrefersPropertiesOverFields
        {
            public string member1;
            public string Member1 { get; set; }
        }
#pragma warning restore 0649
    }
}
