namespace ServicesStatusChecker.Extensions
{
    public static class StaticExtensions
    {
        public static string ToHEX(this System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}
