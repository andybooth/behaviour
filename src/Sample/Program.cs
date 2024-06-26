using Behaviour;
using System.Collections.Concurrent;
using System.Security.Principal;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
var group = app.MapGroup("/application");

group.MapPost("/", async (Application application, IPrincipal principal, ILogger logger) =>
{
    var context = new BehaviourContext(logger)
    {
        Principal = principal
    };

    await BehaviourRunner.ExecuteAsync(context, [new SubmitApplicationFeature()]);
});

app.Run();

public record Application(string FirstName, string LastName, int Age);

public record Product(bool ExistingUsers, int MinimumAge);

public class SubmitApplicationFeature : BehaviourFeature<Application>
{
    public override List<BehaviourScenario> Scenarios => [
        new ProductLookup(),
        new AuthorizationPolicy(),
        new ApplicationValidation(),
        new AgeRestriction(),
        new ApplicationStore()
    ];
}

public class ProductLookup : BehaviourScenario
{
    public override BehaviourPhase Given(BehaviourContext context) => BehaviourPhase.Initialize;

    public override Task<BehaviourResult> ThenAsync(BehaviourContext context)
    {
        context.SetState(new Product(ExistingUsers: true, MinimumAge: 18));
        return context.Next();
    }
}

public class AuthorizationPolicy : BehaviourScenario
{
    public override BehaviourPhase Given(BehaviourContext context) => BehaviourPhase.Before;

    public override bool When(BehaviourContext context)
        => context.Principal?.Identity?.IsAuthenticated == context.GetState<Product>().ExistingUsers;

    public override Task<BehaviourResult> ThenAsync(BehaviourContext context)
        => context.Complete(code: 401);
}

public class AgeRestriction : BehaviourScenario<Application>
{
    public override BehaviourPhase Given(BehaviourContext context) => BehaviourPhase.Before;

    public override bool When(BehaviourContext context, Application input)
        => input.Age < context.GetState<Product>().MinimumAge;

    public override Task<BehaviourResult> ThenAsync(BehaviourContext context, Application input)
        => context.Complete(code: 400, message: $"Minimum age {context.GetState<Product>().MinimumAge}");
}

public class ApplicationValidation : BehaviourScenario<Application>
{
    public override BehaviourPhase Given(BehaviourContext context) => BehaviourPhase.Before;

    public override bool When(BehaviourContext context, Application input)
        => input.FirstName is null || input.LastName is null;

    public override Task<BehaviourResult> ThenAsync(BehaviourContext context, Application input)
        => context.Complete(code: 400, message: "First and last name required");
}

public class ApplicationStore : BehaviourScenario<Application>
{
    private static readonly ConcurrentDictionary<string, Application> Applications = [];

    public override Task<BehaviourResult> ThenAsync(BehaviourContext context, Application input)
    {
        var applicationId = Guid.NewGuid().ToString();
        Applications[applicationId] = input;
        return context.Complete(code: 200, output: applicationId);
    }
}
