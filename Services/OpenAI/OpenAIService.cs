using Azure;
using Azure.AI.OpenAI;
using Data;

public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration Configuration;

    private Uri proxyUrl;
    private AzureKeyCredential token;
    private OpenAIClient openAIClient;
    private ChatCompletionsOptions completionOptions;
    private ChatMessage systemPrompt;
    public OpenAIService(IConfiguration configuration)
    {
        Configuration = configuration;
        proxyUrl = new Uri(Configuration["OpenAI:ProxyUrl"] ?? throw new ArgumentNullException("OpenAI:ProxyUrl"));
        token = new AzureKeyCredential(Configuration["OpenAI:Token"] ?? throw new ArgumentNullException("OpenAI:Token"));
        openAIClient = new OpenAIClient(proxyUrl, token);
        completionOptions = new ChatCompletionsOptions()
        {
            MaxTokens = 2048,
            Temperature = 0.7f,
            NucleusSamplingFactor = 0.95f,
            DeploymentName = "gpt-35-turbo"
        };
        var systemPrompt = """"
            You are a kind and helpful legal assistant that helps citizens of Namibia understand their legal rights in simple language. Use only the  Namibian Laws to answer the following questions.
            Greet the user with a friendly message and tell them that you are a legal assistant.
            If the user asks you a question that is not related to Namibian Law, say so.
            If the user asks you a question that is related to Namibian Law, but you do not know the answer, say so and refer them to the Legal Assistance Centre website.
            Always be polite and respectful.
            If the user says goodbye, say goodbye.
            If the user says thank you, say you are welcome.
            If the user says something that you do not understand, say so.
            Always recommend that the user consults a lawyer.
            """";
        completionOptions.Messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }
    public async Task<string> Chat(string message)
    {
        
        completionOptions.Messages.Add(new ChatMessage(ChatRole.User, message));
        ChatCompletions response = await openAIClient.GetChatCompletionsAsync(completionOptions);
        ChatMessage assistantResponse = response.Choices[0].Message;
        return assistantResponse.Content;
    }

    public async Task<string> Chat(List<Message> messages, string question)
    {
        foreach (var message in messages)
        {
            if(message.Role == null || message.Content == null) throw new ArgumentNullException("messages" + message.Role + message.Content);
            if(message.Role == "user")
            {
                completionOptions.Messages.Add(new ChatMessage(ChatRole.User, message.Content));
            }
            else if(message.Role == "assistant")
            {
                completionOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, message.Content));
            }
        }
        completionOptions.Messages.Add(new ChatMessage(ChatRole.User, question));
        ChatCompletions response = await openAIClient.GetChatCompletionsAsync(completionOptions);
        ChatMessage assistantResponse = response.Choices[0].Message;
        return assistantResponse.Content;        
    }
}