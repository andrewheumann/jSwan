using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace jSwan
{
    public class JSONToXML : JSwanComponent
    {
        public JSONToXML() : base("JSON to XML", "J2X", "Convert JSON to XML")
        {

        }
        public override Guid ComponentGuid => new Guid("3a5e2ab6-72ce-4ee1-8e2b-4b1bbf44b1ab");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "The converted JSON", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("XML", "X", "XML-formatted text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var json = "";
            DA.GetData("JSON", ref json);
            json = TryGetJsonFromFile(json);
            var jobject = JsonConvert.DeserializeObject(json);
            if (jobject is Newtonsoft.Json.Linq.JArray array)
            {
                jobject = new Dictionary<string, object>
                {
                    {"Items", jobject }
                };
                json = JsonConvert.SerializeObject(jobject);
            }
            XmlDocument xdoc = JsonConvert.DeserializeXmlNode(json, "Root");

            var xml = "";
            using (var stringwriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringwriter))
            {
                xdoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                xml = stringwriter.GetStringBuilder().ToString();
            }

            DA.SetData("XML", xml);
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override Bitmap Icon => Properties.Resources.JSON_to_XML;
    }
}
