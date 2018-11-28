using System;

namespace CoreHook.IPC.Messages
{
    public class InjectionCompleteMessage : CustomMessage
    {
        public InjectionCompleteMessage(int pid, bool didComplete)
        {
            PID = pid;
            Completed = didComplete;
        }

        public int PID { get; set; }

        public bool Completed { get; set; }

        internal static InjectionCompleteMessage FromBody(string body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                string[] dataParts = body.Split(MessageSeparator);
                int pid;
                bool didComplete = false;

                if (dataParts.Length < 2)
                {
                    throw new InvalidOperationException($"Invalid complete message. Expected at least 2 parts, got: {dataParts.Length} from message: '{body}'");
                }

                if (!int.TryParse(dataParts[0], out pid))
                {
                    throw new InvalidOperationException($"Invalid complete message. Expected PID, got: {dataParts[0]} from message: '{body}'");
                }

                if (!bool.TryParse(dataParts[1], out didComplete))
                {
                    throw new InvalidOperationException($"Invalid complete message. Expected bool for didComplete, got: {dataParts[1]} from message: '{body}'");
                }

                return new InjectionCompleteMessage(pid, didComplete);
            }

            return null;
        }

        public override string ToMessage()
        {
            return string.Join(MessageSeparator.ToString(), PID, Completed);
        }
    }
}
