using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Grasshopper.Kernel.Parameters;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace jsonhopper
{
    public class Deserialize : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Deserialize()
          : base("Deserialize Json", "DeJson",
              "deserialize it",
              "JsonHopper", "JsonHopper")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "The Json to parse", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        JObject deserialized = null;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var json = "";
            if (!DA.GetData("JSON", ref json)) return;
            deserialized = JsonConvert.DeserializeObject<JObject>(json);
            if (OutputMismatch())
            {
                OnPingDocument().ScheduleSolution(5, (d) =>
                {
                    AutoCreateOutputs(false);
                });
            }
            else
            {
                var children = deserialized.Children().ToList();
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child is JProperty property)
                    {
                        if (property.Value is JArray array)
                        {
                            DA.SetDataList(i, array);
                        }
                        else
                        {
                            DA.SetData(i, property.Value);

                        }
                    }
                }
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_DocumentObject.Menu_AppendItem(menu, "Match outputs", Menu_AutoCreateOutputs_Clicked);
        }

        private bool OutputMismatch()
        {
            var countMatch = deserialized.Children().Count() == Params.Output.Count;
            if (!countMatch) return true;

            var children = deserialized.Children().ToList();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is JProperty property)
                {
                    if (Params.Output[i].NickName != property.Name)
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        private void AutoCreateOutputs(bool recompute)
        {
            var tokens = deserialized.Children();
            var tokenCount = tokens.Count();

            var outputParamCount = Params.Output.Count;

            if (OutputMismatch())
            {
                RecordUndoEvent("Output from Json");
                if (Params.Output.Count < tokenCount)
                {
                    while (Params.Output.Count < tokenCount)
                    {
                        IGH_Param new_param = CreateParameter(GH_ParameterSide.Output, Params.Output.Count);
                        Params.RegisterOutputParam(new_param);
                    }
                }
                else if (Params.Output.Count > tokenCount)
                {
                    while (Params.Output.Count > tokenCount)
                    {
                        Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                    }
                }
                Params.OnParametersChanged();
                VariableParameterMaintenance();
                ExpireSolution(recompute);
            }
        }

        private void Menu_AutoCreateOutputs_Clicked(object sender, EventArgs e)
        {

            AutoCreateOutputs(true);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            var tokens = deserialized?.Children().ToList();
            if (tokens == null) return;
            for (int i = 0; i < Params.Output.Count; i++)
            {
                var token = tokens[i];
                var ttype = token.GetType();
                var prop = tokens.First();
                if (token is JProperty property)
                {
                    Params.Output[i].Name = $"{property.Name}";
                    Params.Output[i].NickName = $"{property.Name}";
                    Params.Output[i].Description = $"Data from property: {property.Name}";
                    Params.Output[i].MutableNickName = false;
                    if (property.Value is JArray)
                    {
                        Params.Output[i].Access = GH_ParamAccess.list;
                    }
                    else
                    {
                        Params.Output[i].Access = GH_ParamAccess.item;

                    }

                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("22786e9f-f9df-46cc-8815-c2eb104e3455");
    }
}
