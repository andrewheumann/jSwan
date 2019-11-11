using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jSwan
{
    public abstract class JSwanComponent : GH_Component
    {

       
        public JSwanComponent() : base()
        {

        }

        public JSwanComponent(string name, string nickname, string description) : base(name, nickname, description, "jSwan", "jSwan")
        {

        }

        public static string TryGetJsonFromFile(string json)
        {
            var last4 = json.Substring(Math.Max(0, json.Length - 4)).ToUpper();
            if (last4.Equals("JSON") && File.Exists(json))
            {
                json = File.ReadAllText(json);
            }

            return json;
        }


    }
}
