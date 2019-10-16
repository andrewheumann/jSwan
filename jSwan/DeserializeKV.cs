using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace jSwan
{
    public class DeserializeKV : JSwanComponent
    {
        /// <summary>
        /// Initializes a new instance of the DeserializeKV class.
        /// </summary>
        public DeserializeKV()
          : base("Deserialize Keys and Values", "DeJsonKV",
              "Deserializes objects to keys and values")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "The Json to parse", GH_ParamAccess.item);
            pManager.AddTextParameter("Keys", "K", "Keys to retrieve", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Key", "K", "The key", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "The value", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var json = "";
            if (!DA.GetData("JSON", ref json)) return;
            List<string> keys = new List<string>();
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);
            if (DA.GetDataList("Keys", keys))
            {
                DA.SetDataList("Key", keys);
                DA.SetDataList("Value", keys.Select(k => deserialized.ContainsKey(k) ? deserialized[k] : null));
            }
            else
            {

                DA.SetDataList("Key", deserialized.Select(kvp => kvp.Key));
                DA.SetDataList("Value", deserialized.Select(kvp => kvp.Value));
            }

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("01e741f7-35bb-4e64-bd80-e304135374c7");
    }
}