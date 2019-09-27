using GH_IO.Serialization;
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
    public class JTokenGoo : GH_Goo<JToken>
    {
        public JTokenGoo() : base()
        {

        }

        public JTokenGoo(JToken token)
        {
            Value = token;
        }

        public JTokenGoo(JTokenGoo other) : base(other)
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "JSON Object";

        public override string TypeDescription => "JSON Object";

        public override IGH_Goo Duplicate()
        {
            return new JTokenGoo(this);
        }

        public override string ToString()
        {
            return Value.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public override bool Read(GH_IReader reader)
        {
            string content = "";
            if(reader.TryGetString("Content", ref content))
            {
                Value = JsonConvert.DeserializeObject<JToken>(content);
            }
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Content", ToString());
            return base.Write(writer);
        }
    }
}
