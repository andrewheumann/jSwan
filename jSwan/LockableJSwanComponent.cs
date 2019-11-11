using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace jSwan
{
    public abstract class LockableJSwanComponent : JSwanComponent
    {
        public LockableJSwanComponent(string name, string nickname, string description) : base(name, nickname, description)
        {

        }

        public enum ComponentType
        {
            Serialize,
            Deserialize
        }

        public abstract ComponentType Type { get; }

        internal bool StructureLocked { get; set; }

        internal void UpdateMessage()
        {
            Message = StructureLocked ? "Locked" : "";
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_DocumentObject.Menu_AppendItem(menu, $"Lock All JSwan {Type.ToString()} Components", LockAllJSwan_Clicked, true, false);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void LockAllJSwan_Clicked(object sender, EventArgs e)
        {
            Utilities.LockAllJswanComponents(OnPingDocument(), Type);
        }

    }
}
