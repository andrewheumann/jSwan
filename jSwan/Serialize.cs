using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using jSwan.Properties;

namespace jSwan
{
    public class Serialize : LockableJSwanComponent, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the Serialize class.
        /// </summary>
        public Serialize()
          : base("Serialize Json", "ReJson",
              "Serialize it")
        {
            StructureLocked = false;
        }
        

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("JSON", "J", "The JSON Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var valueOutput = new JsonDict();
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var name = Params.Input[i].NickName;
                var access = Params.Input[i].Access;
                try
                {
                    switch (access)
                    {
                        case GH_ParamAccess.item:
                            dynamic dataValue = null;
                            DA.GetData(i, ref dataValue);
                            var rawValue = dataValue?.Value;
                            if (StructureLocked || rawValue != null)
                            {
                                valueOutput[name] = rawValue;
                            }

                            break;
                        case GH_ParamAccess.list:
                            var dataValues = new List<dynamic>();
                            DA.GetDataList(i, dataValues);
                            if (StructureLocked || dataValues.Any(v => v != null))
                            {
                                valueOutput[name] = dataValues.Select(v => v?.Value);
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message);
                }

                if (valueOutput.Count > 0) DA.SetData("JSON", new JDictGoo(valueOutput));
            }
        
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new Param_JsonInput {NickName = "-"};
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i];
                if (param.NickName == "-")
                {
                    param.Name = $"Data {i + 1}";
                    param.NickName = $"d{i + 1}";
                }
                else
                {
                    param.Name = param.NickName;
                }
                param.Description = $"Input {i + 1}";
                param.Optional = true;
                param.MutableNickName = true;
                //param.Access = GH_ParamAccess.item;
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("IncludeNullAndEmptyProperties", StructureLocked);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var locked = false;
            if(reader.TryGetBoolean("IncludeNullAndEmptyProperties", ref locked))
            {
                StructureLocked = locked;
            }
            UpdateMessage();
            return base.Read(reader);
        }


        public override LockableJSwanComponent.ComponentType Type => LockableJSwanComponent.ComponentType.Serialize;

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Lock Structure (Include null and empty properties)", Menu_LockOutput_Clicked, true, StructureLocked);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void Menu_LockOutput_Clicked(object sender, EventArgs e)
        {
            StructureLocked = !StructureLocked;
            ExpireSolution(true);
            UpdateMessage();
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.Serialize_JSON;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a02f83ba-161b-42cf-9f5e-cd27d7f39a6f");
    }
}