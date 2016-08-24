using System;

namespace Dynamix.EntityFramework.Model
{
    public struct DataType
    {
        public string DbType;
        public Type SystemType;

        public override string ToString()
        {
            return string.Format("({0}) {1}", DbType, SystemType.Name);
        }
    }
}
