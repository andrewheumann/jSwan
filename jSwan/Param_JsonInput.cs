using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jSwan
{
    public class Param_JsonInput : Param_GenericObject, IDisposable
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        private void Menu_ItemAccessClicked(object sender, EventArgs e)
        {
            if (Access != 0)
            {
                Access = GH_ParamAccess.item;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }

        private void Menu_ListAccessClicked(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.list)
            {
                Access = GH_ParamAccess.list;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }

        private void Menu_TreeAccessClicked(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.tree)
            {
                Access = GH_ParamAccess.tree;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }


        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            if (Kind != GH_ParamKind.output)
            {

                Menu_AppendItem(menu, "Item Access", Menu_ItemAccessClicked, true, Access == GH_ParamAccess.item);
                Menu_AppendItem(menu, "List Access", Menu_ListAccessClicked, true, Access == GH_ParamAccess.list);

            }
        }

        protected override Bitmap Icon => Properties.Resources.param_jswan;

        public override Guid ComponentGuid => new Guid("{6965B371-D035-4A6D-9B69-4BB67673959B}");

        public override bool Write(GH_IWriter writer)
        {
            bool result = base.Write(writer);
            writer.SetInt32("ScriptParamAccess", (int)Access);
            return result;
        }


        public override bool Read(GH_IReader reader)
        {
            bool result = base.Read(reader);
           
            if (reader.ItemExists("ScriptParamAccess"))
            {
                try
                {
                    Access = (GH_ParamAccess)reader.GetInt32("ScriptParamAccess");
                    return result;
                }
                catch (Exception ex)
                {
                    //smoosh
                }
            }
            return result;
        }

        public void Dispose()
        {
            this.ClearData();
        }
    }
}
