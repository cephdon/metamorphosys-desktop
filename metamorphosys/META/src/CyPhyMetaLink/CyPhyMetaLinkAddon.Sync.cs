﻿/*
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

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml.Serialization;

using GME.CSharp;
using GME.MGA;
using GME.MGA.Meta;
using GME;
using GME.MGA.Core;

using CyPhyML = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyMLClasses = ISIS.GME.Dsml.CyPhyML.Classes;

using ProtoBuf;
using edu.vanderbilt.isis.meta;
using MetaLinkProtobuf = edu.vanderbilt.isis.meta;
using System.Security;
using Newtonsoft.Json;
using System.Reflection;
using META;

namespace CyPhyMetaLink
{
    /// <summary>
    /// This file contains business logic independent functionality related to the general operation of Meta-Link.
    /// 
    /// </summary>
    partial class CyPhyMetaLinkAddon
    {
        public enum CreoOpenMode
        {
            OPEN_COMPONENT,             // Open Creo with an existing component
            OPEN_EMPTY,                 // Open an empty Creo session
        }

        [DllImport("user32.dll")]
        static extern IntPtr PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public delegate void FailureCallback();
        public FailureCallback StartupFailureCallback
        {
            get
            {
                return startupFailedCallback;
            }
            set
            {
                startupFailedCallback = value;
            }
        }

        private FailureCallback startupFailedCallback;

        private MgaAddOn addon;
        private bool componentEnabled = true;
        private bool handleEvents = true;
        GMEConsole GMEConsole { get; set; }
        public CyPhyMetaLinkBridgeClient.MetaLinkBridgeClient bridgeClient = new CyPhyMetaLinkBridgeClient.MetaLinkBridgeClient();

        // Data maintained for synced components
        public readonly Dictionary<string, SyncedComponentData> syncedComponents = new Dictionary<string, SyncedComponentData>();

        // right now, support sycing only one ComponentAssembly
        public string AssemblyID { get; set; }
        protected Dictionary<string, string> designIdToCadAssemblyXml = new Dictionary<string, string>();

        private Dictionary<string, Action<MetaLinkProtobuf.Edit>> noticeActions = new Dictionary<string, Action<Edit>>();

        //Dictionary<String, Edit> componentEditMessages = new Dictionary<String, Edit>();

        // The last guid the executable started with
        private string LastStartedGuid;

        private Process _metalinkBridge;
        public Process metalinkBridge
        {
            set
            {
                if (_metalinkBridge != null)
                {
                    if (_metalinkBridge.MainWindowHandle != IntPtr.Zero)
                    {
                        int WM_CLOSE = 0x0010;
                        PostMessage(metalinkBridge.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }
                    _metalinkBridge.Dispose();
                    _metalinkBridge = null;
                }
                _metalinkBridge = value;
            }
            get
            {
                return _metalinkBridge;
            }
        }

        System.Windows.Forms.Control SyncControl;
        ConcurrentQueue<MetaLinkProtobuf.Edit> queuedMessages = new ConcurrentQueue<Edit>();

        public CyPhyMetaLinkAddon()
        {
            StartupFailureCallback = ExeStartupFailed;
        }

        public void ConnectionClosed(Exception e)
        {
            SyncControl.BeginInvoke((System.Action)delegate
            {
                if (addon != null)
                {
                    GMEConsole console = GMEConsole.CreateFromProject(addon.Project);
                    console.Error.WriteLine("Connection to MetaLink lost.");
                    //componentEditMessages.Clear();
                    interests.Clear();
                    AssemblyID = null;
                    metalinkBridge = null;
                    if (console.gme != null)
                    {
                        // Marshal.FinalReleaseComObject(console.gme); is this needed?
                    }
                    Enable(false);
                    if (syncedComponents.Count > 0)
                    {
                        bool inTx = ((addon.Project.ProjectStatus & 8) != 0);
                        if (inTx == false)
                        {
                            addon.Project.BeginTransactionInNewTerr();
                        }
                        try
                        {
                            foreach (string id in syncedComponents.Keys)
                            {
                                ISIS.GME.Common.Interfaces.Base model = CyphyMetaLinkUtils.GetComponentAssemblyByGuid(addon.Project, id);
                                if (model == null)
                                {
                                    model = CyphyMetaLinkUtils.GetComponentByAvmId(addon.Project, id);
                                }
                                if (model != null)
                                {
                                    HighlightInTree(model, 0);
                                }
                            }
                        }
                        finally
                        {
                            if (inTx == false)
                            {
                                addon.Project.AbortTransaction();
                            }
                        }
                        syncedComponents.Clear();
                    }
                }
            });
        }
        public void EditMessageReceived(MetaLinkProtobuf.Edit message)
        {
            queuedMessages.Enqueue(message);
            SyncControl.BeginInvoke((System.Action)delegate
            {
                if (addon != null)
                {
                    /*if ((addon.Project.ProjectStatus & 8) != 0) // in tx
                    {
                        GMEConsole.Warning.WriteLine("Message received during transaction, ignoring.");
                        // In GlobalEvent, we will post a message to process the queue
                    }
                    else
                    {*/
                        ProcessQueuedEditMessages();
                    //}
                }
            });
        }

        private void ProcessQueuedEditMessages()
        {
            // assert SyncControl.InvokeRequired == false
            Edit msg;
            while (queuedMessages.TryDequeue(out msg))
            {
                try
                {
                    ProcessEditMessage(msg);
                }
                catch (Exception e)
                {
                    GMEConsole.Error.WriteLine("Error processing MetaLink message: " + e.Message);
                    // FIXME: who knows what state we're in now. Should we disconnect from MetaLink?
                    /*
                     * TODO: send debug info back to ISIS
                    string dllPath = Assembly.GetExecutingAssembly().Location;
                    Dictionary<string, string> debugInfo = new Dictionary<string,string>();
                    debugInfo["edit_mode"] = msg.editMode.ToString();
                    
                    debugInfo["dllPath"] = Assembly.GetExecutingAssembly().Location;
                    debugInfo["dllLastModified"] = new FileInfo(dllPath).LastWriteTimeUtc.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
                    debugInfo["dllVersion"] = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ToString();
                    debugInfo["stack"] = e.StackTrace;
                    int i = 2;
                    Exception inner = e.InnerException;
                    while (inner != null)
                    {
                        debugInfo[string.Format("stack{0}", i)] = inner.StackTrace;
                        inner = e.InnerException;
                    }


                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://symbols.isis.vanderbilt.edu/META_exception.php");
                    //myRequest.
                    // fire and forget
                    ThreadPool.QueueUserWorkItem(o => {
                        try
                        {
                            myRequest.GetResponse();
                        }
                        catch (Exception)
                        {
                        }
                    });
                    */
                }
            }
        }
        private void StartAssemblyExe(CreoOpenMode mode, string guid, bool isComponentAssembly, string exeparams, string workingDir = null)
        {
            SyncedComponentData syncedCompData = null;
            if (!syncedComponents.TryGetValue(guid, out syncedCompData))
            {
                GMEConsole.Warning.WriteLine("StartAssemblyExe(): Can't find guid " + guid);
                return;
            }

            string proeIsisExtensionsDir = System.Environment.GetEnvironmentVariable("PROE_ISIS_EXTENSIONS");
            string createAssemblyExe = Path.Combine(proeIsisExtensionsDir ?? "", "bin", "CADCreoParametricMetaLink.exe");
            if (File.Exists(createAssemblyExe) == false)
            {
                GMEConsole.Error.WriteLine("CADCreoParametricMetaLink.exe could not be found");
                return;
            }
            if (workingDir == null)
            {
                workingDir = syncedCompData.WorkingDir;
            }

            try
            {
                File.Copy(Path.Combine(GetProjectDir(), "constraintdata.xml"), Path.Combine(workingDir, "constraintdata.xml"));
            }
            catch (Exception)
            {
                //  not an issue
            }

            string logfile = Path.Combine(GetProjectDir(), "log", Path.ChangeExtension("MetaLink_CreoCreateAssembly_" + Path.GetRandomFileName(), "log"));
            string arguments = String.Format("-v inform -g -s 127.0.0.1:15150 -w \"{0}\" -l \"{1}\" {2}", workingDir, logfile, exeparams);
            arguments += " -d " + guid;

            ProcessStartInfo info = new ProcessStartInfo()
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = createAssemblyExe,
                Arguments = arguments
                // TODO -p ?
            };
            if (mode == CreoOpenMode.OPEN_EMPTY)
            {
                info.Arguments += " -m passive";
            }
            else if (!isComponentAssembly)
            {
                info.Arguments += " -m component";
            }
            if (Configuration.ConfigProFile.Length > 0)
            {
                info.Arguments += " -c " + Configuration.ConfigProFile;
            }
            if (!string.IsNullOrWhiteSpace(syncedCompData.AuxDir))
            {
                info.Arguments += String.Format(" -a \"{0}\"", syncedCompData.AuxDir);
            }
            Process createAssembly = new Process();
            createAssembly.StartInfo = info;
            StringBuilder stdoutData = new StringBuilder();
            StringBuilder stderrData = new StringBuilder();
            createAssembly.OutputDataReceived += (o, args) =>
            {
                lock (stdoutData)
                {
                    stdoutData.Append(args.Data);
                    stdoutData.Append(System.Environment.NewLine);
                }
            };
            createAssembly.ErrorDataReceived += (o, args) =>
            {
                lock (stderrData)
                {
                    stderrData.Append(args.Data);
                    stderrData.Append(System.Environment.NewLine);
                }
            };
            createAssembly.EnableRaisingEvents = true;
            createAssembly.Exited += (o, args) =>
            {
                lock (stderrData)
                {
                    if (GMEConsole != null && createAssembly.ExitCode != 0)
                    {
                        string errlog = "CADCreoParametricCreateAssembly_err.log";
                        StreamWriter writer = new StreamWriter(errlog);
                        writer.Write(stderrData.ToString());
                        writer.Flush();
                        writer.Close();
                        GMEConsole.Error.WriteLine(String.Format("CADCreoParametricCreateAssembly exited with code {0}, the logfile is {1}", createAssembly.ExitCode, errlog));
                        SyncControl.Invoke(startupFailedCallback);
                    }
                }
            };
            LastStartedGuid = guid;
            createAssembly.Start();
            createAssembly.BeginOutputReadLine();
            createAssembly.BeginErrorReadLine();
            createAssembly.StandardInput.Close();
            GMEConsole.Info.WriteLine("Creo is starting, the logfile is at: <a href=\"file:///{0}\" target=\"_blank\">{0}</a>", Path.Combine(workingDir, "log", logfile));
            ShowStartupDialog(true);
            // createAssembly.WaitForExit(10 * 1000);
        }

        // Callback invoked if the executable startup has failed
        public void ExeStartupFailed()
        {
            // Unhighlight tree and remove item from synced components
            if (LastStartedGuid != null)
            {
                try
                {
                    addon.Project.BeginTransactionInNewTerr();
                    ISIS.GME.Common.Interfaces.Base model = CyphyMetaLinkUtils.GetComponentAssemblyByGuid(addon.Project, LastStartedGuid);
                    if (model == null)
                    {
                        model = CyphyMetaLinkUtils.GetComponentByAvmId(addon.Project, LastStartedGuid);
                        SendDisinterest(ComponentUpdateTopic, LastStartedGuid);
                    }
                    if (model != null)
                    {
                        AssemblyID = null;
                        HighlightInTree(model, 0);
                        SendDisinterest(CadAssemblyTopic, LastStartedGuid);

                    }
                    syncedComponents.Remove(LastStartedGuid);
                }
                finally
                {
                    addon.Project.AbortTransaction();
                }
            }
            LastStartedGuid = null;
            ShowStartupDialog(false);
        }

        private void ShowStartupDialog(bool show)
        {
            if (TestMode) return;
            if (show)
                StartupDialog.ShowDialog();
            else
                StartupDialog.Hide();
        }

        public bool CloseConnection()
        {
            bool ret = bridgeClient.CloseConnection();
            bridgeClient = new CyPhyMetaLinkBridgeClient.MetaLinkBridgeClient();
            return ret;
        }

        public bool EstablishConnection()
        {
            bool ret = bridgeClient.EstablishConnection(EditMessageReceived, ConnectionClosed);
            if (ret)
            {
                SendInterest(null, ComponentCreateTopic);
            }
            return ret;
        }


        public void CallCyPhy2CADWithTransaction(MgaProject project, MgaFCO toplevelAssembly, int param)
        {
            if (GMEConsole == null)
            {
                GMEConsole = GMEConsole.CreateFromProject(project);
            }
            Dictionary<MgaFCO, MgaFCO> objectMap = new Dictionary<MgaFCO, MgaFCO>();
            try
            {
                project.BeginTransactionInNewTerr(transactiontype_enum.TRANSACTION_NON_NESTED);
                handleEvents = false;
                GenerateCADAssemblyXml(project, toplevelAssembly, param);

                try
                {
                    HighlightInTree(toplevelAssembly, 1);
                }
                catch (Exception e)
                {
                    GMEConsole.Error.Write("Error during processing the model: " + e.Message);
                    // Don't do anything, there won't be highlight
                }
            }
            finally
            {
                project.AbortTransaction();
                handleEvents = true;
            }
        }

        private void PrepareMetaLinkData(CyPhy2CAD_CSharp.MetaLinkData data, CyPhyML.ComponentAssembly assembly, String prefix)
        {
            Dictionary<string, CyPhyML.ComponentRef> instanceguids = new Dictionary<string, CyPhyML.ComponentRef>();
            CyphyMetaLinkUtils.CollectInstanceGUIDs(instanceguids, "", assembly);
            if (assembly.Children.ComponentCollection.Any())
            {
                GMEConsole.Warning.WriteLine("Assembly contains components, Meta-Link may not work properly on this model.");
            }
            //List<CyPhyML.ComponentRef> comps = CyphyMetaLinkUtils.CollectComponentRefsRecursive(assembly);
            foreach (var item in instanceguids)
            {
                CyPhy2CAD_CSharp.MetaLinkData.Component mcomp = new CyPhy2CAD_CSharp.MetaLinkData.Component();
                mcomp.ID = item.Key;
                mcomp.AvmID = (item.Value.AllReferred as CyPhyML.Component).Attributes.AVMID;
                data.Components.Add(mcomp);
                foreach (CyPhyML.Connector conn in (item.Value.AllReferred as CyPhyML.Component).Children.ConnectorCollection)
                {
                    CyPhy2CAD_CSharp.MetaLinkData.Connector mconn = new CyPhy2CAD_CSharp.MetaLinkData.Connector();
                    mconn.ID = conn.Guid.ToString();
                    mconn.DisplayName = conn.Name;
                    mcomp.Connectors.Add(mconn);
                    foreach (CyPhyML.CADDatum d in conn.Children.CADDatumCollection)
                    {
                        CyPhy2CAD_CSharp.MetaLinkData.Datum md = new CyPhy2CAD_CSharp.MetaLinkData.Datum();
                        md.ID = d.Guid.ToString();
                        md.DisplayName = d.Name;
                        mconn.Datums.Add(md);
                    }
                }
            }
        }

        // Verify ComponentAssembly model so that it's Meta-Link compatible
        private bool VerifyAssembly(CyPhyML.ComponentAssembly assembly, List<string> errorList)
        {
            // An assembly should only contain component references
            if (assembly.Children.ComponentCollection.Any())
            {
                foreach (var comp in assembly.Children.ComponentCollection)
                {
                    errorList.Add("The assembly contains a component:" + comp.Name + ". Meta-Link will only work with references.");
                }
            }
            if (assembly.Children.ComponentAssemblyCollection.Any())
            {
                foreach (var comp in assembly.Children.ComponentCollection)
                {
                    errorList.Add("The assembly contains a component assembly:" + comp.Name + ". Meta-Link will only work with references.");
                }
            }
            foreach (var cref in assembly.Children.ComponentRefCollection)
            {
                if (cref.AllReferred == null)
                {
                    errorList.Add("The assembly contains a null-reference:" + cref.Name);
                }
            }
            VerifyCADResources(assembly, errorList);
            return errorList.Count == 0;
        }

        private void VerifyCADResources(CyPhyML.Component component, List<string> errorList)
        {
            string comppath = component.GetDirectoryPath(ComponentLibraryManager.PathConvention.ABSOLUTE);
            foreach (var res in component.Children.ResourceCollection)
            {
                if (CyphyMetaLinkUtils.IsCADResource(res))
                {
                    string fullpath = Path.Combine(comppath, res.Attributes.Path);
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(fullpath), Path.GetFileName(fullpath) + "*");
                    if (files.Length == 0)
                    {
                        errorList.Add("Missing CAD resource file: " + fullpath);
                    }
                }
            }
        }

        private void VerifyCADResources(CyPhyML.ComponentAssembly assembly, List<string> errorList)
        {
            var componentlist = CyphyMetaLinkUtils.CollectComponentsRecursive(assembly);
            foreach (var component in componentlist)
            {
                VerifyCADResources(component, errorList);
            }
        }

        private void GenerateCADAssemblyXml(MgaProject project, MgaFCO currentobj, int param)
        {
            int origPrefs = project.Preferences;
            try
            {
                project.Preferences = project.Preferences | (int)GME.MGA.preference_flags.MGAPREF_IGNORECONNCHECKS | (int)GME.MGA.preference_flags.MGAPREF_FREEINSTANCEREFS;
                CyPhyML.ComponentAssembly assembly = CyPhyMLClasses.ComponentAssembly.Cast(currentobj);
                List<string> errorList = new List<string>();
                if (!VerifyAssembly(assembly, errorList))
                {
                    System.Windows.Forms.MessageBox.Show("Model verification failed for Meta-Link and it may not work properly for this model.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    foreach (string s in errorList)
                        GMEConsole.Warning.WriteLine(s);
                }
                SyncedComponentData syncedCompData;
                if (!syncedComponents.TryGetValue(assembly.Guid.ToString(), out syncedCompData))
                {
                    throw new Exception("Internal error in GenerateCADAssemblyXml, can't find data for guid " + assembly.Guid);
                }

                HashSet<CyPhyML.Component> comps = CyphyMetaLinkUtils.CollectComponentsRecursive(assembly);

                // Prepare Meta-Link specific data based on the original model
                CyPhy2CAD_CSharp.MetaLinkData metaLinkData = new CyPhy2CAD_CSharp.MetaLinkData();
                try
                {
                    PrepareMetaLinkData(metaLinkData, assembly, "");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error during collecting metalink data." + ex.Message);
                }
                
                DateTime t1 = DateTime.Now;
                // call elaborator and expand the references
                // Please note that elaborator will change "currentobj"'s internal structure
                Type t = Type.GetTypeFromProgID("MGA.Interpreter.CyPhyElaborateCS");
                if (t == null)
                {
                    throw new NullReferenceException("Cannot find MGA.Interpreter.CyPhyElaborate. Is it registered?");
                }
                IMgaComponentEx elaborator = Activator.CreateInstance(t) as IMgaComponentEx;
                elaborator.Initialize(project);
                elaborator.ComponentParameter["automated_expand"] = "true";
                elaborator.ComponentParameter["console_messages"] = "off";

                elaborator.InvokeEx(project, currentobj, (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs")), param);
                TimeSpan ts = DateTime.Now-t1;
                GMEConsole.Info.WriteLine("Elaborator was running for " + ts.TotalMilliseconds + "ms");

                CyPhy2CAD_CSharp.CyPhy2CADSettings cadSettings = new CyPhy2CAD_CSharp.CyPhy2CADSettings();
                cadSettings.AuxiliaryDirectory = syncedCompData.AuxDir;
                cadSettings.OutputDirectory = syncedCompData.WorkingDir;
                cadSettings.MetaLink = true;

                CyPhy2CAD_CSharp.Logger.Instance.Reset();
                CyPhy2CAD_CSharp.CADFlatDataCreator datacreator = new CyPhy2CAD_CSharp.CADFlatDataCreator(cadSettings.OutputDirectory, GetProjectDir(), metalink: true);

                datacreator.CreateFlatData(assembly);
                CyPhy2CAD_CSharp.DataRep.CADContainer cadcontainer = datacreator.CreateCADDataContainer(assembly.Guid.ToString(), CyPhy2CAD_CSharp.UtilityHelpers.CleanString2(assembly.Name));

                cadcontainer.MergeAssemblies();

                // IF this is an empty design, add a root component
                if (cadcontainer.assemblies.Count == 0)
                {
                    cadcontainer.AddRootComponent(assembly);
                }

                cadcontainer.InjectMetaLinkData(assembly, metaLinkData);


                CyPhy2CAD_CSharp.TestBenchModel.TestBenchBase testbenchbase = new CyPhy2CAD_CSharp.TestBenchModel.TestBenchBase(cadSettings, cadSettings.OutputDirectory, GetProjectDir());
                testbenchbase.cadDataContainer = cadcontainer;
                testbenchbase.CollectDirectories();

                testbenchbase.GenerateCADXMLOutput();
                testbenchbase.GenerateScriptFiles();
                bool stop = CyPhy2CAD_CSharp.Logger.Instance.ErrorCnt > 0;
                CyPhy2CAD_CSharp.Logger.Instance.DumpLog(GMEConsole, "");
                if (stop)
                {
                    throw new Exception("There were errors during processing the model. Please look at the console log and fix these.");
                }

                string CADAssembly = File.ReadAllText(Path.Combine(cadSettings.OutputDirectory, CyPhy2CAD_CSharp.TestBenchModel.TestBenchBase.CADAssemblyFile));
                string designId = new Guid(currentobj.GetGuidDisp()).ToString("D");
                SendInterest(null, CadAssemblyTopic, designId);
                SendManifestInterest();
                //SendComponentManifest();
                SendInterest(null, ConnectTopic);
                SendInterest(null, ResyncTopic, designId);
                foreach (var comp in comps)
                    SendInterest(null, ComponentUpdateTopic, comp.Attributes.AVMID);
                SaveCadAssemblyXml(CADAssembly, designId);
            }
            finally
            {
                project.Preferences = origPrefs;
            }
        }

        public void HighlightInTree(ISIS.GME.Common.Interfaces.Base item, int highlight)
        {
            if (TestMode) return;
            HighlightInTree(item.Impl, highlight);
        }

        public void HighlightInTree(IMgaObject item, int highlight)
        {
            if (TestMode) return;
            if (GMEConsole.gme == null) // e.g. unit tests
            {
                return;
            }
            object browser = GetBrowser();
            if (browser != null)
            {
                try
                {
                    browser.GetType().InvokeMember("HighlightItem", System.Reflection.BindingFlags.InvokeMethod, null, browser, new object[] { item, highlight });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
        }

        public void StartCreoEmpyMode()
        {
            SyncedComponentData cdata = new SyncedComponentData()
            {
                Type = SyncedComponentData.EditType.Component,
                WorkingDir = GetProjectDir()
            };
            string tmpid = Guid.NewGuid().ToString();
            syncedComponents.Add(tmpid, cdata);
            StartAssemblyExe(CreoOpenMode.OPEN_EMPTY, tmpid, false, "");
            SendInterest(null, ComponentCreateTopic, tmpid);
        }

        public void StartEditingComponent(CyPhyML.Component component, MgaFCO cadModel, bool createNewComponent)
        {
            addon.Project.BeginTransactionInNewTerr();
            string avmid = component.Attributes.AVMID;
            SyncedComponentData syncedCompData;
            try
            {
                if (syncedComponents.TryGetValue(avmid, out syncedCompData))
                {
                    GMEConsole.Error.WriteLine("This component is already open in Creo, AVMID: " + component.Attributes.AVMID);
                    return;
                }
                else
                {
                }
                List<string> errorList = new List<string>();
                VerifyCADResources(component, errorList);
                if (errorList.Count != 0)
                {
                    foreach (var err in errorList)
                    {
                        GMEConsole.Error.WriteLine(err);
                    }
                    System.Windows.Forms.MessageBox.Show("Errors found in component. Can't open in Meta-Link. Please check the log.");
                    return;
                }
                CyPhyML.Component newComponent = CyphyMetaLinkUtils.CopyComponent(component, createNewComponent);
                avmid = newComponent.Attributes.AVMID;
                if (createNewComponent && GMEConsole.gme != null)
                {
                    // META-2109: open copy in model editor, select in Browser
                    try
                    {
                        string newComponentID = newComponent.ID;
                        bool closeModelEditor = false;
                        if (GMEConsole.gme.OleIt != null)
                        {
                            closeModelEditor = GMEConsole.gme.OleIt.MgaModel.ID == component.ID;
                        }
                        SyncControl.BeginInvoke((System.Action)delegate
                        {
                            try
                            {
                                if (closeModelEditor)
                                {
                                    GMEConsole.gme.OleIt.Close();
                                }
                                GMEConsole.gme.ShowFCO((MgaFCO)newComponent.Impl, false);
                                object browser = GetBrowser();
                                if (browser != null)
                                {
                                    browser.GetType().InvokeMember("FocusItem", System.Reflection.BindingFlags.InvokeMethod, null, browser, new object[] { newComponentID });
                                }
                            }
                            catch (Exception) { }
                        });
                    }
                    catch (Exception) { }
                }
                HighlightInTree(newComponent, 1);
            }
            finally
            {
                addon.Project.CommitTransaction();
            }

            syncedCompData = new SyncedComponentData()
            {
                Type = SyncedComponentData.EditType.Component
            };
            syncedComponents.Add(avmid, syncedCompData);

            if (!SendInterest(StartComponentEditAction, ComponentUpdateTopic, avmid))
            {
                StartComponentEdit(avmid, true);
            }
            SendInterest(null, ComponentAnalysisPointTopic, avmid);
        }

        private Edit CreateComponentEditMessage(CyPhyML.Component component, CyPhyML.CADModel cadModel, bool clear = false)
        {
            string cadModelRelativePath = "";
            if (false == cadModel.TryGetResourcePath(out cadModelRelativePath)
                || cadModelRelativePath == "")
            {
                // TODO log
                return null;
            }

            var message = new Edit()
            {
                editMode = MetaLinkProtobuf.Edit.EditMode.POST,
                guid = Guid.NewGuid().ToString(),
                //sequence = 0,
            };
            message.origin.Add(GMEOrigin);
            message.topic.Add(ComponentUpdateTopic);
            message.topic.Add(component.Attributes.AVMID);
            if (clear)
            {
                message.actions.Add(new edu.vanderbilt.isis.meta.Action() { actionMode = MetaLinkProtobuf.Action.ActionMode.CLEAR });
            }
            edu.vanderbilt.isis.meta.Action action = new edu.vanderbilt.isis.meta.Action();
            message.actions.Add(action);
            action.actionMode = MetaLinkProtobuf.Action.ActionMode.INSERT;
            action.environment.Add(new edu.vanderbilt.isis.meta.Environment()
            {
                name = SearchPathStr,
            });
            AddSearchPathToEnvironment(component, action.environment[0]);
            action.payload = new Payload();
            CADComponentType cadComponent = new CADComponentType()
            {
                AvmComponentID = component.Attributes.AVMID,
                CADModelID = CyphyMetaLinkUtils.GetResourceID(cadModel), // Using CADModelID to transport this information (the resource id)
                Name = Path.GetFileNameWithoutExtension(cadModelRelativePath), // the partial creo file name (less .prt or .asm)
                Type = (Path.GetExtension(cadModelRelativePath).EndsWith(".prt") || Path.GetExtension(cadModelRelativePath).EndsWith(".PRT"))?"PART":"ASSEMBLY"
            };
            foreach (var connector in component.Children.ConnectorCollection)
            {
                cadComponent.Connectors.Add(new ConnectorType() { ID = connector.Guid.ToString(), DisplayName = connector.Name });
            }
            foreach (var datum in cadModel.Children.CADDatumCollection)
            {
                cadComponent.Datums.Add(new ConnectorDatumType() { ID = datum.Attributes.DatumName, DisplayName = datum.Name });
            }
            action.payload.components.Add(cadComponent);
            return message;
        }

        private object GetBrowser()
        {
            object browser = null;
            foreach (dynamic panel in GMEConsole.gme.Panels)
            {
                if (panel.Name == "Browser")
                {
                    browser = panel.Interface;
                    break;
                }
            }
            return browser;
        }


    }
}