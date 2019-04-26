using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Parser
{
    class Pokemon
    {
        public string Name { get; }

        //Not sure get and set will work all that well with lists, but we'll see
        public List<string> Moves { get; set; }
        public List<string> Abilities { get; set; }
        public List<string> Items { get; set; }
        public List<string> Spreads { get; set; }

        public Pokemon(string name)
        {
            Name = name;
        }
    }
}
