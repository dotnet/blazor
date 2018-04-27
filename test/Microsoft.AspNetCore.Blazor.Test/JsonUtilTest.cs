﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Json;
using System;
using System.Collections.Generic;
using System.Threading;
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
                Age = new TimeSpan(7665, 1, 30, 0)
            };

            // Act/Assert
            Assert.Equal(
                "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\"}",
                JsonUtil.Serialize(person));
        }

        [Fact]
        public void CanDeserializeClassFromJson()
        {
            // Arrange
            var json = "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\"}";

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
        public void CamelCasePropertyNamingIsTheDefaultPropertyNaming()
        {
            // Arrange
            var classJson = "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\"}";

            // Act
            var deserializedJson = JsonUtil.Deserialize<Person>(classJson);
            var serializedJson = JsonUtil.Serialize(deserializedJson);

            // Assert
            Assert.Equal(1844, deserializedJson.Id);
            Assert.Equal("Athos", deserializedJson.Name);
            Assert.Equal(new[] { "Aramis", "Porthos", "D'Artagnan" }, deserializedJson.Pets);
            Assert.Equal(Hobbies.Swordfighting, deserializedJson.Hobby);
            Assert.Equal(new[] { "Comte de la Fère", "Armand" }, deserializedJson.Nicknames);
            Assert.Equal(new DateTimeOffset(1825, 8, 6, 18, 45, 21, TimeSpan.FromHours(-6)), deserializedJson.BirthInstant);
            Assert.Equal(new TimeSpan(7665, 1, 30, 0), deserializedJson.Age);
            Assert.Equal(
                "{\"id\":1844,\"name\":\"Athos\",\"pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"hobby\":2,\"nicknames\":[\"Comte de la Fère\",\"Armand\"],\"birthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"age\":\"7665.01:30:00\"}",
                serializedJson);
        }

        [Fact]
        public void PascalCasePropertyNamingSerializesPropertyNamesToPascalCase()
        {
            var person = new Person
            {
                Id = 1844,
                Name = "Athos",
                Pets = new[] { "Aramis", "Porthos", "D'Artagnan" },
                Hobby = Hobbies.Swordfighting,
                Nicknames = new List<string> { "Comte de la Fère", "Armand" },
                BirthInstant = new DateTimeOffset(1825, 8, 6, 18, 45, 21, TimeSpan.FromHours(-6)),
                Age = new TimeSpan(7665, 1, 30, 0)
            };
            var commandResult = new SimpleStruct
            {
                StringProperty = "Test",
                BoolProperty = true,
                NullableIntProperty = 1
            };

            // Act
            var structResult = JsonUtil.Serialize(commandResult, SimpleJson.PropertyNaming.PascalCase);
            var classResult = JsonUtil.Serialize(person, SimpleJson.PropertyNaming.PascalCase);

            // Assert
            Assert.Equal("{\"StringProperty\":\"Test\",\"BoolProperty\":true,\"NullableIntProperty\":1}", structResult);
            Assert.Equal(
                "{\"Id\":1844,\"Name\":\"Athos\",\"Pets\":[\"Aramis\",\"Porthos\",\"D'Artagnan\"],\"Hobby\":2,\"Nicknames\":[\"Comte de la Fère\",\"Armand\"],\"BirthInstant\":\"1825-08-06T18:45:21.0000000-06:00\",\"Age\":\"7665.01:30:00\"}",
                classResult);
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
        public void JsonSerializeAndDeserializeIsThreadSafe()
        {
            // Arrange
            var command1 = new SimpleStruct
            {
                StringProperty = "Test",
                BoolProperty = true,
                NullableIntProperty = 1
            };
            var command2 = new SimpleStruct
            {
                StringProperty = "Test",
                BoolProperty = true,
                NullableIntProperty = 1
            };
            var json1 = "{\"StringProperty\":\"Test\",\"BoolProperty\":true,\"NullableIntProperty\":1}";
            var json2 = "{\"stringProperty\":\"Test\",\"boolProperty\":true,\"nullableIntProperty\":1}";
            SimpleStruct structSample1 = default;
            SimpleStruct structSample2 = default;

            var result1 = string.Empty;
            var result2 = string.Empty;

            //Act

            var thread1 = new Thread(() =>
            {
                structSample1 = JsonUtil.Deserialize<SimpleStruct>(json1, SimpleJson.PropertyNaming.PascalCase);
                result1 = JsonUtil.Serialize(command1, SimpleJson.PropertyNaming.PascalCase);
            });
            var thread2 = new Thread(() =>
            {
                structSample2 = JsonUtil.Deserialize<SimpleStruct>(json2);
                result2 = JsonUtil.Serialize(command2);
            });

            // Act
            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            // Assert
            Assert.Equal("{\"StringProperty\":\"Test\",\"BoolProperty\":true,\"NullableIntProperty\":1}", result1);
            Assert.Equal("{\"stringProperty\":\"Test\",\"boolProperty\":true,\"nullableIntProperty\":1}", result2);

            Assert.Equal("Test", structSample1.StringProperty);
            Assert.True(structSample1.BoolProperty);
            Assert.Equal(1, structSample1.NullableIntProperty);

            Assert.Equal("Test", structSample2.StringProperty);
            Assert.True(structSample2.BoolProperty);
            Assert.Equal(1, structSample2.NullableIntProperty);
        }
      
        public void SupportsInternalCustomSerializer()
        {
            // Arrange/Act
            var json = JsonUtil.Serialize(new WithCustomSerializer());

            // Asssert
            Assert.Equal("{\"key1\":\"value1\",\"key2\":123}", json);
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
    }
}
