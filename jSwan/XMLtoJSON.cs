using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace jSwan
{
    public class XMLtoJSON : JSwanComponent
    {

        public XMLtoJSON() : base("XML to JSON", "X2J","Convert XML to JSON")
        {

        }
        public override Guid ComponentGuid => new Guid("09a7bf01-3f5b-4235-a395-87d84bc24a56");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("XML", "X", "XML-formatted text, or the path to an XML file", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "The converted JSON", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string xml = "";
            DA.GetData("XML", ref xml);
            if(xml.ToLowerInvariant().EndsWith(".xml") && File.Exists(xml))
            {
                xml = File.ReadAllText(xml);
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var json = JsonConvert.SerializeXmlNode(doc);
            DA.SetData("JSON", json);
        }
    }
}
