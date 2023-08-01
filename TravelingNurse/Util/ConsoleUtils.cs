namespace TravelingNurse.Util
{
    public static class ConsoleUtils
    {
        public static string AsColor(string str, string color) => $"{color}{str}\x1b[39m";
        public static string Red(string str) => AsColor(str, "\x1b[91m");
        public static string Green(string str) => AsColor(str, "\x1b[92m");
        public static string Yellow(string str) => AsColor(str, "\x1b[93m");
        public static string Blue(string str) => AsColor(str, "\x1b[94m");
    }
}
