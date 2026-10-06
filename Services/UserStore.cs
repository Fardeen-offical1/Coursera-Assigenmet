using System.Collections.Concurrent;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

// Simple thread-safe in-memory store (no database needed).
public class UserStore
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId = 0;

    public UserStore()
    {
        Add(new User { Name = "Ali Khan", Email = "ali@example.com", Age = 28, Department = "IT" });
        Add(new User { Name = "Sara Ahmed", Email = "sara@example.com", Age = 31, Department = "HR" });
    }

    public IEnumerable<User> GetAll() => _users.Values.OrderBy(u => u.Id);

    public User? GetById(int id) => _users.TryGetValue(id, out var user) ? user : null;

    public bool EmailExists(string email, int? excludeId = null) =>
        _users.Values.Any(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.Id != excludeId);

    public User Add(User user)
    {
        user.Id = Interlocked.Increment(ref _nextId);
        _users[user.Id] = user;
        return user;
    }

    public bool Update(int id, User updated)
    {
        if (!_users.ContainsKey(id)) return false;
        updated.Id = id;
        _users[id] = updated;
        return true;
    }

    public bool Delete(int id) => _users.TryRemove(id, out _);
}
