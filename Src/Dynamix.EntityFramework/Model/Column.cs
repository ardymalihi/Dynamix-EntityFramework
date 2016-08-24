using Dynamix.EntityFramework.Util;

namespace Dynamix.EntityFramework.Model
{
    public class Column
    {
        public Table Table { get; set; }

        //Table
        public string TableSchema { get; set; }
        public string TableName { get; set; }

        //Column
        public string ColumnName { get; set; }
        public string ColumnDescription { get; set; }
        public string ColumnType { get; set; }
        public int ColumnLength { get; set; }
        public bool ColumnIsNullable { get; set; }

        //PK
        public string PKName { get; set; }
        public string PKColumnName { get; set; }
        public int PKPosition { get; set; }
        public bool PKIsIdentity { get; set; }

        //FK
        public string FKName { get; set; }
        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }

        //XML Extended Properties
        public string ColumnProperties { get; set; }

        //Getters
        public DataType DataType
        {
            get
            {
                return Utils.DBTypeToDataType(ColumnType, ColumnIsNullable);
            }
        }

        public bool IsPK
        {
            get
            {
                return !string.IsNullOrEmpty(PKName);
            }
        }

        public bool IsFK
        {
            get
            {
                return !string.IsNullOrEmpty(FKName);
            }
        }

        public bool Browsable
        {
            get
            {
                return !((this.IsFK) || (this.IsPK && !this.PKIsIdentity));
            }
        }

        public string ReferencedVariableName
        {
            get
            {
                if (string.IsNullOrEmpty(this.ReferencedSchema) || this.ReferencedSchema == "dbo")
                {
                    return this.ReferencedTable;
                }
                else
                {
                    return string.Format("{0}_{1}", this.ReferencedSchema, this.ReferencedTable);
                }
            }
        }

        public override string ToString()
        {
            return ColumnName;
        }
    }
}
