using Azure;
using Azure.AI.Projects;

namespace Agents.Extensions;

public static class ThreadMessageExtensions
{
    public static void PrintMessages(this Response<PageableList<ThreadMessage>> messages, bool newestToOldest = true)
    {
        messages.Value?.Data?.PrintMessages(newestToOldest);
    }

    public static void PrintMessages(this IReadOnlyList<ThreadMessage> messages, bool newestToOldest = true)
    {
        foreach (ThreadMessage threadMessage in (newestToOldest ? messages : messages.Reverse()))
        {
            threadMessage.PrintMessage();
        }
    }

    public static void PrintMessage(this ThreadMessage threadMessage)
    {
        Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");

        foreach (MessageContent contentItem in threadMessage.ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.Write(textItem.Text);
            }
            else if (contentItem is MessageImageFileContent imageFileItem)
            {
                Console.Write($"<image from ID: {imageFileItem.FileId}");
            }

            Console.WriteLine();
        }
    }
}
