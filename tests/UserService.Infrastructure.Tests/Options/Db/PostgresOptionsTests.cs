using System.ComponentModel.DataAnnotations;
using Infrastructure.Options.Db;
using Microsoft.Extensions.Options;

namespace UserService.Infrastructure.Tests.Options.Db
{
    [TestFixture]
    public class PostgresOptionsTests
    {
        private PostgresOptions _options;

        [SetUp]
        public void SetUp()
        {
            _options = new PostgresOptions
            {
                Host = "localhost",
                Port = 5432,
                Database = "testdb",
                UserName = "testuser",
                Password = "testpass"
            };
        }

        [Test]
        public void GetConnectionString_ShouldReturnCorrectFormat()
        {
            // Arrange
            const string expected = "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpass;";
            
            // Act
            var actual = _options.GetConnectionString();
            
            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase("h")]
        public void Host_ShouldBeValid(string host)
        {
            // Arrange Act
            _options.Host = host;
            
            // Assert
            Assert.Throws<ValidationException>(() => ValidateOptions(_options));
        }

        [Test]
        public void Port_ShouldBeRequired()
        {
            // Arrange Act
            _options.Port = 0;
            
            // Assert
            Assert.Throws<ValidationException>(() => ValidateOptions(_options));
        }

        [TestCase("")]
        [TestCase("d")]
        public void Database_ShouldBeRequired(string database)
        {
            // Arrange Act
            _options.Database = database;
            
            // Assert
            Assert.Throws<ValidationException>(() => ValidateOptions(_options));
        }

        [TestCase("")]
        [TestCase("use")]
        public void UserName_ShouldBeRequired(string userName)
        {
            // Arrange Act
            _options.UserName = userName;
            
            // Assert
            Assert.Throws<ValidationException>(() => ValidateOptions(_options));
        }

        [TestCase("")]
        [TestCase("pass")]
        public void Password_ShouldBeRequired(string password)
        {
            // Arrange Act
            _options.Password = password;
            
            // Assert
            Assert.Throws<ValidationException>(() => ValidateOptions(_options));
        }
        
        private void ValidateOptions(PostgresOptions options)
        {
            var context = new ValidationContext(options, serviceProvider: null, items: null);
            Validator.ValidateObject(options, context, validateAllProperties: true);
        }
    }
}