using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jSwan
{
    public abstract class JSwanComponent : GH_Component
    {

        internal bool StructureLocked { get; set; }

        public JSwanComponent() : base()
        {

        }

        public JSwanComponent(string name, string nickname, string description) : base(name, nickname, description, "jSwan", "jSwan")
        {

        }

        internal void UpdateMessage()
        {
            Message = StructureLocked ? "Locked" : "";
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_DocumentObject.Menu_AppendItem(menu, "Lock All JSwan Components", Menu_LockAllOutputs_Clicked, true, false);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void Menu_LockAllOutputs_Clicked(object sender, EventArgs e)
        {
            Utilities.LockAllJswanComponents(OnPingDocument());
        }


    }
}
