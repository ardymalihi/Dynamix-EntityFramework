using System.Collections.Generic;

namespace Dynamix.EntityFramework.Model
{
    public class Table
    {
        public string Schema { get; set; }
        public string Name { get; set; }

        public List<Column> Columns { get; set; }

        public string VariableName
        {
            get
            {
                if (string.IsNullOrEmpty(Schema) || Schema == "dbo")
                {
                    return Name;
                }
                else
                {
                    return string.Format("{0}_{1}", Schema, Name);
                }
            }
        }

        //XML Extended Properties
        public string TableProperties { get; set; }


        public Table()
        {
            Columns = new List<Column>();
        }

        public override string ToString()
        {
            return VariableName;
        }
    }
}
