using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jSwan
{
    public class JDictGoo : GH_Goo<JsonDict>
    {

        public JDictGoo() : base()
        {

        }

        public JDictGoo(JDictGoo other)
        {
            Value = other.Value;
        }

        public JDictGoo(JsonDict dict)
        {
            Value = dict;
        }

        public override bool IsValid => true;

        public override string TypeName => "JSON Object";

        public override string TypeDescription => "JSON Object Dictionary";

        public override IGH_Goo Duplicate()
        {
            return new JDictGoo(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Content", Value.ToString());
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            string content = "";
            if (reader.TryGetString("Content", ref content))
            {
                Value = JsonConvert.DeserializeObject<JsonDict>(content);
            }
            return base.Read(reader);
        }
    }
}
