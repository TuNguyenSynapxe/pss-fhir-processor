using FluentAssertions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using Xunit;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.UnitTests
{
    public class ValidationErrorTests
    {
        [Fact]
        public void ValidationError_ShouldInitializeWithProperties()
        {
            // Arrange & Act
            var error = new ValidationError
            {
                Code = "ERR001",
                FieldPath = "Patient.name",
                Message = "Name is required",
                Scope = "Patient"
            };

            // Assert
            error.Code.Should().Be("ERR001");
            error.FieldPath.Should().Be("Patient.name");
            error.Message.Should().Be("Name is required");
            error.Scope.Should().Be("Patient");
        }

        [Fact]
        public void ValidationError_ShouldAllowNullProperties()
        {
            // Arrange & Act
            var error = new ValidationError();

            // Assert
            error.Code.Should().BeNull();
            error.FieldPath.Should().BeNull();
            error.Message.Should().BeNull();
            error.Scope.Should().BeNull();
        }
    }
}
