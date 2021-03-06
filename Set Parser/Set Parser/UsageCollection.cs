﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Parser
{
    class UsageCollection
    {
        List<Usage> usages=new List<Usage>();
        public void Add(string name, float usage)
        {
            usages.Add(new Usage(name, usage));
        }

        public List<string> ApplyFilter(float usg)
        {
            List<string> results = new List<string>();
            foreach (Usage element in usages)
            {
                if (element.Value >= usg) results.Add(element.Name);
            }

            return results;
        }

        public List<string> ApplyFilter(int rank)
        {
            List<Usage> workingList = usages;
            workingList.Sort((u1, u2) => -1 * u1.Value.CompareTo(u2.Value));
            if (rank < workingList.Count) workingList.RemoveRange(rank, workingList.Count - rank);

            List<string> results = new List<string>();
            foreach (Usage usage in workingList) results.Add(usage.Name);

            return results;
        }

        public List<string> ApplyFilter(float usg, int rank)
        {
            //Remove those below min usage
            List<Usage> workingList = new List<Usage>();
            foreach (Usage usage in usages)//I'm great at naming things
            {
                if (usage.Value >= usg) workingList.Add(usage);
            }
            //Sort list
            workingList.Sort((u1, u2) => -1 * u1.Value.CompareTo(u2.Value));
            //Remove all usages below rank
            if (rank < workingList.Count) workingList.RemoveRange(rank, workingList.Count - rank);
            //Get strings
            List<string> results = new List<string>();
            foreach (Usage usage in workingList) results.Add(usage.Name);

            return results;
        }

        public void Reset()
        {
            usages = new List<Usage>();
        }

        //TESTing purposes only
        public int Count()
        {
            return usages.Count;
        }

        private class Usage
        {
            public string Name { get; }
            public float Value { get; }

            public Usage(string _name, float _usage)
            {
                Name = _name;
                Value = _usage;
            }

        }
    }
}
