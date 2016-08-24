using System;
using System.Linq;
using System.ComponentModel;
using Dynamix.EntityFramework.Runtime;
using Dynamix.EntityFramework.Model;
using Dynamix.EntityFramework.Util;

namespace Dynamix.EntityFramework.Db
{
    public delegate void FinishedLoading();

    public class DbAdapter
    {
        public event FinishedLoading OnFinishedLoading;

        private BackgroundWorker bg = new BackgroundWorker();

        public DbSchemaBuilder dbSchemaBuilder { get; set; }
        public DynamicClassBuilder dynamicClassBuilder { get; set; }
        public dynamic Instance { get; set; }

        public Config Config { get; set; }

        public bool IsActive { get; set; }
        

        public Type ContextType { get; set; }
        

        public DbAdapter(string ConnectionString, bool ExtendedProperties = false)
        {
            Config = new Config();
            Config.ConnectionString = ConnectionString;
            Config.ExtendedProperties = ExtendedProperties;
        }

        public void Load()
        {
            if (Config.Asynchronized)
            {
                bg.DoWork += new DoWorkEventHandler(bg_DoWork);
                bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
                bg.RunWorkerAsync();
            }
            else
            {
                bg_DoWork(this, null);
                if (OnFinishedLoading != null) OnFinishedLoading();
            }
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnFinishedLoading != null) OnFinishedLoading();
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                dbSchemaBuilder = new DbSchemaBuilder(Config);
                dynamicClassBuilder = new DynamicClassBuilder(Config);
                ContextType = dynamicClassBuilder.CreateContextType(dbSchemaBuilder.Tables);
                Instance = (DbContextBase)Activator.CreateInstance(ContextType);
                Instance.Database.Connection.ConnectionString = Config.ConnectionString;
                IsActive = true;
            }
            catch (Exception exp)
            {
                IsActive = false;
                e.Cancel = true;
                throw exp;
            }
        }

        public dynamic Get(string TableName)
        {
            return Utils.GetPropertyValue(this.Instance, TableName);
        }

        public dynamic New(dynamic dbSet)
        {
            Type type = Utils.GetListType(dbSet);
            return Activator.CreateInstance(type);
        }

        public dynamic New(string TableName)
        {
            var obj = Utils.GetPropertyValue(Instance, TableName);
            Type type = Utils.GetListType(obj);
            return Activator.CreateInstance(type);
        }

        public void Add(dynamic obj)
        {
            dynamic dbSet = Utils.GetPropertyValue(Instance, obj.GetType().Name);
            dbSet.Add(obj);
        }

        public void Add(dynamic MasterObject, dynamic DetailObject, string ConnectorField)
        {
            var obj = Utils.GetPropertyValue(MasterObject, ConnectorField);
            if (obj == null)
            {
                Type type = Utils.GetPropertyType(MasterObject, ConnectorField);
                obj = Activator.CreateInstance(type);
                Utils.SetPropertyValue(MasterObject, ConnectorField + Config.CollectionPostfixName, obj);
            }

            obj.Add(DetailObject);
        }

        public void Add(dynamic MasterObject, dynamic DetailObject)
        {
            Table table = dbSchemaBuilder.Tables.FirstOrDefault(o => o.VariableName == DetailObject.GetType().Name);
            var query = from o in table.Columns
                        where o.IsFK && o.ReferencedTable == MasterObject.GetType().Name
                        select o;
            if (query.Count() == 1)
            {
                Column column = query.FirstOrDefault();
                if (column != null)
                {
                    Add(MasterObject, DetailObject, column.ColumnName);
                }
                else
                {
                    throw new Exception("Cannot find any connection between two objects");
                }
            }
            else
            {
                throw new Exception("there is a logical problem connecting two objects");
            }
        }

        public void Delete(dynamic obj)
        {
            dynamic dbSet = Utils.GetPropertyValue(Instance, obj.GetType().Name);
            dbSet.Remove(obj);
        }

        public void Save()
        {
            Instance.SaveChanges();
        }

        public void Cancel()
        {
            Instance.CancelChanges();
        }

    }
}
