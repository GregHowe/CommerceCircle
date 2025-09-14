using N1coLoyalty.Application.TermsConditions.Queries;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.FunctionalTests.TermsConditions.Queries;

using static Testing;

public class GetTermsConditionsTests : BaseTestFixture
{

    [Test]
    public async Task Should_Return_Current_TermsAndConditions()
    {

        var termsConditions1 = new TermsConditionsInfo { Url = "test1.com", IsCurrent = false, Version = "1.0.0", Created = DateTime.Now };
        var termsConditions2 = new TermsConditionsInfo { Url = "test2.com", IsCurrent = false, Version = "2.0.0", Created = DateTime.Now };
        var termsConditions3 = new TermsConditionsInfo { Url = "test3.com", IsCurrent = true, Version = "3.0.0", Created = DateTime.Now.AddMicroseconds(1.0) };

        await AddAsync(termsConditions1);
        await AddAsync(termsConditions2);
        await AddAsync(termsConditions3);

        var termsConditionsInfo = await SendAsync(new GetTermsConditionsInfoQuery());

        termsConditionsInfo.Should().NotBeNull();
        termsConditionsInfo.Version.Should().Be("3.0.0");

    }

    [Test]
    public async Task Should_Return_Null_TermsAndConditions_Cause_This_NotExist()
    {
        var termsConditionsInfo = await SendAsync(new GetTermsConditionsInfoQuery());
        termsConditionsInfo.Should().BeNull();
    }

    [Test]
    public async Task Should_Return__TermsAndConditions_Accepted()
    {
        var user = new User() { ExternalUserId = "anyIdUser", Id = Guid.NewGuid() };

        var termsConditions1 = new TermsConditionsInfo { Url = "test1.com", IsCurrent = false, Version = "1.0.0", Created = DateTime.Now };
        var termsConditions2 = new TermsConditionsInfo { Url = "test2.com", IsCurrent = false, Version = "2.0.0", Created = DateTime.Now };
        var termsConditions3 = new TermsConditionsInfo { Url = "test3.com", IsCurrent = true, Version = "3.0.0", Created = DateTime.Now.AddMicroseconds(1.0) };

        await AddAsync(termsConditions1);
        await AddAsync(termsConditions2);

        var termsConditionsAccepted = new TermsConditionsAcceptance
        {
            User = user,
            UserId = user.Id,
            TermsConditionsInfo = termsConditions3
                                                                                                                ,
            TermsConditionsId = termsConditions3.Id,
            Created = DateTime.Now,
            IsAccepted = true
        };

        await AddAsync(termsConditionsAccepted);

        var termsConditionsInfo = await SendAsync(new GetTermsConditionsInfoQuery());

        termsConditionsInfo.Should().NotBeNull();
        termsConditionsInfo.IsAccepted.Should().Be(true);

    }

    [Test]
    public async Task Should_Return__TermsAndConditions_Not_Accepted()
    {
        var user = new User() { ExternalUserId = "anyIdUser", Id = Guid.NewGuid() };

        var termsConditions1 = new TermsConditionsInfo { Url = "test1.com", IsCurrent = false, Version = "1.0.0", Created = DateTime.Now };
        var termsConditions2 = new TermsConditionsInfo { Url = "test2.com", IsCurrent = false, Version = "2.0.0", Created = DateTime.Now };
        var termsConditions3 = new TermsConditionsInfo { Url = "test3.com", IsCurrent = true, Version = "3.0.0", Created = DateTime.Now.AddMicroseconds(1.0) };

        await AddAsync(termsConditions1);
        await AddAsync(termsConditions2);

        var termsConditionsAccepted = new TermsConditionsAcceptance
        {
            Id = new Guid("969f6de2-36c0-4aa3-98ee-72aae37052e2")
                                                                                                                ,
            User = user,
            UserId = user.Id,
            TermsConditionsInfo = termsConditions3
                                                                                                                ,
            TermsConditionsId = termsConditions3.Id,
            Created = DateTime.Now,
            IsAccepted = false
        };

        await AddAsync(termsConditionsAccepted);

        var termsConditionsInfo = await SendAsync(new GetTermsConditionsInfoQuery());

        termsConditionsInfo.Should().NotBeNull();
        termsConditionsInfo.IsAccepted.Should().Be(false);

    }

    [Test]
    public async Task Should_Return__TermsAndConditions_Not_Accepted_When_ThereIsNot_Acceptance()
    {
        var user = new User() { ExternalUserId = "anyIdUser", Id = Guid.NewGuid() };

        var termsConditions1 = new TermsConditionsInfo { Url = "test1.com", IsCurrent = false, Version = "1.0.0", Created = DateTime.Now };
        var termsConditions2 = new TermsConditionsInfo { Url = "test2.com", IsCurrent = false, Version = "2.0.0", Created = DateTime.Now };
        var termsConditions3 = new TermsConditionsInfo { Url = "test3.com", IsCurrent = true, Version = "3.0.0", Created = DateTime.Now.AddMicroseconds(1.0) };

        await AddAsync(termsConditions1);
        await AddAsync(termsConditions2);
        await AddAsync(termsConditions3);

        var termsConditionsInfo = await SendAsync(new GetTermsConditionsInfoQuery());

        termsConditionsInfo.Should().NotBeNull();
        termsConditionsInfo.IsAccepted.Should().Be(false);

    }

    [Test]
    public async Task Catch_Exception_When_Insert_TwoCurrent_In_True()
    {
        var termsConditions1 = new TermsConditionsInfo { Url = "test1.com", IsCurrent = false, Version = "1.0.0", Created = DateTime.Now };
        var termsConditions2 = new TermsConditionsInfo { Url = "test2.com", IsCurrent = true, Version = "2.0.0", Created = DateTime.Now };
        var termsConditions3 = new TermsConditionsInfo { Url = "test3.com", IsCurrent = true, Version = "3.0.0", Created = DateTime.Now.AddMicroseconds(1.0) };

        await AddAsync(termsConditions1);
        await AddAsync(termsConditions2);

        Assert.CatchAsync<Exception>(() => AddAsync(termsConditions3));
        Assert.ThrowsAsync(Is.InstanceOf<Exception>(), async () => await AddAsync(termsConditions3));

    }


}

