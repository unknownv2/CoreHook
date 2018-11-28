using System;

namespace CoreHook.IPC.Messages
{
    public class InjectionCompleteMessage : CustomMessage
    {
        public bool Completed { get; }

        public int ProcessId { get; set; }

        public InjectionCompleteMessage(bool didComplete, int processId)
        {
            Completed = didComplete;
            ProcessId = processId;
        }

        internal static InjectionCompleteMessage FromBody(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            string[] dataParts = body.Split(MessageSeparator);

            if (dataParts.Length < 2)
            {
                throw new InvalidOperationException($"Invalid complete message. Expected at least 2 parts, got: {dataParts.Length} from message: '{body}'");
            }

            if (!int.TryParse(dataParts[0], out var processId))
            {
                throw new InvalidOperationException($"Invalid complete message. Expected PID, got: {dataParts[0]} from message: '{body}'");
            }

            if (!bool.TryParse(dataParts[1], out var didComplete))
            {
                throw new InvalidOperationException($"Invalid complete message. Expected bool for didComplete, got: {dataParts[1]} from message: '{body}'");
            }

            return new InjectionCompleteMessage(didComplete, processId);
        }

        public override string ToMessage()
        {
            return string.Join(MessageSeparator.ToString(), ProcessId, Completed);
        }
    }
}
