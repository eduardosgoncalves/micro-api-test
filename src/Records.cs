namespace MicroApiTest;

public record Account(int? AccountId);
public record Event(string Type, int? Origin, int? Destination, int Amount);
public record Transaction(DateTime Date, string Type, int? Origin, int? Destination, int Amount);

public class Operations
{
    public static HashSet<Account> Accounts = new HashSet<Account>();
    public static HashSet<Transaction> Transactions = new HashSet<Transaction>();
    public static int MaxLimit = -100;

    public static IResult Deposit(Event model)
    {
        int? accountId = 0;
        int? balance = 0;

        if (!Accounts.Any(a => a.AccountId == model.Destination))
        {
            Accounts.Add(new Account(model.Destination ?? 0));
        }
        Transactions.Add(new Transaction(DateTime.Now, model.Type, null, model.Destination, model.Amount));

        accountId = Accounts.FirstOrDefault(a => a.AccountId == model.Destination)?.AccountId;
        balance = GetBalance(accountId);

        return Results.Ok(new
        {
            destination = new
            {
                id = $"{accountId}",
                balance = balance
            }
        });
    }

    public static IResult Withdraw(Event model)
    {
        int? accountId = 0;
        int? balance = 0;

        if (!Accounts.Any(a => a.AccountId == model.Origin))
        {
            return Results.NotFound(0);
        }

        accountId = Accounts.FirstOrDefault(a => a.AccountId == model.Origin)?.AccountId;
        balance = GetBalance(accountId);

        if (balance - model.Amount < MaxLimit)
            return Results.NotFound(0);

        Transactions.Add(new Transaction(DateTime.Now, model.Type, model.Origin, null, model.Amount));

        accountId = Accounts.FirstOrDefault(a => a.AccountId == model.Origin)?.AccountId;
        balance = GetBalance(accountId);

        return Results.Ok(new
        {
            origin = new
            {
                id = $"{accountId}",
                balance = balance
            }
        });
    }

    public static IResult Transfer(Event model)
    {
        int? originId = 0;
        int? originBalance = 0;
        int? destinationId = 0;
        int? destinationBalance = 0;

        if (!Accounts.Any(a => a.AccountId == model.Destination))
        {
            Accounts.Add(new Account(model.Destination));
        }

        if (!Accounts.Any(a => a.AccountId == model.Origin))
        {
            return Results.NotFound(0);
        }

        Transactions.Add(new Transaction(DateTime.Now, model.Type, model.Origin, model.Destination, model.Amount));
        originId = Accounts.FirstOrDefault(a => a.AccountId == model.Origin)?.AccountId;
        destinationId = Accounts.FirstOrDefault(a => a.AccountId == model.Destination)?.AccountId;
        originBalance = GetBalance(originId);
        destinationBalance = GetBalance(destinationId);

        return Results.Ok(new
        {
            origin = new
            {
                id = $"{originId}",
                balance = originBalance
            },
            destination = new
            {
                id = $"{destinationId}",
                balance = destinationBalance
            }
        });
    }

    public static int GetBalance(int? accountId)
    {
        var balance = 0;

        foreach (var transaction in Transactions.Where(a => a.Destination == accountId).OrderBy(t => t.Date))
        {
            if (transaction.Type == "deposit")
                balance += transaction.Amount;
            else if (transaction.Type == "withdraw")
                balance -= transaction.Amount;
            else if (transaction.Type == "transfer")
                balance += transaction.Amount;
        }

        foreach (var transaction in Transactions.Where(a => a.Origin == accountId).OrderBy(t => t.Date))
        {
            if (transaction.Type == "deposit")
                balance += transaction.Amount;
            else if (transaction.Type == "withdraw")
                balance -= transaction.Amount;
            else if (transaction.Type == "transfer")
                balance -= transaction.Amount;
        }

        return balance;
    }
}
