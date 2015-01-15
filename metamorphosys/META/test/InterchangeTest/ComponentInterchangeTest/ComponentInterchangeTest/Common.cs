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
using System.IO;
using System.Diagnostics;
using GME.MGA;
using Xunit;

namespace ComponentInterchangeTest
{
    public abstract class ComponentInterchangeTestFixtureBase : IDisposable
    {
        public String testPath
        {
            get
            {
                return Path.GetDirectoryName(xmePath);
            }
        }
        public abstract String xmePath { get; }
        public MgaProject proj { get; private set; }

        public ComponentInterchangeTestFixtureBase()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(xmePath, out mgaConnectionString);
            var mgaPath = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(mgaPath)),
                        String.Format("{0} not found. Model import may have failed.", mgaPath));

            proj = new MgaProject();
            bool ro_mode;
            proj.Open("MGA=" + Path.GetFullPath(mgaPath), out ro_mode);
            proj.EnableAutoAddOns(true);
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
        }
    }

    class CommonFunctions
    {
        /// <summary>
        /// Unpack an XME file and return the path to the MGA
        /// </summary>
        /// <param name="xmePath">Path to the XME file to be unpacked</param>
        /// <returns>Path to MGA file</returns>
        public static string unpackXme(string xmePath,string suffix="")
        {
            if (!File.Exists(xmePath))
            {
                throw new FileNotFoundException(String.Format("{0} not found", xmePath));
            }
            string mgaPath = Path.Combine(
                Path.GetDirectoryName(xmePath),
                Path.GetFileNameWithoutExtension(xmePath) + suffix + ".mga");
            GME.MGA.MgaUtils.ImportXME(xmePath, mgaPath);
            
            return mgaPath;
        }

        public static int processCommon(Process process, bool redirect = false)
        {
            using (process)
            {
                process.StartInfo.UseShellExecute = false;

                if (redirect)
                {
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                }
                
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                if (redirect)
                {
                    char[] buffer = new char[4096];
                    while (true)
                    {
                        int read = process.StandardError.Read(buffer, 0, 4096);
                        if (read == 0)
                        {
                            break;
                        }
                        Console.Error.Write(buffer, 0, read);
                    }

                }
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        public static int runCyPhyComponentImporterCLRecursively(string mgaPath,string compFolderRoot)
        {
            var arguments = ("-r " + compFolderRoot + " " + mgaPath).Split(' ');

            return CyPhyComponentImporterCL.CyPhyComponentImporterCL.Main(arguments);
        }

        public static int runCyPhyComponentImporterCL(string mgaPath, string acmPath)
        {
            var arguments = (acmPath + " " + mgaPath).Split(' ');
            return CyPhyComponentImporterCL.CyPhyComponentImporterCL.Main(arguments);
        }

        public static int RunCyPhyMLComparator(string desired, string imported)
        {
            var path = Path.GetDirectoryName(desired);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(META.VersionInfo.MetaPath, "src", "bin", "CyPhyMLComparator.exe"),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            process.StartInfo.Arguments += desired;
            process.StartInfo.Arguments += " " + imported;

            return processCommon(process,true);
        }


        public static int runCyPhyComponentExporterCL(string mgaPath, string subfolder = null)
        {
            var testPath = Path.GetDirectoryName(mgaPath);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(META.VersionInfo.MetaPath, "src", "CyPhyComponentExporterCL", "bin", "Release", "CyPhyComponentExporterCL.exe"),
                    WorkingDirectory = testPath
                }
            };

            process.StartInfo.Arguments += " " + mgaPath;
            process.StartInfo.Arguments += " " + testPath;
            if (!String.IsNullOrWhiteSpace(subfolder))
            {
                process.StartInfo.Arguments += " -f " + subfolder;
            }

            return CommonFunctions.processCommon(process, true);
        }

        public static int RunXmlComparator(string exported, string desired)
        {
            string xmlComparatorPath = Path.Combine(
                META.VersionInfo.MetaPath,
                "test",
                "InterchangeTest",
                "InterchangeXmlComparator",
                "bin",
                "Release",
                "InterchangeXmlComparator.exe"
                );

            var process = new Process
            {
                StartInfo =
                {
                    FileName = xmlComparatorPath
                }
            };

            process.StartInfo.Arguments += String.Format(" -e {0} -d {1} -m Component", exported, desired);
            return CommonFunctions.processCommon(process, true);
        }

        public static MgaProject GetProject(String filename)
        {
            MgaProject result = null;

            if (filename != null && filename != "")
            {
                if (Path.GetExtension(filename) == ".mga")
                {
                    result = new MgaProject();
                    if (System.IO.File.Exists(filename))
                    {
                        Console.Out.Write("Opening {0} ... ", filename);
                        bool ro_mode;
                        result.Open("MGA=" + Path.GetFullPath(filename), out ro_mode);
                    }
                    else
                    {
                        Console.Out.Write("Creating {0} ... ", filename);
                        result.Create("MGA=" + filename, "CyPhyML");
                    }
                    Console.Out.WriteLine("Done.");
                }
                else
                {
                    Console.Error.WriteLine("{0} file must be an mga project.", filename);
                }
            }
            else
            {
                Console.Error.WriteLine("Please specify an Mga project.");
            }

            return result;
        }

    }
}