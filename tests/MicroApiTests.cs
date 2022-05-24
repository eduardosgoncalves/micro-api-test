using System.Net.Http.Json;

namespace MicroApiTest.Tests;

public class MicroApiTests
{
    const HttpStatusCode EXPECTED_OK_RESULT = HttpStatusCode.OK;
    const HttpStatusCode EXPECTED_NOT_FOUND_RESULT = HttpStatusCode.NotFound;

    [Fact]
    public async Task PostResetShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "OK";

        //Act
        var response = await client.PostAsync("/reset", null);
        var responseStr = response.Content.ReadAsStringAsync().Result;

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, responseStr);
    }

    [Fact]
    public async Task GetBalanceForNonExistingAccountShouldReturnNotFound()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();

        //Act
        var response = await client.GetAsync("/balance?account_id=1234");

        //Assert
        Assert.Equal(EXPECTED_NOT_FOUND_RESULT, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccountWithInitialBalanceShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "{\"destination\":{\"id\":\"100\",\"balance\":10}}";
        await client.PostAsync("/reset", null);

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, content);
    }

    [Fact]
    public async Task DepositIntoExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "{\"destination\":{\"id\":\"100\",\"balance\":20}}";
        await client.PostAsync("/reset", null);
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, content);
    }

    [Fact]
    public async Task GetBalanceForExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "20";
        await client.PostAsync("/reset", null);
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        //Act
        var response = await client.GetAsync("/balance?account_id=100");

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, response.Content.ReadAsStringAsync().Result);
    }

    [Fact]
    public async Task WithdrawFromNonExistingAccountShouldReturnNotFound()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "withdraw",
            origin = 200,
            amount = 10
        });

        //Assert
        Assert.Equal(EXPECTED_NOT_FOUND_RESULT, response.StatusCode);
    }

    [Fact]
    public async Task WithdrawFromExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "{\"origin\":{\"id\":\"100\",\"balance\":15}}";
        await client.PostAsync("/reset", null);
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "withdraw",
            origin = 100,
            amount = 5
        });

        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, content);
    }

    [Fact]
    public async Task TransferFromNonExistingAccountShouldReturnNotFound()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "transfer",
            origin = 200,
            destination = 300,
            amount = 15
        });

        //Assert
        Assert.Equal(EXPECTED_NOT_FOUND_RESULT, response.StatusCode);
    }


    [Fact]
    public async Task TransferFromExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        var expected = "{\"origin\":{\"id\":\"100\",\"balance\":0},\"destination\":{\"id\":\"300\",\"balance\":15}}";
        await client.PostAsync("/reset", null);
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });
        await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });
        await client.PostAsJsonAsync("/event", new
        {
            type = "withdraw",
            origin = 100,
            amount = 5
        });

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "transfer",
            origin = 100,
            destination = 300,
            amount = 15
        });

        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(EXPECTED_OK_RESULT, response.StatusCode);
        Assert.Equal(expected, content);
    }

}
