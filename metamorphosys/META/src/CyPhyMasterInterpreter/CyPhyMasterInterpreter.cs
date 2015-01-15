/*
Copyright (C) 2013-2015 MetaMorph Software, Inc

Permission is hereby granted, free of charge, to any person obtaining a
copy of this data, including any software or models in source or binary
form, as well as any drawings, specifications, and documentation
(collectively "the Data"), to deal in the Data without restriction,
including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Data, and to
permit persons to whom the Data is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Data.

THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  

=======================
This version of the META tools is a fork of an original version produced
by Vanderbilt University's Institute for Software Integrated Systems (ISIS).
Their license statement:

Copyright (C) 2011-2014 Vanderbilt University

Developed with the sponsorship of the Defense Advanced Research Projects
Agency (DARPA) and delivered to the U.S. Government with Unlimited Rights
as defined in DFARS 252.227-7013.

Permission is hereby granted, free of charge, to any person obtaining a
copy of this data, including any software or models in source or binary
form, as well as any drawings, specifications, and documentation
(collectively "the Data"), to deal in the Data without restriction,
including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Data, and to
permit persons to whom the Data is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Data.

THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  
*/

namespace CyPhyMasterInterpreter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using GME.CSharp;
    using GME.MGA;
    using GME.MGA.Core;

    /// <summary>
    /// This class implements the necessary COM interfaces for a GME 
    /// interpreter component.
    /// </summary>
    [Guid(ComponentConfig.guid),
    ProgId(ComponentConfig.progID),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CyPhyMasterInterpreter : IMgaComponentEx, IGMEVersionInfo
    {
        /// <summary>
        /// Contains information about the GUI event that initiated the invocation.
        /// </summary>
        [ComVisible(false)]
        public enum ComponentStartMode
        {
            GME_MAIN_START = 0, 		// Not used by GME
            GME_BROWSER_START = 1,      // Right click in the GME Tree Browser window
            GME_CONTEXT_START = 2,		// Using the context menu by right clicking a model element in the GME modeling window
            GME_EMBEDDED_START = 3,		// Not used by GME
            GME_MENU_START = 16,		// Clicking on the toolbar icon, or using the main menu
            GME_BGCONTEXT_START = 18,	// Using the context menu by right clicking the background of the GME modeling window
            GME_ICON_START = 32,		// Not used by GME
            GME_SILENT_MODE = 128 		// Not used by GME, available to testers not using GME
        }

        /// <summary>
        /// This function is called for each interpreter invocation before Main.
        /// Don't perform MGA operations here unless you open a tansaction.
        /// </summary>
        /// <param name="project">The handle of the project opened in GME, for 
        /// which the interpreter was called.</param>
        public void Initialize(MgaProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
        }

        #region IMgaComponentEx Members

        private MgaGateway MgaGateway { get; set; }

        private GMEConsole GMEConsole { get; set; }

        private void Process(MgaFCO currentobj)
        {
            if (currentobj == null)
            {
                this.Logger.WriteError("Context is invalid. This component can be executed only if a valid context is open in the main editor (e.g.: Test Bench, Component Assembly).");
                return;
            }

            MgaGateway.PerformInTransaction(() =>
            {
                this.Logger.WriteInfo("Processing {0} [{1}] model", currentobj.Name, currentobj.MetaBase.Name);
            });

            using (var masterInterpreter = new CyPhyMasterInterpreterAPI(currentobj.Project, this.Logger))
            {
                masterInterpreter.IsInteractive = this.InteractiveMode;
                this.Logger.WriteDebug("Interactive mode: {0}", masterInterpreter.IsInteractive);

                Rules.ContextCheckerResult[] contextCheckerResults = null;

                // check context
                var checkerSuccess = false;
                try
                {
                    checkerSuccess = masterInterpreter.TryCheckContext(currentobj as MgaModel, out contextCheckerResults);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    this.Logger.WriteDebug(ex.ToString());
                    this.Logger.WriteError(ex.Message);
                    return;
                }

                List<Rules.ContextCheckerResult> sortedResults = contextCheckerResults.ToList();

                // sort results Passed, Failed, then alphabetically based on message.
                sortedResults.Sort((x, y) =>
                {
                    return x.Success == y.Success ?
                        x.Message.CompareTo(y.Message) :
                        y.Success.CompareTo(x.Success);
                });

                // Print Checker results
                MgaGateway.PerformInTransaction(() =>
                {
                    foreach (var result in sortedResults)
                    {
                        if (result.Success)
                        {
                            this.Logger.WriteCheckPassed(" <i><a href=\"mga:{0}\">{1}</a></i> {2}", result.Subject.ID, result.Subject.Name, result.Message);
                        }
                        else
                        {
                            this.Logger.WriteCheckFailed(" <i><a href=\"mga:{0}\">{1}</a></i> {2}", result.Subject.ID, result.Subject.Name, result.Message);
                        }
                    }
                });

                if (sortedResults.Any(x => x.Success == false))
                {
                    this.Logger.WriteError("Context is invalid see messeges above. Please fix the problems.");

                    bool controlWasHeld = false;
                    int VK_CONTROL = 0x11;
                    // if user held the control ignore the checker results ... for debugging purposes ONLY!
                    if ((bool)((GetKeyState(VK_CONTROL) & 0x8000) == 0x8000))
                    {
                        controlWasHeld = true;
                    }

                    if (controlWasHeld == false)
                    {
                        return;
                    }
                    else
                    {
                        this.Logger.WriteWarning("Bypassing checker results. This mode is strictly for debugging purposes.");
                    }
                }

                if (checkerSuccess == false)
                {
                    return;
                }

                // context is valid
                // show GUI for the user
                ConfigurationSelection selection = null;

                try
                {
                    selection = masterInterpreter.ShowConfigurationSelectionForm(currentobj as MgaModel);
                    MgaGateway.PerformInTransaction(() =>
                    {
                        this.Logger.WriteDebug("MasterExe command: CyPhyMasterExe.exe \"{0}\" \"{1}\" \"{2}\"", currentobj.Project.ProjectConnStr, GMELightObject.ShortenAbsPath(currentobj.AbsPath),
                            String.Join("\" \"", selection.SelectedConfigurations.Cast<IMgaFCO>().Select(f => GMELightObject.ShortenAbsPath(f.AbsPath)).ToArray()));
                    });
                }
                catch (ExecutionCanceledByUserException ex)
                {
                    this.Logger.WriteWarning("Operation was canceled by user. {0}", ex.Message);
                    return;
                }



                MasterInterpreterResult[] miResults = null;

                // Get a progress dialog
                using (var progressDialog = new ProgressDialog(masterInterpreter))
                {
                    masterInterpreter.MultipleConfigurationProgress += progressDialog.MultipleConfigurationProgressHandler;
                    masterInterpreter.SingleConfigurationProgress += progressDialog.SingleConfigurationProgressHandler;

                    if (masterInterpreter.IsInteractive)
                    {
                        progressDialog.ShowWithDisabledMainWindow();
                    }

                    try
                    {
                        miResults = masterInterpreter.RunInTransactionWithConfig(selection);
                    }
                    catch (AnalysisModelInterpreterConfigurationFailedException ex)
                    {
                        this.Logger.WriteWarning("Operation was canceled by user. {0}", ex.Message);
                    }
                    catch (ExecutionCanceledByUserException ex)
                    {
                        this.Logger.WriteWarning("Operation was canceled by user. {0}", ex.Message);
                    }
                }

                if (selection.OpenDashboard)
                {
                    masterInterpreter.OpenDashboardWithChrome();
                }

                masterInterpreter.WriteSummary(miResults);
            }
        }

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int Key);

        public CyPhyGUIs.GMELogger Logger { get; set; }

        public void InvokeEx(
                MgaProject project,
                MgaFCO currentobj,
                MgaFCOs selectedobjs,
                int param)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (selectedobjs == null)
            {
                throw new ArgumentNullException("selectedobjs");
            }

            this.InteractiveMode = this.Convert(param) != ComponentStartMode.GME_SILENT_MODE;

            try
            {
                GMEConsole = GMEConsole.CreateFromProject(project);
                MgaGateway = new MgaGateway(project);
                project.CreateTerritoryWithoutSink(out MgaGateway.territory);

                this.GMEConsole.Clear();
                System.Windows.Forms.Application.DoEvents();

                this.InteractiveMode = this.Convert(param) != ComponentStartMode.GME_SILENT_MODE;

                this.Logger = new CyPhyGUIs.GMELogger(project, this.GetType().Name);

                this.Logger.GMEConsoleLoggingLevel = Properties.Settings.Default.bVerboseLogging ?
                    CyPhyGUIs.SmartLogger.MessageType_enum.Debug :
                    CyPhyGUIs.SmartLogger.MessageType_enum.Info;

                //this.Logger.WriteDebug("Hello debug");
                //this.Logger.WriteError("Hello error {0} {1} {2}", 1, "string", true);
                //this.Logger.WriteInfo("Hello info");
                //this.Logger.WriteWarning("Hello WriteWarning");
                //this.Logger.WriteFailed("Hello WriteFailed");
                //this.Logger.WriteSuccess("Hello WriteSuccess");
                //this.Logger.WriteCheckFailed("Hello WriteCheckFailed");
                //this.Logger.WriteCheckPassed("Hello WriteCheckPassed");

                this.Logger.WriteInfo("Master Interpreter 2.0");
                System.Windows.Forms.Application.DoEvents();

                this.Logger.MakeVersionInfoHeader();

                // control was held
                this.Process(currentobj);

                foreach (var filename in this.Logger.LogFilenames)
                {
                    this.Logger.WriteInfo("Log file was generated here: <a href=\"file:///{0}\" target=\"_blank\">{1}</a>", Path.GetDirectoryName(filename), filename);
                }

                this.Logger.WriteInfo("Master Interpreter 2.0 finished");
            }
            finally
            {
                this.Logger.Dispose();
                System.Windows.Forms.Application.DoEvents();

                if (MgaGateway.territory != null)
                {
                    MgaGateway.territory.Destroy();
                }
                MgaGateway = null;
                project = null;
                currentobj = null;
                selectedobjs = null;
                GMEConsole = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private ComponentStartMode Convert(int param)
        {
            switch (param)
            {
                case (int)ComponentStartMode.GME_BGCONTEXT_START:
                    return ComponentStartMode.GME_BGCONTEXT_START;
                case (int)ComponentStartMode.GME_BROWSER_START:
                    return ComponentStartMode.GME_BROWSER_START;

                case (int)ComponentStartMode.GME_CONTEXT_START:
                    return ComponentStartMode.GME_CONTEXT_START;

                case (int)ComponentStartMode.GME_EMBEDDED_START:
                    return ComponentStartMode.GME_EMBEDDED_START;

                case (int)ComponentStartMode.GME_ICON_START:
                    return ComponentStartMode.GME_ICON_START;

                case (int)ComponentStartMode.GME_MAIN_START:
                    return ComponentStartMode.GME_MAIN_START;

                case (int)ComponentStartMode.GME_MENU_START:
                    return ComponentStartMode.GME_MENU_START;
                case (int)ComponentStartMode.GME_SILENT_MODE:
                    return ComponentStartMode.GME_SILENT_MODE;
            }
            return ComponentStartMode.GME_SILENT_MODE;
        }

        #region Component Information
        public string ComponentName
        {
            get { return GetType().Name; }
        }

        public string ComponentProgID
        {
            get
            {
                return ComponentConfig.progID;
            }
        }

        public componenttype_enum ComponentType
        {
            get { return ComponentConfig.componentType; }
        }
        public string Paradigm
        {
            get { return ComponentConfig.paradigmName; }
        }
        #endregion

        #region Enabling
        bool enabled = true;
        public void Enable(bool newval)
        {
            enabled = newval;
        }
        #endregion

        #region Interactive Mode
        protected bool interactiveMode = true;
        public bool InteractiveMode
        {
            get
            {
                return interactiveMode;
            }
            set
            {
                interactiveMode = value;
            }
        }
        #endregion

        #region Custom Parameters
        SortedDictionary<string, object> componentParameters = null;

        public object get_ComponentParameter(string Name)
        {
            if (Name == "type")
            {
                return "csharp";
            }

            if (Name == "path")
            {
                return GetType().Assembly.Location;
            }

            if (Name == "fullname")
            {
                return GetType().FullName;
            }

            object value;
            if (componentParameters != null &&
                    componentParameters.TryGetValue(Name, out value))
            {
                return value;
            }

            return null;
        }

        public void set_ComponentParameter(string Name, object pVal)
        {
            if (componentParameters == null)
            {
                componentParameters = new SortedDictionary<string, object>();
            }

            componentParameters[Name] = pVal;
        }
        #endregion

        #region Unused Methods
        // Old interface, it is never called for MgaComponentEx interfaces
        public void Invoke(MgaProject Project, MgaFCOs selectedobjs, int param)
        {
            throw new NotImplementedException();
        }

        // Not used by GME
        public void ObjectsInvokeEx(
                MgaProject Project,
                MgaObject currentobj,
                MgaObjects selectedobjs,
                int param)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IMgaVersionInfo Members

        public GMEInterfaceVersion_enum version
        {
            get { return GMEInterfaceVersion_enum.GMEInterfaceVersion_Current; }
        }

        #endregion

        #region Registration Helpers

        [ComRegisterFunctionAttribute]
        public static void GMERegister(Type t)
        {
            Registrar.RegisterComponentsInGMERegistry();

        }

        [ComUnregisterFunctionAttribute]
        public static void GMEUnRegister(Type t)
        {
            Registrar.UnregisterComponentsInGMERegistry();
        }

        #endregion

    }


}