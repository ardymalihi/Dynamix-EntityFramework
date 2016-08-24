using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Dynamix.EntityFramework.Model
{
    public class PocoBase : INotifyPropertyChanged

    {
        public override string ToString()
        {

            List<PropertyInfo> propertyInfo = this.GetType().GetProperties().Where(o => (o.PropertyType != typeof(byte[]) && !o.PropertyType.IsClass)).ToList();

            if (propertyInfo.Count < 1)
            {
                return base.ToString();
            }
            else
            {

                string[] s = propertyInfo.ConvertAll(o => string.Format("{0}={1}", o.Name, o.GetValue(this, null).ToString())).ToArray();
                return string.Join(",", s);

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(object sender,string PropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
