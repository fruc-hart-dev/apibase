using System.Text.Json;
using ApiBase.Models;

namespace ApiBase.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(Guid id, User user);
    Task<bool> DeleteUserAsync(Guid id);
}

public class JsonUserService : IUserService
{
    private readonly string _jsonFilePath;
    private readonly ILogger<JsonUserService> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonUserService(IConfiguration configuration, ILogger<JsonUserService> logger)
    {
        _jsonFilePath = Path.Combine(AppContext.BaseDirectory, "users.json");
        _logger = logger;
        EnsureJsonFileExists();
    }

    private void EnsureJsonFileExists()
    {
        if (!File.Exists(_jsonFilePath))
        {
            File.WriteAllText(_jsonFilePath, "[]");
        }
    }

    private async Task<List<User>> LoadUsersAsync()
    {
        try
        {
            var jsonString = await File.ReadAllTextAsync(_jsonFilePath);
            return JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users from JSON file");
            return new List<User>();
        }
    }

    private async Task SaveUsersAsync(List<User> users)
    {
        var jsonString = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_jsonFilePath, jsonString);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await LoadUsersAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await LoadUsersAsync();
            return users.FirstOrDefault(u => u.Id == id);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await LoadUsersAsync();
            user.Id = Guid.NewGuid();
            users.Add(user);
            await SaveUsersAsync(users);
            return user;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<User?> UpdateUserAsync(Guid id, User updatedUser)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await LoadUsersAsync();
            var existingUser = users.FirstOrDefault(u => u.Id == id);
            
            if (existingUser == null)
                return null;

            existingUser.Name = updatedUser.Name;
            existingUser.Password = updatedUser.Password;
            
            await SaveUsersAsync(users);
            return existingUser;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
                return false;

            users.Remove(user);
            await SaveUsersAsync(users);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}