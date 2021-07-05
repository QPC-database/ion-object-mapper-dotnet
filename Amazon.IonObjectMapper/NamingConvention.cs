namespace Amazon.IonObjectMapper
{
    /// <summary>
    /// Interface for property naming convention.
    /// </summary>
    public interface IonPropertyNamingConvention
    {
        /// <summary>
        /// Convert name back to original .NET property name.
        /// </summary>
        ///
        /// <param name="s">The specified naming convention version of the .NET property name.</param>
        ///
        /// <returns>The original .NET property name.</returns>
        public string ToProperty(string s);
        
        /// <summary>
        /// Convert .NET property name to some naming convention.
        /// </summary>
        ///
        /// <param name="s">The original .NET property name.</param>
        ///
        /// <returns>The specified naming convention version of the .NET property name.</returns>
        public string FromProperty(string s);
    }

    /// <summary>
    /// Camel Case naming convention for property names.
    /// </summary>
    public class CamelCaseNamingConvention : IonPropertyNamingConvention
    {
        /// <summary>
        /// Convert camel case name back to original .NET property name.
        /// </summary>
        ///
        /// <param name="s">Camel case version of the .NET property name.</param>
        ///
        /// <returns>The original .NET property name.</returns>
        public string ToProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }

        /// <summary>
        /// Convert .NET property name to camel case.
        /// </summary>
        ///
        /// <param name="s">The original .NET property name.</param>
        ///
        /// <returns>Camel case version of the .NET property name.</returns>
        public string FromProperty(string s)
        {
            return s.Substring(0, 1).ToLowerInvariant() + s.Substring(1);
        }
    }

    /// <summary>
    /// Camel Case naming convention for property names.
    /// </summary>
    public class TitleCaseNamingConvention : IonPropertyNamingConvention
    {
        /// <summary>
        /// Convert title case name back to original .NET property name.
        /// </summary>
        ///
        /// <param name="s">Title case version of the .NET property name.</param>
        ///
        /// <returns>The original .NET property name.</returns>
        public string ToProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }

        /// <summary>
        /// Convert .NET property name to title case.
        /// </summary>
        ///
        /// <param name="s">The original .NET property name.</param>
        ///
        /// <returns>Title case version of the .NET property name.</returns>
        public string FromProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }
    }

    /// <summary>
    /// Snake Case naming convention for property names.
    /// </summary>
    public class SnakeCaseNamingConvention : IonPropertyNamingConvention
    {
        /// <summary>
        /// Convert snake case name back to original .NET property name.
        /// </summary>
        ///
        /// <param name="s">Snake case version of the .NET property name.</param>
        ///
        /// <returns>The original .NET property name.</returns>
        public string FromProperty(string s)
        {
            var output = "";
            for (int i=0; i< s.Length; i++)
            {
                char c = s[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        output += "_";
                    }
                    output += char.ToLowerInvariant(c);
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }

        /// <summary>
        /// Convert .NET property name to snake case.
        /// </summary>
        ///
        /// <param name="s">The original .NET property name.</param>
        ///
        /// <returns>Snake case version of the .NET property name.</returns>
        public string ToProperty(string s)
        {
            if (s.Length == 0)
            {
                return "";
            }
            int i = 0;
            var output = "";
            if (s[0] == '_')
            {
                if (s.Length == 1) 
                {
                    i++;
                    output += "_";
                }
                else
                {
                    i += 2;
                    output += s.Substring(1, 1).ToUpperInvariant();
                }
            }
            else 
            {
                i++;
                output += s.Substring(0, 1).ToUpperInvariant();
            }
            for (; i< s.Length; i++)
            {
                char c = s[i];
                if (c == '_')
                {
                    if (i+1 < s.Length)
                    {
                        output += char.ToUpperInvariant(s[i+1]);
                        i++;
                    }
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }
    }
}