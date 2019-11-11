using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace jSwan
{
    public class JsonDict : Dictionary<string, object>, IDisposable
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
