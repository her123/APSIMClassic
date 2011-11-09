﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using CSGeneral;
using System.CodeDom.Compiler;
using ApsimFile;

public class FinishedException : Exception
   {
   public FinishedException(string msg)
      : base(msg)
      
      {
      }
   }

public class Engine
{
    /// <summary>
    /// Main program entry point.
    /// </summary>
    static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1)
                throw new Exception("Usage: FastApsim .ApsimFileName");

            Engine ApsimEngine = new Engine();
            ApsimEngine.Load(args[0]);
            ApsimEngine.Run();
            return 0;
        }
        catch (Exception err)
        {
            Console.WriteLine(err.Message);
            return 1;
        }
    }

    // Some data for our engine.
    private ModelInstance Simulation;

    /// <summary>
    /// Load a simulation from the specified file.
    /// </summary>
    public void Load(string FileName)
    {
        if (!File.Exists(FileName))
            throw new Exception("Cannot find file: " + FileName);

        XmlDocument Doc = new XmlDocument();
        Doc.Load(FileName);

        // Find the <simulation> element.
        XmlNode SimulationNode = XmlHelper.FindByType(Doc.DocumentElement, "Simulation");
        if (SimulationNode == null)
            throw new Exception("Cannot find a simulation to run");

        Simulation = CreateSimulationObjects(SimulationNode);
        Simulation.SetModelAPIs();
        Simulation Sim = (Simulation)Simulation.TheModel;
        Sim.InvokeInitialised();
        Simulation.UpdateValues();
    }

    /// <summary>
    /// Load a simulation from the specified XML
    /// </summary>
    public void LoadXml(string XML)
    {
        XmlDocument Doc = new XmlDocument();
        Doc.LoadXml(XML);

        Simulation = CreateSimulationObjects(Doc.DocumentElement);
        Simulation.SetModelAPIs();
        Simulation Sim = (Simulation)Simulation.TheModel;
        Sim.InvokeInitialised();
        Simulation.UpdateValues();
    }

    /// <summary>
    /// Run the simulation previously loaded into memory. Returns 1 on on receiving a finish.
    /// </summary>
    public int RunSingleTimeStep()
    {
        try
        {
            Simulation Sim = (Simulation)Simulation.TheModel;
            Sim.InvokeTimeStep();
        }
        catch (FinishedException exc)
        {
            Console.WriteLine(exc.Message);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Run entire simulation.
    /// </summary>
    private void Run()
    {
        Simulation.UpdateValues();
        while (RunSingleTimeStep() == 0) ;
    }

    public ModelAPIInterface API
    {
        get
        {
            return new ModelEnvironment(Simulation);
        }
    }

    /// <summary>
    /// Find an instance of a model with the specified name.
    /// Usefull for unit tests.
    /// </summary>
    public object FindModel(string ModelName)
    {
        return Simulation.FindModel(ModelName);
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Create instances of all objects specified by the XmlNode. Returns the top 
    /// level instance.
    /// </summary>
    private static ModelInstance CreateSimulationObjects(XmlNode Node)
    {
        ModelInstance RootInstance = CreateSimulationObjects(Node, null);
        ConnectInputsAndOutputs(RootInstance, RootInstance);
        ConnectEvents(RootInstance, RootInstance);
        RootInstance.ResolveRefs();
        CheckAllInputs(RootInstance);
        return RootInstance;
    }

    /// <summary>
    /// Internal recursive method to create instances of all objects specified by the 
    /// XmlNode. The parent instance is also passed in. Returns the top level instance.
    /// </summary>
    private static ModelInstance CreateSimulationObjects(XmlNode Node, ModelInstance ParentInstance)
    {
        // Create an instance that we'll return to our caller. Populate it with 
        ModelInstance ReturnInstance = new ModelInstance();

        ReturnInstance.ClassType = GetClassType(Node.Name);

        if (ReturnInstance.ClassType == null)
            throw new Exception("Cannot find a model class called: " + Node.Name);

        ReturnInstance.Node = Node;
        ReturnInstance.Parent = ParentInstance;
        ReturnInstance.TheModel = Activator.CreateInstance(ReturnInstance.ClassType);
        CollectVariablesAndEvents(ReturnInstance);

        if (!ReturnInstance.ModelHasXMLParam())
        {
            foreach (XmlNode Child in XmlHelper.ChildNodes(Node, ""))
            {
                // If the model has a [Param] with the same name as this child xml node
                // then don't go and try create a nested simulation object for this xml node.
                if (!ReturnInstance.ModelHasParam(Child.Name) && Child.Name.ToLower() != "summaryfile")
                    ReturnInstance.Children.Add(CreateSimulationObjects(Child, ReturnInstance));
            }
        }
        return ReturnInstance;
    }


    /// <summary>
    /// Go find and return the specified ClassName either in the current executing asssembly or
    /// in a dll of the same name as the class.
    /// </summary>
    private static Type GetClassType(string ClassName)
    {
        int PosPeriod = ClassName.IndexOf('.');
        if (PosPeriod == -1)
        {
            Type T = Assembly.GetExecutingAssembly().GetType(ClassName, false, true);
            if (T == null)
            {
                string AssemblyFileName = Path.Combine(Configuration.ApsimBinDirectory(), ClassName + "X.dll");
                if (File.Exists(AssemblyFileName))
                {
                    Assembly Dll = Assembly.LoadFile(AssemblyFileName);
                    return Dll.GetType(ClassName, false, true);
                }
            }
            return T;
        }
        else
        {
            // This is used in unit tests.
            string AssemblyFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + ClassName.Substring(0, PosPeriod) + ".dll";
            if (!File.Exists(AssemblyFileName))
                throw new Exception("Cannot find assembly file: " + AssemblyFileName);
            Assembly Dll = Assembly.LoadFile(AssemblyFileName);
            return Dll.GetType(ClassName.Substring(PosPeriod + 1));
        }
    }

    /// <summary>
    /// Collect all variables and events from the specified instance and all child instances.
    /// </summary>
    private static void CollectVariablesAndEvents(ModelInstance Inst)
    {
        // Collection all fields.
        foreach (FieldInfo Field in GetAllFields(Inst.TheModel.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (object Attribute in Field.GetCustomAttributes(false))
            {
                if (Attribute is Link)
                {
                    Link LinkAttribute = (Link)Attribute;
                    Inst.Refs.Add(new LinkField(Inst.TheModel, LinkAttribute.NamePath, Field, LinkAttribute.IsOptional));
                }
                else
                {
                    FieldVariable Var = new FieldVariable(Field, Inst.TheModel);
                    if (Var.Type.ToString() == "ModelAPIInterface")
                        Inst.ModelAPI = Var;
                    else if (Attribute is Input)
                        Inst.Inputs.Add(Var);
                    else if (Attribute is Output)
                        Inst.Outputs.Add(Var);
                    else
                        Inst.States.Add(Var);

                    // If this is a param then go find a value for it.
                    if (Attribute is Param)
                    {
                        // Get the parameter name taking any alias' into account.
                        string ParamName = Field.Name;
                        Param p = (Param)Attribute;
                        if (p.Alias != null)
                            ParamName = p.Alias;
                        Var.Name = ParamName;

                        // Get the parameter value.
                        if (ParamName.ToLower() == "name")
                            Var.Value = Inst.Name;
                        else if (Field.FieldType.Name == "XmlNode")
                            Var.Value = Inst.Node;
                        else
                        {
                            XmlNode ParamNode = XmlHelper.Find(Inst.Node, ParamName);
                            string ParamValue = null;
                            if (ParamNode != null)
                                ParamValue = ParamNode.InnerXml;
                            if (ParamValue == null)
                                throw new Exception("Cannot find a parameter value for: " + Field.Name + " for " + Inst.Name);
                            VariableBase ConvertedVariable = TypeConverter.CreateConverterIfNecessary(new StringVariable(ParamName, ParamValue), Var);
                            Var.Value = ConvertedVariable.Value;
                        }
                        Inst.Params.Add(Var);
                    }
                }
            }
        }
        // Collect all properties.
        foreach (PropertyInfo Property in Inst.TheModel.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (object Attribute in Property.GetCustomAttributes(false))
            {
                if (Attribute is Link)
                {
                }
                else
                {
                    PropertyVariable Var = new PropertyVariable(Property, Inst.TheModel);
                    if (Attribute is Input)
                        Inst.Inputs.Add(Var);
                    else if (Attribute is Output)
                        Inst.Outputs.Add(Var);
                    else
                        Inst.States.Add(Var);

                    // If this is a param then go find a value for it.
                    if (Attribute is Param)
                    {
                        string ParamValue = XmlHelper.Value(Inst.Node, Property.Name);
                        if (ParamValue == "")
                            throw new Exception("Cannot find a parameter value for: " + Property.Name);

                        VariableBase ConvertedVariable = TypeConverter.CreateConverterIfNecessary(new StringVariable(Property.Name, ParamValue), Var);
                        Var.Value = ConvertedVariable.Value;
                    }
                }
            }
        }
        // Look for all events in the model
        foreach (EventInfo Event in Inst.TheModel.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public))
            Inst.Publishers.Add(new EventPublisher(Event, Inst.TheModel));

        // Look for all event handlers in the model
        foreach (MethodInfo EventHandler in Inst.TheModel.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (object Attribute in EventHandler.GetCustomAttributes(false))
                if (Attribute is EventHandler)
                    Inst.Subscribers.Add(new EventSubscriber(EventHandler, Inst.TheModel));
        }

        // Now go and do the same for all child models.
        foreach (ModelInstance Child in Inst.Children)
        {
            CollectVariablesAndEvents(Child);
        }
    }

    /// <summary>
    /// Return all fields. The normal .NET reflection doesn't return private fields in base classes.
    /// This function does.
    /// </summary>
    public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
    {
        if (type == typeof(Object)) return new List<FieldInfo>();

        var list = GetAllFields(type.BaseType, flags);
        // in order to avoid duplicates, force BindingFlags.DeclaredOnly
        list.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
        return list;
    }

    /// <summary>
    /// Connect all inputs to outputs for the specified instance and all child instances.
    /// </summary>
    private static void ConnectInputsAndOutputs(ModelInstance Inst, ModelInstance RootInstance)
    {
        foreach (VariableBase Input in Inst.Inputs)
        {
            VariableBase Output = RootInstance.FindOutput(Input.Name);
            if (Output != null)
            {
                Output = TypeConverter.CreateConverterIfNecessary(Output, Input);
                Input.ConnectTo(Output);
            }
        }
        foreach (ModelInstance Child in Inst.Children)
        {
            ConnectInputsAndOutputs(Child, RootInstance);
        }
    }

    /// <summary>
    /// Connect all inputs to outputs for the specified instance and all child instances.
    /// </summary>
    private static void ConnectEvents(ModelInstance Inst, ModelInstance RootInstance)
    {
        foreach (EventSubscriber Subscriber in Inst.Subscribers)
        {
            EventPublisher Publisher = FindEventPublisher(RootInstance, Subscriber.Name);
            if (Publisher != null)
            {
                Publisher.ConnectTo(Subscriber);
            }
        }
        foreach (ModelInstance Child in Inst.Children)
        {
            ConnectEvents(Child, RootInstance);
        }
    }

    /// <summary>
    /// Recurse through all model instances and find an event publisher with the specified name.
    /// Returns null if not found.
    /// </summary>
    private static EventPublisher FindEventPublisher(ModelInstance RootInstance, string EventName)
    {
        foreach (EventPublisher Publisher in RootInstance.Publishers)
        {
            if (Publisher.Name.ToLower() == EventName.ToLower())
                return Publisher;
        }

        foreach (ModelInstance Child in RootInstance.Children)
        {
            EventPublisher Publisher = FindEventPublisher(Child, EventName);
            if (Publisher != null)
                return Publisher;
        }
        return null;
    }



    /// <summary>
    /// Check to make sure all inptus are connected to an output.
    /// </summary>
    private static void CheckAllInputs(ModelInstance Inst)
    {
        foreach (VariableBase Var in Inst.Inputs)
        {
            if (!Var.IsConnected)
                throw new Exception("Cannot find an input value for: " + Var.Name + " in " + Inst.Name);
        }
        foreach (ModelInstance Child in Inst.Children)
            CheckAllInputs(Child);
    }

}