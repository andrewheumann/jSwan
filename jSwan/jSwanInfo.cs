using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace jSwan
{
    public class jSwanInfo : GH_AssemblyInfo
    {

        public override string Version => "1.2.0";

        public override string Name => "jSwan";

        public override Bitmap Icon => null;

        public override string Description => "A utility library for deserializing and composing JSON.";

        public override Guid Id => new Guid("048b6ac2-b3b3-473f-a17a-12f289de8413");

        public override string AuthorName => "Andrew Heumann";

        public override string AuthorContact => "";
    }
}
