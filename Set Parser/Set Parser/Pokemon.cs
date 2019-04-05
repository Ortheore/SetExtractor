using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Parser
{
    class Pokemon
    {
        private UsageEntity species;
        private List<UsageEntity> moves;
        private List<UsageEntity> abilities;
        private List<UsageEntity> items;
        private List<UsageEntity> spreads;

        public Pokemon(string name, float usage)
        {
            species.Name = name;
            species.Usage = usage;
        }

        public void AddMove(string move, float usage)
        {
            moves.Add(new UsageEntity(move, usage));
        }
        public void AddMove(string move, float usage, float cutoff)
        {
            if (usage >= cutoff) moves.Add(new UsageEntity(move, usage));
        }

        public void AddAbility(string ability, float usage)
        {
            abilities.Add(new UsageEntity(ability, usage));
        }
        public void AddAbility(string ability, float usage, float cutoff)
        {
            if (usage >= cutoff) abilities.Add(new UsageEntity(ability, usage));
        }

        public void AddItem(string item, float usage)
        {
            items.Add(new UsageEntity(item, usage));
        }
        public void AddItem(string item, float usage, float cutoff)
        {
            if (usage >= cutoff) items.Add(new UsageEntity(item, usage));
        }

        public void AddSpread(string spread, float usage)
        {
            spreads.Add(new UsageEntity(spread, usage));
        }
        public void AddSpread(string spread, float usage, float cutoff)
        {
            if (usage >= cutoff) spreads.Add(new UsageEntity(spread, usage));
        }
    }
}
