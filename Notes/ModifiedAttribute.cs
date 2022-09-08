
namespace Notes
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                               System.AttributeTargets.Struct|
                               System.AttributeTargets.Method,
                               AllowMultiple = true)  // multiuse attribute  
        ]
    public class ModifiedAttribute : System.Attribute
    {
        public static string ModifiedAuthor { get; set; }
        public static double Version { get; set; }
        public static string Notes { get; set; }

        /// <summary>
        /// The name of the author that modified the script, sets the version to 1
        /// </summary>
        /// <param name="modifiedBy">the name of the author</param>
        public ModifiedAttribute(string modifiedBy)
        {
            ModifiedAuthor = modifiedBy;
            Version = 1;
            Notes = "";
        }
        /// <summary>
        /// The name of the author that modified the script, sets the version to 1
        /// </summary>
        /// <param name="modifiedBy">the name of the author</param>
        /// <param name="notes">add notes to specify the modification</param>
        public ModifiedAttribute(string modifiedBy, string notes)
        {
            ModifiedAuthor = modifiedBy;
            Notes = notes;
            Version = 1;
        }

        /// <summary>
        /// The name of the author that modified the script, sets the version to 1
        /// </summary>
        /// <param name="modifiedBy">the name of the author</param>
        /// <param name="notes">add notes to specify the modification</param>
        /// <param name="version">specify the version when the modification took place</param>
        public ModifiedAttribute(string modifiedBy, string notes, double version)
        {
            ModifiedAuthor = modifiedBy;
            Notes = notes;
            Version = version;
        }
    }
}
