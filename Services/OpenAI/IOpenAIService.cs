
using Data;

public interface IOpenAIService
{
    Task<string> Chat(List<Message> messages, string question);
}
