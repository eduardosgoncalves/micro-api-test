using System.Net.Http.Json;

namespace MicroApiTest.Tests;

public class MicroApiTests
{
    [Fact]
    public async Task PostResetShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();

        //Act
        var response = await client.PostAsync("/reset", null);

        //Assert
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
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
        Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAccountWithInitialBalanceShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
        await client.PostAsync("/reset", null);

        //Act
        var response = await client.PostAsJsonAsync("/event", new
        {
            type = "deposit",
            destination = 100,
            amount = 10
        });

        var responseStr = "{\"destination\":{\"id\":\"100\",\"balance\":10}}";
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(content, responseStr);
    }

    [Fact]
    public async Task DepositIntoExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
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

        var responseStr = "{\"destination\":{\"id\":\"100\",\"balance\":20}}";
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(content, responseStr);
    }

    [Fact]
    public async Task GetBalanceForExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
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
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(response.Content.ReadAsStringAsync().Result, "20");
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
        Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WithdrawFromExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
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

        var responseStr = "{\"origin\":{\"id\":\"100\",\"balance\":15}}";
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(content, responseStr);
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
        Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task TransferFromExistingAccountShouldReturnOK()
    {
        //Arrange
        await using var application = new MicroApiApplication();
        var client = application.CreateClient();
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

        var responseStr = "{\"origin\":{\"id\":\"100\",\"balance\":0},\"destination\":{\"id\":\"300\",\"balance\":15}}";
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        Assert.Equal(content, responseStr);
    }

}
