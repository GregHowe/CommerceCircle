using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.TermsConditions.Commands;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.FunctionalTests.TermsConditions.Commands;
using static Testing;

public class AcceptTermsConditionsTests: BaseTestFixture
{
    private TermsConditionsInfo _termsConditions;
    
    [SetUp]
    public async Task SetUp()
    {
        // create a new terms and conditions in the database
        _termsConditions = new TermsConditionsInfo()
        {
            Id = Guid.NewGuid(),
            Version = "1.0.0",
            Url = "https://n1co.com/terminos-y-condiciones/",
            IsCurrent = true
        };
        
        // save the terms and conditions in the database
        await AddAsync(_termsConditions);
    }
    
    [Test]
    public async Task Should_Accept_TermsConditions()
    {
        // call the command to accept the terms and conditions
        var command = new AcceptTermsConditionsCommand
        {
            IsAccepted = true
        };
        
        var response = await SendAsync(command);

        // assert the response
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        response.Data.Should().NotBeNull();
        response.Data?.IsAccepted.Should().BeTrue();
        response.Data?.Id.Should().NotBeEmpty();
        
        // assert the database
        var acceptance = await FindAsync<TermsConditionsAcceptance>(response.Data!.Id);
        acceptance.Should().NotBeNull();
        acceptance?.IsAccepted.Should().BeTrue();
        acceptance?.TermsConditionsId.Should().Be(_termsConditions.Id);
    }
    
    [Test]
    public async Task Should_Not_Accept_TermsConditions()
    {
        // call the command to don't accept the terms and conditions
        var command = new AcceptTermsConditionsCommand
        {
            IsAccepted = false
        };
        
        var response = await SendAsync(command);

        // assert the response
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        response.Data.Should().NotBeNull();
        response.Data?.IsAccepted.Should().BeFalse();
        response.Data?.Id.Should().NotBeEmpty();
        
        // assert the database
        var acceptance = await FindAsync<TermsConditionsAcceptance>(response.Data!.Id);
        acceptance.Should().NotBeNull();
        acceptance?.IsAccepted.Should().BeFalse();
        acceptance?.TermsConditionsId.Should().Be(_termsConditions.Id);
    }
    
    [Test]
    public async Task Should_Not_Accept_TermsConditions_When_Not_Exist()
    {
        // delete the terms and conditions from the database
        await RemoveAsync(_termsConditions);
        
        // call the command to don't accept the terms and conditions
        var command = new AcceptTermsConditionsCommand
        {
            IsAccepted = false
        };

        // assert the exception
        var exceptionAssertions = await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
        
        var errors = exceptionAssertions.Which.Errors;
        errors.Should().HaveCount(1);
        errors.Should().ContainKey("TermsConditions");
        errors["TermsConditions"].Should().Contain("No se encontraron los términos y condiciones.");
    }
    
    [Test]
    public async Task Should_Add_New_Registers_For_Not_Accepted()
    {
        // call the command to not accept the terms and conditions
        var firstCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = false
        };
        
        var firstResponse = await SendAsync(firstCommand);
        
        // assert the response
        firstResponse.Success.Should().BeTrue();
        firstResponse.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        firstResponse.Data.Should().NotBeNull();
        firstResponse.Data?.IsAccepted.Should().BeFalse();
        firstResponse.Data?.Id.Should().NotBeEmpty();
        
        // call the command to not accept the terms and conditions
        var secondCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = false
        };
        
        var secondResponse = await SendAsync(secondCommand);

        // assert the response
        secondResponse.Success.Should().BeTrue();
        secondResponse.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        secondResponse.Data.Should().NotBeNull();
        secondResponse.Data?.IsAccepted.Should().BeFalse();
        secondResponse.Data?.Id.Should().NotBeEmpty();
        
        // assert the database
        var acceptances = await AllToListAsync<TermsConditionsAcceptance>();
        acceptances.Count.Should().Be(2);
    }

    [Test]
    public async Task Should_Add_Only_One_Register_For_Accepted()
    {
        // call the command to not accept the terms and conditions
        var firstCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = true
        };
        
        var firstResponse = await SendAsync(firstCommand);
        
        // assert the response
        firstResponse.Success.Should().BeTrue();
        firstResponse.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        firstResponse.Data.Should().NotBeNull();
        firstResponse.Data?.IsAccepted.Should().BeTrue();
        firstResponse.Data?.Id.Should().NotBeEmpty();
        
        // call the command to not accept the terms and conditions
        var secondCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = true
        };
        
        var secondResponse = await SendAsync(secondCommand);

        // assert the response
        secondResponse.Success.Should().BeFalse();
        secondResponse.Message.Should().Be("Los términos y condiciones ya han sido aceptados.");
        secondResponse.Data.Should().NotBeNull();
        secondResponse.Data?.IsAccepted.Should().BeTrue();
        secondResponse.Data?.Id.Should().NotBeEmpty();
        
        // assert the database
        var acceptances = await AllToListAsync<TermsConditionsAcceptance>();
        acceptances.Count.Should().Be(1);
    }

    [Test]
    public async Task Should_Add_New_Register_When_Not_Accepted_And_Then_Accepted()
    {
        // call the command to not accept the terms and conditions
        var firstCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = false
        };
        
        var firstResponse = await SendAsync(firstCommand);
        
        // assert the response
        firstResponse.Success.Should().BeTrue();
        firstResponse.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        firstResponse.Data.Should().NotBeNull();
        firstResponse.Data?.IsAccepted.Should().BeFalse();
        firstResponse.Data?.Id.Should().NotBeEmpty();
        
        // call the command to accept the terms and conditions
        var secondCommand = new AcceptTermsConditionsCommand
        {
            IsAccepted = true
        };
        
        var secondResponse = await SendAsync(secondCommand);

        // assert the response
        secondResponse.Success.Should().BeTrue();
        secondResponse.Message.Should().Be("Aceptación de los términos y condiciones actualizada con éxito.");
        secondResponse.Data.Should().NotBeNull();
        secondResponse.Data?.IsAccepted.Should().BeTrue();
        secondResponse.Data?.Id.Should().NotBeEmpty();
        
        // assert the database
        var acceptances = await AllToListAsync<TermsConditionsAcceptance>();
        acceptances.Count.Should().Be(2);
    }
}
