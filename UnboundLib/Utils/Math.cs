namespace Unbound.Core.Utils
{
    public static class Math
    {
        public static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
