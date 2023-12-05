
using Data;

public interface IOpenAIService
{
    Task<string> Chat(string message);
    Task<string> Chat(List<Message> messages, string question);
}
