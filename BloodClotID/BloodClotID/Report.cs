using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloodClotID
{
    class Report
    {
        private Report()
        {

        }

        static Report instance;
        public static Report Instance
        {
            get
            {
                if (instance == null)
                    instance = new Report();
                return instance;
            }
        }

        Dictionary<string, List<int>> assay_results = new Dictionary<string, List<int>>();
        public void AddResult(string assayName,List<int> results)
        {
            if (assay_results.ContainsKey(assayName))
            {
                assay_results[assayName].AddRange(results);
            }
            else
                assay_results.Add(assayName, results);
        }

        public Dictionary<string,List<int>> AssayResults
        {
            get
            {
                return assay_results;
            }
        }
    }
}
