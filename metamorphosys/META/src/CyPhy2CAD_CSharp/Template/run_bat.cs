﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CyPhy2CAD_CSharp.Template
{
    using System;
    
    
    #line 1 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class run_bat : run_batBase
    {
        public virtual string TransformText()
        {
            this.Write(@"
REM	The following system environment variable must be set:
REM	    PROE_ISIS_EXTENSIONS	// typically set to C:\Program Files\META\Proe ISIS Extensions
REM
REM	See ""C:\Program Files\META\Proe ISIS Extensions\0Readme - CreateAssembly.txt"" for the complete setup instructions.

set WORKING_DIR="".""
set ERROR_CODE=0

set ERROR_MSG=""""

FOR /F ""skip=2 tokens=2,*"" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query ""HKLM\software\META"" /v ""META_PATH""') DO set MetaPath=%%B

if exist TestBench_PreProcess.cmd (
cmd /c TestBench_PreProcess.cmd
)

set ERROR_CODE=%ERRORLEVEL%
if %ERRORLEVEL% NEQ 0 (
set ERROR_MSG=""Error from runCADJob.bat: Encountered error during execution of TestBench_PreProcess.cmd, error level is %ERROR_CODE%""
goto :ERROR_SECTION
)

""%MetaPath%\bin\Python27\Scripts\Python.exe"" ""%MetaPath%\bin\CAD\CADJobDriver.py"" -assembler ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Assembler));
            
            #line default
            #line hidden
            this.Write(" -mesher ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Mesher));
            
            #line default
            #line hidden
            this.Write(" -analyzer ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Analyzer));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
 if (Mode!=null) { 
            
            #line default
            #line hidden
            this.Write("-mode ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Mode));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 26 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 28 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
 if (CallDomainTool!=null) { 
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 30 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(CallDomainTool));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n");
            
            #line 32 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n\r\n");
            
            #line 35 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
 if (Automation) { 
            
            #line default
            #line hidden
            this.Write("@echo off\r\n\r\n\r\nRem ****************************\r\nREM Python Metric Update Script\r" +
                    "\nRem ****************************\r\n\r\nset RESULT_XML_FILE=");
            
            #line 43 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ComputedMetricsPath));
            
            #line default
            #line hidden
            this.Write(@"
set PY_SCRIPT_NAME=UpdateReportJson_CAD.py
set PY_SCRIPT=""%MetaPath%bin\CAD\%PY_SCRIPT_NAME%""
set PYTHONPATH=%PYTHONPATH%;%MetaPath%

if exist %PY_SCRIPT% goto  :PY_FOUND
@echo off
echo		Error: Could not find %PY_SCRIPT_NAME%.
echo		Your system is not properly configured to run %PY_SCRIPT_NAME%.
echo		Please see For instructions on how to configure your system, please see ""0Readme - CreateAssembly.txt""
echo		which is typically located at ""C:\Program Files\META\Proe ISIS Extensions""
set ERROR_CODE=2
set ERROR_MSG=""Error from runCADJob.bat: Could not find UpdateReportJson_CAD.py.""
goto :ERROR_SECTION

:PY_FOUND
""%MetaPath%\bin\Python27\Scripts\Python.exe"" %PY_SCRIPT% -m %RESULT_XML_FILE%

set ERROR_CODE=%ERRORLEVEL%
if %ERRORLEVEL% NEQ 0 (
set ERROR_MSG=""Error from runCADJob.bat: %PY_SCRIPT_NAME% encountered error during execution, error level is %ERROR_CODE%""
goto :ERROR_SECTION
)

if exist TestBench_PostProcess.cmd (
cmd /c TestBench_PostProcess.cmd
)

set ERROR_CODE=%ERRORLEVEL%
if %ERRORLEVEL% NEQ 0 (
set ERROR_MSG=""Error from runCADJob.bat: Encountered error during execution of TestBench_PostProcess.cmd, error level is %ERROR_CODE%""
goto :ERROR_SECTION
)
");
            
            #line 76 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\nexit 0\r\n\r\n:ERROR_SECTION\r\necho %ERROR_MSG% >>_FAILED.txt\r\necho \"\"\r\necho \"See Er" +
                    "ror Log: _FAILED.txt\"\r\nping -n 8 127.0.0.1 > nul\r\nexit /b %ERROR_CODE%\r\n\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 87 "C:\Users\snyako.ISIS\Desktop\META\src\CyPhy2CAD_CSharp\Template\run_bat.tt"
  
public string XMLFileName {get;set;}
public bool Automation {get;set;}
public string ComputedMetricsPath = "ComputedValues.xml";
public string AdditionalOptions = "";
public string Mesher { get; set; }
public string Analyzer { get; set; }
public string Assembler { get; set; }
public string Mode { get; set; }
public string CallDomainTool { get; set; }

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class run_batBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
