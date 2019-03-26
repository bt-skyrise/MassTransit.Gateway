using System;
using MassTransit.Gateway.MessageBuilder;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class JsonEnvelopeMessageFactoryTests
    {
        [Fact]
        public void should_create_message_ssing_dynamically_generated_type_when_class_name_is_not_present_in_cache()
        {
            // PREPARE
            var json = @"{
                            ""Id"":""33321"",
                            ""Name"":""Some name"",
                            ""Date"":""2019-03-26T12:00:00""
                         }";

            // RUN
            var message = JsonEnvelopeMessageFactory.CreateMessage("TestClass", json);

            // ASSERT
            CheckType(message.Type);

            Assert.Equal("33321", message.Type.GetProperty("Id").GetValue(message.Message));
            Assert.Equal("Some name", message.Type.GetProperty("Name").GetValue(message.Message));
            Assert.Equal(new DateTime(2019, 3, 26, 12,0,0), message.Type.GetProperty("Date").GetValue(message.Message));
        }

        private void CheckType(Type type)
        {
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
