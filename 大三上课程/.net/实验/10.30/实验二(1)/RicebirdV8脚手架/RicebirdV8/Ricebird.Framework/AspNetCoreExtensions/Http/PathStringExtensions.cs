namespace Ricebird.Framework
{

    public static class PathStringExtensions
    {
        public static string PopSegment(this PathString srcPathString, out PathString remaining)
        {
            if (!srcPathString.HasValue)
            {
                remaining = srcPathString;
                return "";
            }

            string path = srcPathString.Value;
            int i = 1;
            StringBuilder builder = new StringBuilder();
            while (i < path.Length && path[i] != '/' && path[i] != '?')
            {
                builder.Append(path[i++]);
            }

            string remain = path[i..];

            remaining = i == path.Length ? new PathString() : new PathString(remain);
            return builder.ToString();
        }
    }
}
