using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;

namespace jSwan
{
    public class SerializeKV : JSwanComponent
    {

        public SerializeKV() : base("Serialize Keys and Values", "ReJsonKV",
            "Serialize to JSON with specified keys and values")
        {

        }
        public override Guid ComponentGuid => new Guid("B6A01FCE-08DC-4881-87D7-3D53B089B8AB");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Key", "K", "The keys", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "The values", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("JSON", "J", "The JSON Output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var keys = new List<string>();
            var values = new List<dynamic>();

            DA.GetDataList("Key", keys);
            DA.GetDataList("Value", values);

            var valueOutput = new JsonDict();

            if (keys.Count != 1 && values.Count != keys.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Key and Value count mismatch. This component expects either one key for a whole list, or one key per item in a list.");
                return;
            }

            if (keys.Count == 1 && values.Count != 1) // process as an array
            {
                valueOutput[keys.First()] = values.Select(v => v?.Value);
            }
            else
            {
                for (var i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var val = values[i]?.Value;
                    valueOutput[key] = val;
                }
            }

            if (valueOutput.Count > 0) DA.SetData("JSON", new JDictGoo(valueOutput));
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override Bitmap Icon => Properties.Resources.Serialize_KV;
    }
}
