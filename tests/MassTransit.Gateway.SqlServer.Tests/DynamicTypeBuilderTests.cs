using System;
using MassTransit.Gateway.MessageBuilder;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class DynamicTypeBuilderTests
    {
        [Fact]
        public void can_build_dynamic_type()
        {
            // PREPARE
            var json = @"{
                            ""Id"":""33321"",
                            ""Name"":""Some name"",
                            ""Date"":""2019-03-26T12:00:00""
                         }";

            // RUN
            var type = DynamicTypeBuilder.BuildMessageType("TestClass", json);

            // ASSERT
            Assert.Equal("TestClass", type.Name);
            Assert.Equal(3, type.GetProperties().Length);
            var idProperty = type.GetProperty("Id");
            Assert.Equal(typeof(string), idProperty.PropertyType);
            var nameProperty = type.GetProperty("Name");
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            var dateProperty = type.GetProperty("Date");
            Assert.Equal(typeof(DateTime), dateProperty.PropertyType);
        }
    }
}
