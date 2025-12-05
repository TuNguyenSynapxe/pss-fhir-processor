using FluentAssertions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;
using Xunit;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    public class JsonHelperTests
    {
        [Fact]
        public void Serialize_ShouldReturnJsonString_WhenObjectProvided()
        {
            // Arrange
            var obj = new { name = "Test", value = 123 };

            // Act
            var result = JsonHelper.Serialize(obj);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("\"name\": \"Test\"");
            result.Should().Contain("\"value\": 123");
        }

        [Fact]
        public void Serialize_ShouldReturnNull_WhenObjectIsNull()
        {
            // Act
            var result = JsonHelper.Serialize(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Deserialize_ShouldReturnObject_WhenValidJsonProvided()
        {
            // Arrange
            var json = "{\"name\":\"Test\",\"value\":123}";

            // Act
            var result = JsonHelper.Deserialize<TestObject>(json);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test");
            result.Value.Should().Be(123);
        }

        private class TestObject
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        [Fact]
        public void Deserialize_ShouldReturnDefault_WhenJsonIsEmpty()
        {
            // Act
            var result = JsonHelper.Deserialize<string>("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Deserialize_ShouldReturnDefault_WhenJsonIsNull()
        {
            // Act
            var result = JsonHelper.Deserialize<string>(null);

            // Assert
            result.Should().BeNull();
        }
    }
}
