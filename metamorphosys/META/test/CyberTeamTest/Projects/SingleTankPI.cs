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
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;

namespace CyberTeamTest.Projects
{
    public class SingleTankPIFixture
    {
        internal string cyPhyMgaFile = null;
        internal string cyberMgaFile = null;

        public SingleTankPIFixture()
        {
            this.cyPhyMgaFile = Test.ImportXME2Mga("SingleTankPI", "SingleTankPI.xme");
            this.cyberMgaFile = Test.ImportXME2Mga("SingleTankPI", "SingleTankPIController.xme");
        }       
    }

    public class SingleTankPIModelImport
    {
        //[Fact]
        [Trait("Model", "SingleTankPI")]
        [Trait("ProjectImport/Open", "SingleTankPI")]
        public void ProjectXmeImport()
        {
            Assert.DoesNotThrow(() => { new SingleTankPIFixture(); });
        }
    }

    public partial class SingleTankPI : IUseFixture<SingleTankPIFixture>
    {
        internal string cyPhyMgaFile { get { return this.fixture.cyPhyMgaFile; } }
        internal string cyberMgaFile { get { return this.fixture.cyberMgaFile; } }
        private SingleTankPIFixture fixture { get; set; }

        public void SetFixture(SingleTankPIFixture data)
        {
            this.fixture = data;
        }

        //[Fact]
        //[Trait("Model", "SingleTankPI")]
        //[Trait("ProjectImport/Open", "SingleTankPI")]
        //public void CyPhyProjectXmeImport()
        //{
        //    Assert.True(File.Exists(cyPhyMgaFile), "Failed to generate the CyPhy mga.");
        //}

        //[Fact]
        //[Trait("Model", "SingleTankPIController")]
        //[Trait("ProjectImport/Open", "SingleTankPIController")]
        //public void CyberProjectXmeImport()
        //{
        //    Assert.True(File.Exists(cyberMgaFile), "Failed to generate the Cyber mga.");
        //}

        //[Fact]
        [Trait("Model", "SingleTankPI")]
        [Trait("ProjectImport/Open", "SingleTankPI")]
        public void CyPhyProjectMgaOpen()
        {
            var mgaReference = "MGA=" + cyPhyMgaFile;

            MgaProject project = new MgaProject();
            project.OpenEx(mgaReference, "CyPhyML", null);
            project.Close(true);
            Assert.True(File.Exists(mgaReference.Substring("MGA=".Length)));
        }

        //[Fact]
        [Trait("Model", "SingleTankPIController")]
        [Trait("ProjectImport/Open", "SingleTankPIController")]
        public void CyberProjectMgaOpen()
        {
            var mgaReference = "MGA=" + cyberMgaFile;

            MgaProject project = new MgaProject();
            project.OpenEx(mgaReference, "CyberComposition", null);
            project.Close(true);
            Assert.True(File.Exists(mgaReference.Substring("MGA=".Length)),
                string.Format("Error: could not open {0}!", new FileInfo(cyberMgaFile).FullName));
        }

        //[Fact]
        public void RunCyberToSLC()
        {
            var mgaReference = "MGA=" + cyberMgaFile;
            var mgaPath = new FileInfo(cyberMgaFile).Directory.FullName;
            MgaProject project = new MgaProject();

            try
            {
                
                project.OpenEx(mgaReference, "CyberComposition", true);
                project.BeginTransactionInNewTerr();

                var cyberObject = project.get_ObjectByPath("/@Components|kind=Components|relpos=0/@DiscreteWrapper|kind=SimulinkWrapper|relpos=0");
                Assert.True(cyberObject != null, "Error: could not locate SimulinkWrapper in " + cyberMgaFile + "!");

                GME.MGA.MgaFCO cyberObjectFCO = cyberObject as GME.MGA.MgaFCO;
                Assert.True(cyberObjectFCO != null, "Error: Simulink wrapper was found in " + cyberMgaFile + ", but could not be converted into an MgaFCO!");

                var cyberType = Type.GetTypeFromProgID("MGA.Interpreter.Cyber2SLC_CodeGen");
                Assert.True(cyberType != null, "Error: could not create Cyber2SLC_CodeGen Type from ProgID");

                IMgaComponentEx cyberInterpreter = Activator.CreateInstance(cyberType) as IMgaComponentEx;
                Assert.True(cyberInterpreter != null, "Error: created Cyber2SLC_CodeGen Type from ProgID, but this type is not an IMgaComponentEx!");

                cyberInterpreter.Initialize(project);
                cyberInterpreter.ComponentParameter["output_dir"] = mgaPath;
                cyberInterpreter.ComponentParameter["automation"] = "true";
                cyberInterpreter.ComponentParameter["console_messages"] = "off";
                cyberInterpreter.InvokeEx(project, cyberObjectFCO, null, 128);
            }
            finally
            {
                project.Close(true);
            }

            string generatedLibraryPath = Path.Combine(mgaPath, "Simulink", "DiscreteWrapper", "Release", "DiscreteWrapper", "DiscreteWrapper.lib");

            Assert.True(File.Exists(generatedLibraryPath), "Error: could not compile library from the generated cyber code!");
        }
    }
}