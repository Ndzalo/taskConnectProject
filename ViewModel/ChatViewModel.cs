using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using taskConnectProject.Models;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace taskConnectProject.ViewModel;
public partial class ChatViewModel : ObservableObject
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    [ObservableProperty]
    private string _userInput;

    [ObservableProperty]
    private ObservableCollection<ChatMessages> _messages;

    [ObservableProperty]
    private bool _isLoading;

    public ChatViewModel()
    {
        _apiKey = "AIzaSyBytyNsFcJWAEqoUJ6hSIMfJeQI94eFz-Q";
        Messages = new ObservableCollection<ChatMessages>();
        _httpClient = new HttpClient();

        Messages.Add(new ChatMessages
        {
            Content = "Hello, I'm your Task Assistant, TaskBuddy.",
            IsUserMessage = false,
            Timestamp = DateTime.Now
        });
    }
    public ChatViewModel(IOptions<GoogleGeminiConfig> config) : this()
    {
        _apiKey = config.Value.ApiKey;
    }
    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
            return;

        var userMessage = UserInput;
        UserInput = string.Empty;

        Messages.Add(new ChatMessages
        {
            Content = userMessage,
            IsUserMessage = true,
            Timestamp = DateTime.Now
        });

        IsLoading = true;

        try
        {
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 800,
                    topP = 0.8,
                    topK = 40
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{API_URL}?key={_apiKey}";

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response.StatusCode, responseString);
                return;
            }

            var botResponse = ParseBotResponse(responseString);
            if (!string.IsNullOrEmpty(botResponse))
            {
                Messages.Add(new ChatMessages
                {
                    Content = botResponse,
                    IsUserMessage = false,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            await HandleException(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
    private string ParseBotResponse(string responseString)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var text))
                    {
                        return text.GetString();
                    }
                }
            }

            throw new JsonException("Unexpected response format");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex}");
            return "I apologize, but I encountered an error processing your request.";
        }
    }
    private async Task HandleErrorResponse(System.Net.HttpStatusCode statusCode, string responseString)
    {
        var errorMessage = "An error occurred while processing your request.";

        try
        {
            var errorJson = JsonDocument.Parse(responseString);
            if (errorJson.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message))
            {
                errorMessage = message.GetString();
            }
        }
        catch
        {
            errorMessage = $"Server returned status code: {statusCode}";
        }

        Messages.Add(new ChatMessages
        {
            Content = errorMessage,
            IsUserMessage = false,
            Timestamp = DateTime.Now
        });

        await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
    }
    private async Task HandleException(Exception ex)
    {
        var errorMessage = "I apologize, but I encountered an error processing your request.";
        Messages.Add(new ChatMessages
        {
            Content = errorMessage,
            IsUserMessage = false,
            Timestamp = DateTime.Now
        });

        Console.WriteLine($"Exception details: {ex}");
        await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
    }
}