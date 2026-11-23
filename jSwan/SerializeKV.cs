using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace jSwan
{
    public class SerializeKV : JSwanComponent
    {

        public SerializeKV() : base("Serialize Keys and Values", "ReJsonKV",
            "Serialize to JSON with specified keys and values")
        {

        }
        public override Guid ComponentGuid => new Guid("B6A01FCE-08DC-4881-87D7-3D53B089B8AB");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Key", "K", "The keys", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "The values", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("JSON", "J", "The JSON Output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var keys = new List<string>();
            var values = new List<dynamic>();

            DA.GetDataList("Key", keys);
            DA.GetDataList("Value", values);

            var valueOutput = new JsonDict();

            if (keys.Count != 1 && values.Count != keys.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Key and Value count mismatch. This component expects either one key for a whole list, or one key per item in a list.");
                return;
            }

            if (keys.Count == 1 && (values.Count != 1 || !SingleValueAsScalar)) // process as an array
            {
                valueOutput[keys.First()] = values.Select(v => v?.Value);
            }
            else
            {
                for (var i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var val = values[i]?.Value;
                    valueOutput[key] = val;
                }
            }

            if (valueOutput.Count > 0) DA.SetData("JSON", new JDictGoo(valueOutput));
        }

        /// <summary>
        /// With one key and one value: true writes the value as a scalar ({"k": v}, behaviour since 1.2.1),
        /// false writes an array ({"k": [v]}, behaviour up to 1.2.0). New components use true.
        /// </summary>
        public bool SingleValueAsScalar { get; private set; } = true;

        private const string SingleValueAsScalarKey = "SingleValueAsScalar";

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean(SingleValueAsScalarKey, SingleValueAsScalar);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var scalar = true;
            if (reader.TryGetBoolean(SingleValueAsScalarKey, ref scalar))
            {
                SingleValueAsScalar = scalar;
            }
            else
            {
                // Saved before this setting existed: keep the behaviour of the jSwan version
                // the definition was saved with (array up to 1.2.0, scalar in 1.2.1).
                var savedWith = SavedWithJSwanVersion(reader);
                SingleValueAsScalar = savedWith != null && savedWith >= new Version(1, 2, 1);
            }
            UpdateMessage();
            return base.Read(reader);
        }

        /// <summary>
        /// The jSwan version recorded in the GHALibraries chunk of the archive being read, or null if unknown.
        /// </summary>
        private Version SavedWithJSwanVersion(GH_IReader reader)
        {
            try
            {
                var root = (reader as GH_Chunk)?.Archive?.GetRootNode;
                var libraries = root?.FindChunk("Definition")?.FindChunk("GHALibraries");
                if (libraries == null) return null;
                var libraryId = new jSwanInfo().Id;
                foreach (var library in libraries.Chunks.OfType<GH_IReader>())
                {
                    if (library.ItemExists("Id") && library.GetGuid("Id") == libraryId && library.ItemExists("Version")
                        && Version.TryParse(library.GetString("Version"), out var version))
                    {
                        return version;
                    }
                }
            }
            catch
            {
                // fall through: version unknown
            }
            return null;
        }

        private void UpdateMessage()
        {
            Message = SingleValueAsScalar ? null : "Single value as array";
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Single value as scalar", Menu_SingleValueAsScalar_Clicked, true, SingleValueAsScalar)
                .ToolTipText = "With one key and one value, write {\"key\": value} instead of {\"key\": [value]}.";
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void Menu_SingleValueAsScalar_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("Single value as scalar");
            SingleValueAsScalar = !SingleValueAsScalar;
            UpdateMessage();
            ExpireSolution(true);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override Bitmap Icon => Properties.Resources.Serialize_KV;
    }
}
