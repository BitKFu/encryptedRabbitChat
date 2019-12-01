namespace Demo.netFramework.Imlementation
{
    public class MessageObject
    {
        public MessageObject()
        {
        }

        public MessageObject(string msg)
        {
            Message = msg;
        }

        public string Message { get; set; }
    }
}
