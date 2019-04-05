using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Parser
{
    class UsageEntity
    {
        public string Name { get; set; }
        public float Usage { get; set; }

        public UsageEntity(string name, float usage)
        {
            Name = name;
            Usage = usage;
        }
    }
}
