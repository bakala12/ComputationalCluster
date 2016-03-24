namespace CommunicationsUtils.Argument_parser
{
    public class OptionSetPool
    {
        public static string[] ServerOptionsSet => new[]
        {
            "port=","time=","backup","maddress=","mport=","verbose"
        };

        public static string[] ClientOptionsSet => new[]
        {
            "address=","port=","verbose"
        };
    }
}