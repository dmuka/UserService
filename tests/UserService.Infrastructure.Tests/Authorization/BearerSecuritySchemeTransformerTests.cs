using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Moq;

namespace UserService.Infrastructure.Tests.Authorization;

[TestFixture]
public class BearerSecuritySchemeTransformerTests
{
    private Mock<IAuthenticationSchemeProvider> _authenticationSchemeProviderMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private OpenApiDocumentTransformerContext _context;
    private BearerSecuritySchemeTransformer _transformer;

    [SetUp]
    public void SetUp()
    {
        _authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _context = new OpenApiDocumentTransformerContext()
        {
            DocumentName = "DocumentName",
            ApplicationServices = _serviceProviderMock.Object,
            DescriptionGroups = new List<ApiDescriptionGroup>()
        };
        _transformer = new BearerSecuritySchemeTransformer(_authenticationSchemeProviderMock.Object);
    }

    [Test]
    public async Task TransformAsync_WithBearerScheme_AddsSecuritySchemeToDocument()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme> 
        { 
            new("Bearer", null, typeof(IAuthenticationHandler))
        };

        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.Components, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes.ContainsKey("Bearer"), Is.True);
        
            var securityScheme = document.Components.SecuritySchemes["Bearer"];
            Assert.That(securityScheme.Type, Is.EqualTo(SecuritySchemeType.Http));
            Assert.That(securityScheme.Scheme, Is.EqualTo("bearer"));
            Assert.That(securityScheme.BearerFormat, Is.EqualTo("JWT"));
            Assert.That(securityScheme.In, Is.EqualTo(ParameterLocation.Header));
        }
    }

    [Test]
    public async Task TransformAsync_WithBearerScheme_AddsSecurityRequirementsToOperations()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme> 
        { 
            new("Bearer", null, typeof(IAuthenticationHandler))
        };

        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);

        var paths = new OpenApiPaths
        {
            ["/test"] = new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        OperationId = "GetTest",
                        Security = new List<OpenApiSecurityRequirement>()
                    }
                }
            }
        };

        var document = new OpenApiDocument { Paths = paths };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert
        using (Assert.EnterMultipleScope())
        {
            var operation = document.Paths["/test"].Operations[OperationType.Get];
            Assert.That(operation.Security, Has.Count.EqualTo(1));

            var securityRequirement = operation.Security[0];
            Assert.That(securityRequirement.Keys, Has.Count.EqualTo(1));

            var securityScheme = securityRequirement.Keys.First();
            Assert.That(securityScheme.Reference.Id, Is.EqualTo("Bearer"));
            Assert.That(securityScheme.Reference.Type, Is.EqualTo(ReferenceType.SecurityScheme));
        }
    }

    [Test]
    public async Task TransformAsync_WithMultipleAuthenticationSchemes_OnlyAddsBearerScheme()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme> 
        { 
            new("Bearer", null, typeof(IAuthenticationHandler)),
            new("Cookie", null, typeof(IAuthenticationHandler)),
            new("Basic", null, typeof(IAuthenticationHandler))
        };

        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.Components, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes, Has.Count.EqualTo(1));
            Assert.That(document.Components.SecuritySchemes.ContainsKey("Bearer"), Is.True);
        }
    }

    [Test]
    public async Task TransformAsync_WithCaseInsensitiveBearerScheme_AddsSecurityScheme()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme> 
        { 
            new("bearer", null, typeof(IAuthenticationHandler))  // lowercase "bearer"
        };

        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);

        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert

        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.Components, Is.Null);
            Assert.That(document.Components?.SecuritySchemes, Is.Null);
        }
    }

    [Test]
    public async Task TransformAsync_WithoutBearerScheme_DoesNotAddSecurityScheme()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme>();
        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);
            
        var document = new OpenApiDocument { Paths = new OpenApiPaths() };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert
        Assert.That(document.Components, Is.Null);
    }

    [Test]
    public async Task TransformAsync_WithPreExistingComponents_PreservesExistingComponents()
    {
        // Arrange
        var schemes = new List<AuthenticationScheme> 
        { 
            new("Bearer", null, typeof(IAuthenticationHandler))
        };

        _authenticationSchemeProviderMock.Setup(provider => provider.GetAllSchemesAsync())
            .ReturnsAsync(schemes);

        var document = new OpenApiDocument 
        { 
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["TestSchema"] = new OpenApiSchema { Type = "object" }
                }
            }
        };
    
        // Act
        await _transformer.TransformAsync(document, _context, CancellationToken.None);
    
        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.Components, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes, Is.Not.Null);
            Assert.That(document.Components.SecuritySchemes.ContainsKey("Bearer"), Is.True);
            Assert.That(document.Components.Schemas, Is.Not.Null);
            Assert.That(document.Components.Schemas.ContainsKey("TestSchema"), Is.True);
        }
    }
}