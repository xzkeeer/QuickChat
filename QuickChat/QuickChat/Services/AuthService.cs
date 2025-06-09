using QuickChat.Models;
using QuickChat.Services;
using System.Net.Http;

public class AuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<UserModel> Login(object loginData)
    {
        try
        {
            return await _apiClient.PostAsync<UserModel>(
                "api/auth/login",
                loginData);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401"))
        {
            return new UserModel { Username = "error", AvatarUrl = "Неверный логин или пароль" };
        }
        catch (Exception ex)
        {
            return new UserModel { Username = "error", AvatarUrl = $"Ошибка: {ex.Message}" };
        }
    }

    public async Task<UserModel> Register(object registerData)
    {
        return await _apiClient.PostAsync<UserModel>(
            "api/auth/register",
            registerData);
    }
}