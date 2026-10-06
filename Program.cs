using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Middleware;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<UserStore>();

var app = builder.Build();

// Middleware pipeline (order matters):
// 1. Error handling  -> catches exceptions from everything below
// 2. Authentication  -> rejects requests without a valid token
// 3. Logging         -> logs only authenticated requests (and their final status)
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Helper: validate a User using DataAnnotations
static Dictionary<string, string[]> Validate(User user)
{
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(user, new ValidationContext(user), results, validateAllProperties: true);
    return results
        .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "User")
        .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid value.").ToArray());
}

var users = app.MapGroup("/users");

// READ all
users.MapGet("/", (UserStore store) => Results.Ok(store.GetAll()));

// READ one
users.MapGet("/{id:int}", (int id, UserStore store) =>
    store.GetById(id) is { } user
        ? Results.Ok(user)
        : Results.NotFound(new { error = $"User with id {id} not found." }));

// CREATE
users.MapPost("/", (User user, UserStore store) =>
{
    var errors = Validate(user);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    if (store.EmailExists(user.Email))
        return Results.Conflict(new { error = "A user with this email already exists." });

    var created = store.Add(user);
    return Results.Created($"/users/{created.Id}", created);
});

// UPDATE
users.MapPut("/{id:int}", (int id, User user, UserStore store) =>
{
    if (store.GetById(id) is null)
        return Results.NotFound(new { error = $"User with id {id} not found." });

    var errors = Validate(user);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    if (store.EmailExists(user.Email, excludeId: id))
        return Results.Conflict(new { error = "A user with this email already exists." });

    store.Update(id, user);
    return Results.Ok(store.GetById(id));
});

// DELETE
users.MapDelete("/{id:int}", (int id, UserStore store) =>
    store.Delete(id)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"User with id {id} not found." }));

app.Run();
