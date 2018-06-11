// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        [InlineData("1234567890.123456", 1234567890.123456d)]
        [InlineData("-9223372036854775808", long.MinValue)]
        [InlineData("9223372036854775807", long.MaxValue)]
        [InlineData("18446744073709551615", ulong.MaxValue)]
        [InlineData("0.123456789012345", 0.123456789012345d)]
        public void CanDeserializePrimitivesFromJson(string json, object expectedValue)
        {
            var actual = JsonUtil.Deserialize<object>(json);
            Assert.Equal(expectedValue, actual);
        }

        [Theory]
        [InlineData("-9223372036854775809")]
        [InlineData("18446744073709551616")]
        [InlineData("0.12345678901234567")]
        [InlineData("1234567890.1234567")]
        public void CanDeserializeDecimalsFromJson(string json)
        {
            var expected = decimal.Parse(json);
            var actual = JsonUtil.Deserialize<object>(json);
            Assert.Equal(expected, actual);
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


        public static TheoryData<object, string> RoundTrippableBasicTypes => new TheoryData<object, string>
        {
            { byte.MinValue, "{\"value\":0}" },
            { ushort.MinValue, "{\"value\":0}" },
            { uint.MinValue, "{\"value\":0}" },
            { ulong.MinValue, "{\"value\":0}" },
            { sbyte.MinValue, "{\"value\":-128}" },
            { short.MinValue, "{\"value\":-32768}" },
            { int.MinValue, "{\"value\":-2147483648}" },
            { long.MinValue, "{\"value\":-9223372036854775808}" },
            { decimal.MinValue, "{\"value\":-79228162514264337593543950335}" },
            { double.MinValue, "{\"value\":-1.7976931348623157E+308}" },
            { byte.MaxValue, "{\"value\":255}" },
            { ushort.MaxValue, "{\"value\":65535}" },
            { uint.MaxValue, "{\"value\":4294967295}" },
            { ulong.MaxValue, "{\"value\":18446744073709551615}" },
            { sbyte.MaxValue, "{\"value\":127}" },
            { short.MaxValue, "{\"value\":32767}" },
            { int.MaxValue, "{\"value\":2147483647}" },
            { long.MaxValue, "{\"value\":9223372036854775807}" },
            { decimal.MaxValue, "{\"value\":79228162514264337593543950335}" },
            { double.MaxValue, "{\"value\":1.7976931348623157E+308}" },
            { new byte[] { }, "{\"value\":[]}" },
            { new byte[] { 0 }, "{\"value\":[0]}" },
            { new byte[] { 255 }, "{\"value\":[255]}" },
        };
        public static TheoryData<object, string> SerializeOnlyBasicTypes => new TheoryData<object, string>
        {
            { -0.79228162514264337593543950333d, "{\"value\":-0.79228162514264333}" }, // truncated to double precision
            { 0.79228162514264337593543950333d, "{\"value\":0.79228162514264333}" }, // truncated to double precision
            { -0.79228162514264337593543950333m, "{\"value\":-0.7922816251426433759354395033}" }, // looses one digit
            { 0.79228162514264337593543950333m, "{\"value\":0.7922816251426433759354395033}" }, // looses one digit
            { float.MinValue, "{\"value\":-3.402823E+38}" }, // truncated to float precision
            { float.MaxValue, "{\"value\":3.402823E+38}" }, // truncated to float precision
        };
        public static TheoryData<object, string> DeserializeOnlyBasicTypes => new TheoryData<object, string>
        {
            { -0.79228162514264344d, "{\"value\":-0.79228162514264337593543950333}" }, // truncated to double precision
            { 0.79228162514264344d, "{\"value\":0.79228162514264337593543950333}" }, // truncated to double precision
            { -792281625142643375935439503350000000000d, "{\"value\":-792281625142643375935439503350000000000}" },
            { -0.79228162514264337593543950335m, "{\"value\":-0.79228162514264337593543950335}" }, // looses one digit
            { 0.79228162514264337593543950335m, "{\"value\":0.79228162514264337593543950335}" }, // looses one digit
            { float.MinValue, "{\"value\":-3.40282347E+38}" }, // conversion from double is exact
            { float.MaxValue, "{\"value\":3.40282347E+38}" }, // conversion from double is exact
            { new byte[] { 0 }, "{\"value\":\"AA==\"}" }, // Base64
            { new byte[] { 255 }, "{\"value\":\"/w==\"}" }, // Base64
        };
        /// <summary>Test basic types that are not attribute types, which default value does not have type identity,
        /// which do not have constant min/max values, or which are truncated during serialization.</summary>
        public static TheoryData<string, string, string> RoundTrippableNonAttributeTypes => new TheoryData<string, string, string>
        {
            { "DateTime", "0", "{\"value\":\"0001-01-01T00:00:00Z\"}" }, // MinValue
            { "DateTime", "3155378975999999999", "{\"value\":\"9999-12-31T23:59:59.9999999Z\"}" }, // MaxValue
            { "DateTimeOffset", "0", "{\"value\":\"0001-01-01T00:00:00.0000000+00:00\"}" }, // MinValue
            { "DateTimeOffset", "3155378975999999999", "{\"value\":\"9999-12-31T23:59:59.9999999+00:00\"}" }, // MaxValue
            { "TimeSpan", "0", "{\"value\":\"00:00:00\"}" }, // default(TimeSpan)
            { "TimeSpan", "-9223372036854775808", "{\"value\":\"-10675199.02:48:05.4775808\"}" }, // MinValue
            { "TimeSpan", "9223372036854775807", "{\"value\":\"10675199.02:48:05.4775807\"}" }, // MaxValue
        };

        [Theory]
        [MemberData(nameof(RoundTrippableBasicTypes))]
        [MemberData(nameof(SerializeOnlyBasicTypes))]
        public void CanSerializePropertiesOfBasicTypes(object incoming, string expectedJson)
        {
            // Act/Assert
            ExecuteSwitch();

            bool ExecuteSwitch()
            {
                switch (incoming)
                {
                    case sbyte value: return Test(value, expectedJson);
                    case short value: return Test(value, expectedJson);
                    case int value: return Test(value, expectedJson);
                    case long value: return Test(value, expectedJson);
                    case byte value: return Test(value, expectedJson);
                    case ushort value: return Test(value, expectedJson);
                    case uint value: return Test(value, expectedJson);
                    case ulong value: return Test(value, expectedJson);
                    case float value: return Test(value, expectedJson);
                    case double value: return Test(value, expectedJson);
                    case decimal value: return Test(value, expectedJson);
                    case DateTime value: return Test(value, expectedJson);
                    case DateTimeOffset value: return Test(value, expectedJson);
                    case TimeSpan value: return Test(value, expectedJson);
                    case byte[] value: return Test(value, expectedJson);
                    default: throw new NotImplementedException();
                }
            }

            bool Test<T>(T value, string json)
            {
                var actual = JsonUtil.Serialize(new Wrapper<T> { Value = value });
                Assert.Equal(expectedJson, actual);
                return true;
            }
        }

        [Theory]
        [MemberData(nameof(RoundTrippableBasicTypes))]
        [MemberData(nameof(DeserializeOnlyBasicTypes))]
        public void CanDeserializePropertiesOfBasicTypes(object expected, string incomingJson)
        {
            // Act/Assert
            ExecuteSwitch();

            bool ExecuteSwitch()
            {
                switch (expected)
                {
                    case sbyte value: return Test(value, incomingJson);
                    case short value: return Test(value, incomingJson);
                    case int value: return Test(value, incomingJson);
                    case long value: return Test(value, incomingJson);
                    case byte value: return Test(value, incomingJson);
                    case ushort value: return Test(value, incomingJson);
                    case uint value: return Test(value, incomingJson);
                    case ulong value: return Test(value, incomingJson);
                    case float value: return Test(value, incomingJson);
                    case double value: return Test(value, incomingJson);
                    case decimal value: return Test(value, incomingJson);
                    case DateTime value: return Test(value, incomingJson);
                    case DateTimeOffset value: return Test(value, incomingJson);
                    case TimeSpan value: return Test(value, incomingJson);
                    case byte[] value: return Test(value, incomingJson);
                    default: throw new NotImplementedException();
                }
            }

            bool Test<T>(T expectedValue, string json)
            {
                var actual = JsonUtil.Deserialize<Wrapper<T>>(incomingJson);
                Assert.Equal(expectedValue, actual.Value);
                return true;
            }
        }
        [Theory]
        [MemberData(nameof(RoundTrippableNonAttributeTypes))]
        public void CanSerializePropertiesOfBasicTypesSpecialCases(string propertyType, string value, string expectedJson)
        {
            // Arrange
            var iv = CultureInfo.InvariantCulture;
            var incoming = Parse();
            object Parse()
            {
                switch (propertyType)
                {
                    case "float": return float.Parse(value, NumberStyles.Any, iv);
                    case "double": return double.Parse(value, NumberStyles.Any, iv);
                    case "decimal": return decimal.Parse(value, NumberStyles.Any, iv);
                    case "DateTime": return new DateTime(ticks: long.Parse(value, NumberStyles.Any, iv), kind: DateTimeKind.Utc);
                    case "DateTimeOffset": return new DateTimeOffset(ticks: long.Parse(value, NumberStyles.Any, iv), offset: TimeSpan.Zero);
                    case "TimeSpan": return new TimeSpan(ticks: long.Parse(value, NumberStyles.Any, iv));
                    default: throw new NotImplementedException();
                }
            }
            // Act/Assert
            CanSerializePropertiesOfBasicTypes(incoming, expectedJson);
        }
        /// <summary>Test basic types that are not attribute types, which default value does not have type identity,
        /// which do not have constant min/max values, or which are truncated during serialization.</summary>
        [Theory]
        [MemberData(nameof(RoundTrippableNonAttributeTypes))]
        public void CanDeserializePropertiesOfBasicTypesSpecialCases(string propertyType, string value, string json)
        {
            // Arrange
            var iv = CultureInfo.InvariantCulture;
            var expected = Parse();
            object Parse()
            {
                switch (propertyType)
                {
                    case "float": return float.Parse(value, NumberStyles.Any, iv);
                    case "double": return double.Parse(value, NumberStyles.Any, iv);
                    case "decimal": return decimal.Parse(value, NumberStyles.Any, iv);
                    case "DateTime": return new DateTime(ticks: long.Parse(value, NumberStyles.Any, iv), kind: DateTimeKind.Utc);
                    case "DateTimeOffset": return new DateTimeOffset(ticks: long.Parse(value, NumberStyles.Any, iv), offset: TimeSpan.Zero);
                    case "TimeSpan": return new TimeSpan(ticks: long.Parse(value, NumberStyles.Any, iv));
                    default: throw new NotImplementedException();
                }
            }

            // Act/Assert
            CanDeserializePropertiesOfBasicTypes(expected, json);
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
        //
        // Not all of the following cases are really valid .NET member names, but we have no reason
        // to implement more logic to detect invalid member names besides the basics (null or empty).
        [Theory]
        [InlineData("URLValue", "urlValue")]
        [InlineData("URL", "url")]
        [InlineData("ID", "id")]
        [InlineData("I", "i")]
        [InlineData("Person", "person")]
        [InlineData("xPhone", "xPhone")]
        [InlineData("XPhone", "xPhone")]
        [InlineData("X_Phone", "x_Phone")]
        [InlineData("X__Phone", "x__Phone")]
        [InlineData("IsCIA", "isCIA")]
        [InlineData("VmQ", "vmQ")]
        [InlineData("Xml2Json", "xml2Json")]
        [InlineData("SnAkEcAsE", "snAkEcAsE")]
        [InlineData("SnA__kEcAsE", "snA__kEcAsE")]
        [InlineData("already_snake_case_", "already_snake_case_")]
        [InlineData("IsJSONProperty", "isJSONProperty")]
        [InlineData("SHOUTING_CASE", "shoutinG_CASE")]
        [InlineData("9999-12-31T23:59:59.9999999Z", "9999-12-31T23:59:59.9999999Z")]
        [InlineData("Hi!! This is text. Time to test.", "hi!! This is text. Time to test.")]
        [InlineData("BUILDING", "building")]
        [InlineData("BUILDINGProperty", "buildingProperty")]
        public void MemberNameToCamelCase_Valid(string input, string expectedOutput)
        {
            Assert.Equal(expectedOutput, CamelCase.MemberNameToCamelCase(input));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MemberNameToCamelCase_Invalid(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                CamelCase.MemberNameToCamelCase(input));
            Assert.Equal("value", ex.ParamName);
            Assert.StartsWith($"The value '{input ?? "null"}' is not a valid member name.", ex.Message);
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

        class Wrapper<T> { public T Value { get; set; } }

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
