using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Reflection.Emit;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using Dynamix.EntityFramework.Model;
using Dynamix.EntityFramework.Db;
using Dynamix.EntityFramework.Util;

namespace Dynamix.EntityFramework.Runtime
{
    public class DynamicClassBuilder
    {
        public Dictionary<string, Type> Types { get; set; }
        public Dictionary<string, TypeBuilder> TypeBuilders { get; set; }
        public TypeBuilder ContextTypeBuilder { get; set; }

        private Config _Config;

        public DynamicClassBuilder(Config config)
        {
            Types = new Dictionary<string, Type>();
            TypeBuilders = new Dictionary<string, TypeBuilder>();
            _Config = config;

        }

        public Type CreateContextType(List<Table> tables)
        {
            Type PocoType;
            TypeBuilder PocoTypeBuilder;
            //TypeBuilder ClassTypeBuilder;

            Types.Clear();
            TypeBuilders.Clear();

            //Context
            ContextTypeBuilder = DynamicTypeBuilder.GetTypeBuilder("Context", typeof(DbContextBase));

            //Context Constructor
            System.Reflection.Emit.ConstructorBuilder constructor = ContextTypeBuilder.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.RTSpecialName);

            //Create Normal Poco Type to be used as a reference
            foreach (Table table in tables)
            {
                TypeBuilders.Add(table.VariableName, CreatePocoTypeBuilder(table));
            }


            //Navigation properties
            foreach (Table table in tables)
            {
                CreateNavigationProperties(table);
            }


            //Creates DbSet Propeties for the Context
            foreach (Table ti in tables)
            {
                PocoTypeBuilder = TypeBuilders[ti.VariableName];
                PocoType = PocoTypeBuilder.CreateType();
                Types.Add(ti.VariableName, PocoType);
                DynamicTypeBuilder.CreateProperty(ContextTypeBuilder, ti.VariableName , typeof(DbSet<>).MakeGenericType(new Type[] { PocoType }), false);
            }


            Type type = ContextTypeBuilder.CreateType();


            return type;
        }

        private void CreateNavigationProperties(Table table)
        {
            PropertyInfo pi;
            TypeBuilder FKTypeBuilder;
            TypeBuilder CollectionTypeBuilder;
            TypeBuilder builder;

            foreach (Column column in table.Columns)
            {
                if (column.IsFK)
                {
                    builder = TypeBuilders[table.VariableName];

                    //Createing FK Object
                    FKTypeBuilder = TypeBuilders[column.ReferencedVariableName];
                    PropertyBuilder pb = DynamicTypeBuilder.CreateVirtualProperty(builder, column.ColumnName + _Config.ObjectPostfixName, FKTypeBuilder);

                    //DisplayName Attribute
                    ConstructorInfo DisplayNameAttributeBuilder = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                    pi = typeof(DisplayNameAttribute).GetProperties().FirstOrDefault(o => o.Name == "DisplayName");
                    var attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder, new object[] { Utils.GetFancyLabel(column.ColumnName + _Config.ObjectPostfixName) });
                    pb.SetCustomAttribute(attribute);

                    //Browsable Attribute
                    ConstructorInfo BrowsableAttributeBuilder = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                    pi = typeof(BrowsableAttribute).GetProperties().FirstOrDefault(o => o.Name == "Browsable");
                    attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder, new object[] { false });
                    pb.SetCustomAttribute(attribute);


                    //foreignKey Attribute
                    ConstructorInfo foreignKeyAttributeBuilder = typeof(ForeignKeyAttribute).GetConstructor(new Type[] { typeof(string) });
                    pi = typeof(ForeignKeyAttribute).GetProperties().FirstOrDefault(o => o.Name == "Name");
                    attribute = new CustomAttributeBuilder(foreignKeyAttributeBuilder, new object[] { column.ColumnName });
                    pb.SetCustomAttribute(attribute);

                    //Creating Collection Object for the referenced table
                    builder = TypeBuilders[column.ReferencedVariableName];

                    CollectionTypeBuilder = TypeBuilders[table.VariableName];
                    pb = DynamicTypeBuilder.CreateVirtualProperty(builder, column.TableName + _Config.CollectionPostfixName + "From" + column.ColumnName, typeof(CustomList<>).MakeGenericType(new Type[] { CollectionTypeBuilder.UnderlyingSystemType }));


                    //InverseProperty Attribute
                    ConstructorInfo InversePropertyAttributeBuilder = typeof(InversePropertyAttribute).GetConstructor(new Type[] { typeof(string) });
                    pi = typeof(InversePropertyAttribute).GetProperties().FirstOrDefault(o => o.Name == "Property");
                    attribute = new CustomAttributeBuilder(InversePropertyAttributeBuilder, new object[] { column.ColumnName + _Config.ObjectPostfixName });
                    pb.SetCustomAttribute(attribute);


                    //DisplayName Attribute
                    DisplayNameAttributeBuilder = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                    pi = typeof(DisplayNameAttribute).GetProperties().FirstOrDefault(o => o.Name == "DisplayName");
                    attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder, new object[] { Utils.GetFancyLabel(column.TableName + _Config.CollectionPostfixName + "From" + column.ColumnName) });
                    pb.SetCustomAttribute(attribute);

                    //Browsable Attribute
                    BrowsableAttributeBuilder = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                    pi = typeof(BrowsableAttribute).GetProperties().FirstOrDefault(o => o.Name == "Browsable");
                    attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder, new object[] { false });
                    pb.SetCustomAttribute(attribute);
                }
            }
        }

        private TypeBuilder CreatePocoTypeBuilder(Table table)
        {
            Type PropertyType;
            PropertyBuilder propertyBuilder;
            PropertyInfo pi;

            TypeBuilder builder = DynamicTypeBuilder.GetTypeBuilder(table.VariableName, typeof(PocoBase));

            ConstructorBuilder constructor = builder.DefineDefaultConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.RTSpecialName);


            //DataContract Attribute
            ConstructorInfo DataContractAttributeBuilder = typeof(DataContractAttribute).GetConstructor(new Type[] { });
            pi = typeof(DataContractAttribute).GetProperties().FirstOrDefault(o => o.Name == "IsReference");
            var attribute = new CustomAttributeBuilder(DataContractAttributeBuilder, new object[] { }, new PropertyInfo[] { pi }, new object[] { true });
            builder.SetCustomAttribute(attribute);

            //Table Schema Attribute
            ConstructorInfo TableAttributeBuilder = typeof(TableAttribute).GetConstructor(new Type[] { typeof(string) });
            pi = typeof(TableAttribute).GetProperties().FirstOrDefault(o => o.Name == "Schema");
            attribute = new CustomAttributeBuilder(TableAttributeBuilder, new object[] { table.Name }, new PropertyInfo[] { pi }, new object[] { table.Schema });
            builder.SetCustomAttribute(attribute);


            //Creating normal properties for each poco class
            foreach (Column column in table.Columns)
            {
                PropertyType = column.DataType.SystemType;
                propertyBuilder = DynamicTypeBuilder.CreateProperty(builder, column.ColumnName, PropertyType,true);

                
                //DisplayName Attribute
                ConstructorInfo DisplayNameAttributeBuilder = typeof(DisplayNameAttribute).GetConstructor(new Type[] { typeof(string) });
                pi = typeof(DisplayNameAttribute).GetProperties().FirstOrDefault(o => o.Name == "DisplayName");
                attribute = new CustomAttributeBuilder(DisplayNameAttributeBuilder, new object[] { Utils.GetFancyLabel(column.ColumnName) });
                propertyBuilder.SetCustomAttribute(attribute);


                if (column.IsPK)
                {
                    //Key Attribute
                    ConstructorInfo KeyAttributeBuilder = typeof(System.ComponentModel.DataAnnotations.KeyAttribute).GetConstructor(new Type[] { });
                    attribute = new CustomAttributeBuilder(KeyAttributeBuilder, new object[] { });
                    propertyBuilder.SetCustomAttribute(attribute);

                    
                    
                    //Column Attribute
                    ConstructorInfo ColumnAttributeBuilder = typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).GetConstructor(new Type[] { });
                    pi = typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).GetProperties().FirstOrDefault(o => o.Name == "Order");
                    attribute = new CustomAttributeBuilder(ColumnAttributeBuilder, new object[] { }, new PropertyInfo[] { pi }, new object[] { column.PKPosition });
                    propertyBuilder.SetCustomAttribute(attribute);

                    if (!column.PKIsIdentity)
                    {
                        ConstructorInfo IdentityAttributeBuilder = typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute).GetConstructor(new Type[] { typeof(DatabaseGeneratedOption) });
                        pi = typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute).GetProperties().FirstOrDefault(o => o.Name == "DatabaseGeneratedOption");
                        attribute = new CustomAttributeBuilder(IdentityAttributeBuilder, new object[] { DatabaseGeneratedOption.None });
                        propertyBuilder.SetCustomAttribute(attribute);
                    }
                    else
                    {
                        ConstructorInfo IdentityAttributeBuilder = typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute).GetConstructor(new Type[] { typeof(DatabaseGeneratedOption) });
                        pi = typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute).GetProperties().FirstOrDefault(o => o.Name == "DatabaseGeneratedOption");
                        attribute = new CustomAttributeBuilder(IdentityAttributeBuilder, new object[] { DatabaseGeneratedOption.Identity });
                        propertyBuilder.SetCustomAttribute(attribute);
                    }
                }

                //DataMember Attribute
                ConstructorInfo DataMemberAttributeBuilder = typeof(System.Runtime.Serialization.DataMemberAttribute).GetConstructor(new Type[] { });
                attribute = new CustomAttributeBuilder(DataMemberAttributeBuilder, new object[] { });
                propertyBuilder.SetCustomAttribute(attribute);


                bool Brawsable = column.Browsable;
                Brawsable = Brawsable | (column.IsFK && _Config.BrowseForeignKeyColumns) | (column.IsPK && _Config.BrowsePrimaryKeyColumns);
                //Browsable Attribute
                ConstructorInfo BrowsableAttributeBuilder = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });
                pi = typeof(BrowsableAttribute).GetProperties().FirstOrDefault(o => o.Name == "Browsable");
                attribute = new CustomAttributeBuilder(BrowsableAttributeBuilder, new object[] { Brawsable });
                propertyBuilder.SetCustomAttribute(attribute);

            }

            return builder;
        }
    }
}
