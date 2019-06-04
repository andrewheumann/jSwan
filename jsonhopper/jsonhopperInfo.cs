using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace jsonhopper
{
    public class jsonhopperInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "jsonhopper";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("048b6ac2-b3b3-473f-a17a-12f289de8413");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
