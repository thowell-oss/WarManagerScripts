
using System;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles transport of the value with the associated descriptor (type)
    /// </summary>
    [Notes.Author("Handles transport of the value with the associated descriptor (type)")]
    public class ValueTypePair
    {
        /// <summary>
        /// The value
        /// </summary>
        /// <value></value>
        public object Value { get; set; }

        /// <summary>
        /// The descriptor of the value being transported
        /// </summary>
        /// <value></value>
        public string Type { get; set; }


        /// <summary>
        /// The column the value belongs to (if applicable)
        /// </summary>
        /// <value></value>
        public int ColumLocation { get; set; }


        /// <summary>
        /// Empty value type pair constructor
        /// </summary>
        public ValueTypePair()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">the value being given</param>
        /// <param name="type">the descriptor of the value being given</param>
        /// <remarks>sets the column to 1</remarks>
        public ValueTypePair(object value, string type, int columnLocation = 1)
        {

            if (type == null || type.Trim() == string.Empty)
                type = ColumnInfo.GetValueTypeOfParagraph;

            Value = value;
            Type = type;

            if (columnLocation < 0)
                throw new ArgumentOutOfRangeException(nameof(columnLocation), "The column location must be greater than or equal to zero");

            ColumLocation = columnLocation;
        }

        public override string ToString()
        {
            return Value.ToString() + " (" + Type + ")";
        }
    }
}
