using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jSwan
{
    public class Merge : JSwanComponent
    {
        public Merge() : base("Merge Json", "JMerge", "Merge a subset of Json into a given string")
        {
        }

        public override Guid ComponentGuid => new Guid("ecd4d2fd-e522-4dd2-861a-5afc9bb8689c");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Source Json", "J", "The Json to modify", GH_ParamAccess.item);
            pManager.AddTextParameter("Override Json", "O", "The Json to merge", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Merged Json", "J", "The resultant Json", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jsonA = "";
            DA.GetData("Source Json", ref jsonA);

            string jsonB = "";
            DA.GetData("Override Json", ref jsonB);

            var a = JsonConvert.DeserializeObject<JObject>(jsonA);
            var b = JsonConvert.DeserializeObject<JObject>(jsonB);

            a.Merge(b, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            DA.SetData("Merged Json", a.ToString());
        }
    }
}
