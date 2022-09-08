
namespace Notes
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                           System.AttributeTargets.Struct | 
                           System.AttributeTargets.Method | 
                           System.AttributeTargets.Enum,
                           AllowMultiple = true)  // multiuse attribute  
    ]
    public class AuthorAttribute : System.Attribute
    {
        private string _authorName { get; set; } = "Taylor Howell";
        public double Version { get; set; } = 1.1;

        private string _description { get; set; } = "none";

        /// <summary>
        /// Author = Taylor Howell, Version = 1
        /// </summary>
        /// <param name="description">the description of the script</param>
        public AuthorAttribute(string description)
        {
            _authorName = "Taylor Howell";
            Version = 1.0;

            _description = description;
        }

        /// <summary>
        /// Author = Taylor Howell, set the version number
        /// </summary>
        /// <param name="version">the version of the script</param>
        /// <param name="description">the description of the script</param>
        public AuthorAttribute(double version, string description)
        {
            _authorName = "Taylor Howell";
            Version = version;

            _description = description;
        }

        /// <summary>
        /// Set the author custom and the version
        /// </summary>
        /// <param name="name">the name of the author</param>
        /// <param name="version">the version of the script</param>
        /// <param name="description">the description of the script</param>
        public AuthorAttribute(string name, double version, string description)
        {
            _authorName = name;
            Version = version;

            _description = description;
        }

        public override string ToString()
        {
            return "Author: " + _authorName + "\n Version: " + Version + " \n Description: " + _description;
        }
    }
}