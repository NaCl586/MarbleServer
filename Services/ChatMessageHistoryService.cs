using System;
using System.Collections.Generic;
using System.Linq;

namespace MarbleServer.Services
{
    public class ChatMessageHistoryService
    {
        private const int MaxMessages = 20;

        private readonly object _lock = new();

        private readonly Queue<ChatMessage> _messages = new();

        public void AddMessage(
            string username,
            string message,
            string status)
        {
            lock (_lock)
            {
                _messages.Enqueue(
                    new ChatMessage
                    {
                        Username = username,
                        Message = message,
                        Status = status ?? string.Empty,
                        IsSystem = false
                    });

                TrimHistory();
            }

            Console.WriteLine(
                $"Chat history: {_messages.Count} messages stored.");
        }

        public void AddSystemMessage(
            string message)
        {
            lock (_lock)
            {
                _messages.Enqueue(
                    new ChatMessage
                    {
                        Username = string.Empty,
                        Message = message,
                        Status = string.Empty,
                        IsSystem = true
                    });

                TrimHistory();
            }

            Console.WriteLine(
                $"Chat history: {_messages.Count} messages stored.");
        }

        public IReadOnlyList<ChatMessage> GetRecentMessages()
        {
            lock (_lock)
            {
                return _messages.ToList();
            }
        }

        private void TrimHistory()
        {
            while (_messages.Count > MaxMessages)
            {
                _messages.Dequeue();
            }
        }
    }

    public class ChatMessage
    {
        public string Username { get; set; } =
            string.Empty;

        public string Message { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public bool IsSystem { get; set; }
    }
}