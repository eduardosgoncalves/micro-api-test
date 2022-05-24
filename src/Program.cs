using MicroApiTest;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/reset", () =>
{
    Operations.Accounts.Clear();
    Operations.Transactions.Clear();
    return Results.Ok();
});

app.MapGet("/balance", (int account_id) =>
{
    if (!Operations.Accounts.Any(a => a.AccountId.Equals(account_id)))
        return Results.NotFound(0);
    return Results.Ok(Operations.GetBalance(account_id));
});

app.MapPost("/event", (Event model) => model.Type switch
{
    "deposit" => Operations.Deposit(model),
    "withdraw" => Operations.Withdraw(model),
    "transfer" => Operations.Transfer(model),
    _ => Results.BadRequest("Invalid event type")
});

app.Run();
