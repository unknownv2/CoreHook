
namespace CoreHook.IPC.Messages
{
    public abstract class CustomMessage
    {
        protected const char MessageSeparator = '|';

        public abstract string ToMessage();
    }
}
