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

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CyPhyMasterInterpreter.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CyPhyMasterInterpreter.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to import os
        ///import json
        ///import time
        ///import shutil
        ///import subprocess
        ///from glob import glob
        ///
        ///# remarks: should TestBenches inside the project-file be empty?
        ///
        ///DST_FOLDER = &quot;results_light&quot;
        ///
        ///class Exporter(object):
        ///
        ///    #   Default directories
        ///    dashboard_dir = &apos;./dashboard/&apos;
        ///    designs_dir = &apos;./designs/&apos;
        ///    design_space_dir = &apos;./design-space/&apos;
        ///    requirements_dir = &apos;./requirements/&apos;
        ///    results_dir = &apos;./results/&apos;
        ///    test_benches_dir = &apos;./test-benches/&apos;
        ///
        ///    #   Single files
        ///    meta_re [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string export_for_dashboard_scoring {
            get {
                return ResourceManager.GetString("export_for_dashboard_scoring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to import os
        ///import glob
        ///import zipfile
        ///import datetime
        ///import json
        ///
        ///
        ///class GatherStatJsonFiles(object):
        ///    &quot;&quot;&quot;
        ///    Class definition
        ///
        ///    &quot;&quot;&quot;
        ///    # hard-coded names
        ///    compiled_json_name = &apos;Stats.json&apos;
        ///    json_archive_name = &apos;Stats&apos;
        ///    right_now = &apos;&apos;
        ///    # paths
        ///    project_root = &apos;&apos;
        ///    path_to_stats_folder = &apos;&apos;
        ///    # lists
        ///    stat_json_paths = []
        ///    # dictionaries
        ///    main_json_dict = {}
        ///
        ///    def __init__(self):
        ///        &quot;&quot;&quot;
        ///        Constructor
        ///
        ///        &quot;&quot;&quot;
        ///        self.proj [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string gather_stat_json {
            get {
                return ResourceManager.GetString("gather_stat_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to __author__ = &apos;James&apos;
        ///
        ///import os
        ///import glob
        ///import shutil
        ///import datetime
        ///import tempfile
        ///import zipfile
        ///
        ///
        ///class GatherAllLogFiles(object):
        ///    &quot;&quot;&quot;
        ///    Class definition
        ///
        ///    &quot;&quot;&quot;
        ///    # hard-coded names
        ///    new_project_logs_folder_name = &apos;project_logs&apos;
        ///    job_manager_folder_name = &apos;META_JobManager&apos;
        ///    right_now = &apos;&apos;
        ///    new_logs_folder_name = &apos;&apos;
        ///
        ///    # paths
        ///    project_logs_folder_path = &apos;&apos;
        ///    project_root_dir = &apos;&apos;
        ///    project_results_folder_path = &apos;&apos;
        ///    temp_folder = &apos;&apos;
        ///    new [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string gather_all_logfiles {
            get {
                return ResourceManager.GetString("gather_all_logfiles", resourceCulture);
            }
        }
    }
}