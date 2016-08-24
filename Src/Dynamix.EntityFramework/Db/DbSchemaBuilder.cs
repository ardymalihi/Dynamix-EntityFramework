using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Dynamix.EntityFramework.Model;

namespace Dynamix.EntityFramework.Db
{
    public class DbSchemaBuilder
    {
        public Config _Config { get; set; }

        public List<Table> Tables = new List<Table>();
        public List<Column> Columns = new List<Column>();

        public DbSchemaBuilder(Config config)
        {
            try
            {
                _Config = config;
                GetColumns();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        private void GetColumns()
        {
            try
            {
                Table table = null;
                Column column = null;

                string tmpTableName = "";
                string tmpSchemaName = "";

                string ExtendedSQL = (_Config.ExtendedProperties == true) ?
    @"	,Convert(xml,(SELECT name,value FROM fn_listextendedproperty (NULL, 'schema', schema_name(t.schema_id), 'table', t.name, default, default) for xml raw)) as TableProperties
	,Convert(xml,(SELECT name,value FROM fn_listextendedproperty (NULL, 'schema', schema_name(t.schema_id), 'table', t.name, 'Column', c.name) for xml raw)) as ColumnProperties
" : "";

                string Filter = "";
                if (_Config.FilterSchemas.Count > 0)
                {
                    Filter = string.Format("and schema_name(t.schema_id) in ({0})",string.Join(",", _Config.FilterSchemas.ToArray()));

                    if (_Config.IncludedTables.Count > 0)
                    {
                        Filter = Filter + string.Format(" or t.name in ({0})", string.Join(",", _Config.IncludedTables.ToArray()));
                    }
                }


                string SQL =
    @"select
    schema_name(t.schema_id) TableSchema, t.name TableName, 
    c.name ColumnName, ex.value ColumnDescription, type_name(c.user_type_id) as ColumnType,	c.max_length as ColumnLength, c.is_nullable as ColumnIsNullable,
	pk.constraint_name PKName, pk_column_name PKColumnName, isnull(ordinal_position,0) PKPosition, isnull(pk.is_identity,0) PKIsIdentity,
    fk.fk_name FKName, fk.reference_schema ReferencedSchema, fk.referenced_object ReferencedTable, fk.referenced_column ReferencedColumn
	{0}
from sys.tables t      
left outer join 
sys.columns c on t.object_id = c.object_id
left outer join
sys.extended_properties ex on ex.minor_id = c.column_id
left outer join 
(select 
	object_name(constraint_object_id) fk_name	
	,fkc.parent_object_id
	,fkc.parent_column_id
	, schema_name(t.schema_id) reference_schema
	,object_name(referenced_object_id) referenced_object
	,(select name from sys.columns c where c.object_id = fkc.referenced_object_id and c.column_id = fkc.referenced_column_id) as referenced_column
 from sys.foreign_key_columns fkc inner join sys.tables t on t.object_id = fkc.referenced_object_id
) fk 
on fk.parent_object_id = t.object_id and c.column_id = fk.parent_column_id
left outer join 
(select 
	c.is_identity as is_identity,
	c.is_rowguidcol as is_rowguidcol,
	t.object_id as table_object_id, s.name as table_schema, t.name as table_name
    , k.name as constraint_name, k.type_desc as constraint_type
    , c.name as pk_column_name, ic.key_ordinal AS ordinal_position          
 from sys.key_constraints as k
 join sys.tables as t
 on t.object_id = k.parent_object_id
 join sys.schemas as s
 on s.schema_id = t.schema_id
 join sys.index_columns as ic
 on ic.object_id = t.object_id
 and ic.index_id = k.unique_index_id
 join sys.columns as c
 on c.object_id = t.object_id
 and c.column_id = ic.column_id
 where k.type_desc = 'PRIMARY_KEY_CONSTRAINT'
) pk on pk.table_object_id = t.object_id and pk.pk_column_name = c.name
where t.name <> 'sysdiagrams' {1}
order by TableSchema, TableName";

                SqlConnection Connection = new SqlConnection(_Config.ConnectionString);
                SqlCommand Command = new SqlCommand();
                SqlDataReader Reader;

                Command.CommandType = CommandType.Text;
                Command.Connection = Connection;
                Command.CommandText = string.Format(SQL, ExtendedSQL, Filter);

                try
                {
                    Connection.Open();
                    Reader = Command.ExecuteReader();

                    while (Reader.Read())
                    {
                        if (string.Format("{0}.Tables{1}", tmpSchemaName, tmpTableName) != string.Format("{0}.Tables{1}", Reader["TableSchema"].ToString(), Reader["TableName"].ToString()))
                        {
                            tmpSchemaName = Reader["TableSchema"].ToString();
                            tmpTableName = Reader["TableName"].ToString();

                            table = new Table();
                            table.Schema = tmpSchemaName;
                            table.Name = tmpTableName;
                            Tables.Add(table);
                        }

                        column = new Column();
                        column.Table = table;
                        column.TableSchema = Reader["TableSchema"].ToString();
                        column.TableName = Reader["TableName"].ToString();
                        column.ColumnName = _Config.PropertyPreFixName +  Reader["ColumnName"].ToString();
                        column.ColumnDescription = Reader["ColumnDescription"].ToString();
                        column.ColumnType = Reader["ColumnType"].ToString();
                        column.ColumnLength = Convert.ToInt32(Reader["ColumnLength"]);
                        column.ColumnIsNullable = Convert.ToBoolean(Reader["ColumnIsNullable"]);

                        column.PKName = Reader["PKName"].ToString();
                        column.PKColumnName = Reader["PKColumnName"].ToString();
                        column.PKPosition = Convert.ToInt32(Reader["PKPosition"]);
                        column.PKIsIdentity = Convert.ToBoolean(Reader["PKIsIdentity"]);

                        column.FKName = Reader["FKName"].ToString();
                        column.ReferencedSchema = Reader["ReferencedSchema"].ToString();
                        column.ReferencedTable = Reader["ReferencedTable"].ToString();
                        column.ReferencedColumn = Reader["ReferencedColumn"].ToString();

                        if (_Config.ExtendedProperties)
                        {
                            table.TableProperties = Reader["TableProperties"].ToString();
                            column.ColumnProperties = Reader["ColumnProperties"].ToString();
                        }

                        Columns.Add(column);
                        table.Columns.Add(column);
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Connection.Close();
                }


            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        
    
    }
}
