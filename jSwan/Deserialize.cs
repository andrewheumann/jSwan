using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using jSwan.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace jSwan
{
    public class Deserialize : LockableJSwanComponent, IGH_VariableParameterComponent, IDisposable
    {
        Dictionary<string, Type> uniqueChildProperties;


        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Deserialize()
          : base("Deserialize Json", "DeJson",
              "deserialize it")
        {
            UpdateMessage();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "The Json to parse", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }



        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("OutputsLocked", StructureLocked);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            var locked = false;
            if (reader.TryGetBoolean("OutputsLocked", ref locked))
            {
                StructureLocked = locked;
            }
            UpdateMessage();
            return base.Read(reader);
        }



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var json = "";
            DA.GetData("JSON", ref json);


            if (DA.Iteration == 0)
            {
                var allData = Params.Input.OfType<Param_String>()
               .First()
               .VolatileData.AllData(true)
               .OfType<GH_String>()
               .Select(s => DeserializeToObject(s.Value));
                if (!allData.Any())
                {
                    return;
                }
                var children = allData.SelectMany(d => d?.Children() ?? new JEnumerable<JToken>()).ToList();

                var allProperties = children.OfType<JProperty>();

                uniqueChildProperties = new Dictionary<string, Type>();

                foreach (var property in allProperties)
                {
                    if (!uniqueChildProperties.ContainsKey(property.Name))
                    {
                        uniqueChildProperties.Add(property.Name, property.Value.GetType());
                    }
                }

                var names = allProperties.Select(c => c.Name).Distinct().ToList();
            }

            if (uniqueChildProperties.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"No valid JSON properties found");
            }


            if (OutputMismatch() && !StructureLocked && DA.Iteration == 0)
            {
                OnPingDocument().ScheduleSolution(5, d =>
                {
                    AutoCreateOutputs(false);
                });
            }
            else
            {
                var deserialized = DeserializeToObject(json);
                if (deserialized == null)
                {
                    return;
                }
                var children = deserialized.Children().ToList();

                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child is JProperty property)
                    {

                        if (!Params.Output.Any(p => p.Name == property.Name))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "There's a property here that doesn't match the outputs...");
                            continue;
                        }


                        if (property.Value is JArray array)
                        {
                            DA.SetDataList(property.Name, array.Select(t => t.ToSimpleValue()));
                        }
                        else
                        {
                            DA.SetData(property.Name, property.Value.ToSimpleValue());

                        }
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Found a child that's not a property. IDK what do with that yet");
                    }
                }
            }
        }

        private JObject DeserializeToObject(string json)
        {


            try
            {
                json = TryGetJsonFromFile(json);
                var obj = JsonConvert.DeserializeObject(json);
                switch (obj)
                {
                    case JObject jobj:
                        return jobj;
                    case JArray jarr:
                    {
                        var newObj = new JObject {{"array", jarr}};
                        return newObj;
                    }
                    default:
                        return JsonConvert.DeserializeObject<JObject>(json);
                }
            }
            catch
            {
                return null;
            }
        }

        

        public override ComponentType Type => ComponentType.Deserialize;

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Match outputs", Menu_AutoCreateOutputs_Clicked);
            Menu_AppendItem(menu, "Lock Outputs", Menu_LockOutputs_Clicked, true, StructureLocked);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void Menu_LockOutputs_Clicked(object sender, EventArgs e)
        {
            StructureLocked = !StructureLocked;
            UpdateMessage();
        }

        private bool OutputMismatch()
        {
            var countMatch = uniqueChildProperties.Count() == Params.Output.Count;
            if (!countMatch) return true;

            foreach (var name in uniqueChildProperties)
            {
                if (!Params.Output.Select(p => p.NickName).Any(n => n == name.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public override void ClearData()
        {
            base.ClearData();
            uniqueChildProperties?.Clear();
            if (Params == null || !Params.Any()) return;
            foreach (var ghParam in Params)
            {
                ghParam?.ClearData();
            }
        }

        private void AutoCreateOutputs(bool recompute)
        {

            var tokenCount = uniqueChildProperties.Count();
            if (tokenCount == 0) return;

            if (OutputMismatch())
            {
                RecordUndoEvent("Output from Json");
                if (Params.Output.Count < tokenCount)
                {
                    while (Params.Output.Count < tokenCount)
                    {
                        var new_param = CreateParameter(GH_ParameterSide.Output, Params.Output.Count);
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
            UpdateMessage();
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
            var tokens = uniqueChildProperties;
            if (tokens == null) return;
            var names = tokens.Keys.ToList();
            for (var i = 0; i < Params.Output.Count; i++)
            {
                if (i > names.Count - 1) return;
                var name = names[i];
                var type = tokens[name];

                Params.Output[i].Name = $"{name}";
                Params.Output[i].NickName = $"{name}";
                Params.Output[i].Description = $"Data from property: {name}";
                Params.Output[i].MutableNickName = false;
                if (type.IsAssignableFrom(typeof(JArray)))
                {
                    Params.Output[i].Access = GH_ParamAccess.list;
                }
                else
                {
                    Params.Output[i].Access = GH_ParamAccess.item;

                }


            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override Bitmap Icon => Resources.Deserialize_JSON;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("22786e9f-f9df-46cc-8815-c2eb104e3455");


        public void Dispose()
        {
            ClearData();
            foreach (var ghParam in Params)
            {
                ghParam.ClearData();
            }
        }
    }
}
