namespace Diascan.Agent.Logger
{
    public enum TypeOfMessage
    {
        Info = 1,
        Warn,
        Error,
        Fatal
    }
    public class LogCollection
    {
        public int Id { get; set; }
        public string CurrentUserName { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }


        public LogCollection() { }
        public LogCollection(string currentUserName, string time, string message, TypeOfMessage typeOfMessage)
        {
            CurrentUserName = currentUserName;
            Time = time;
            Message = message;
            TypeOfMessage = typeOfMessage;
        }
    }
}
