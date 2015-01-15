# Copyright (C) 2013-2015 MetaMorph Software, Inc

# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this data, including any software or models in source or binary
# form, as well as any drawings, specifications, and documentation
# (collectively "the Data"), to deal in the Data without restriction,
# including without limitation the rights to use, copy, modify, merge,
# publish, distribute, sublicense, and/or sell copies of the Data, and to
# permit persons to whom the Data is furnished to do so, subject to the
# following conditions:

# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Data.

# THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  

# =======================
# This version of the META tools is a fork of an original version produced
# by Vanderbilt University's Institute for Software Integrated Systems (ISIS).
# Their license statement:

# Copyright (C) 2011-2014 Vanderbilt University

# Developed with the sponsorship of the Defense Advanced Research Projects
# Agency (DARPA) and delivered to the U.S. Government with Unlimited Rights
# as defined in DFARS 252.227-7013.

# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this data, including any software or models in source or binary
# form, as well as any drawings, specifications, and documentation
# (collectively "the Data"), to deal in the Data without restriction,
# including without limitation the rights to use, copy, modify, merge,
# publish, distribute, sublicense, and/or sell copies of the Data, and to
# permit persons to whom the Data is furnished to do so, subject to the
# following conditions:

# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Data.

# THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  


# for file in ../src/*/ComponentConfig.cs; do /c/python27/python gen_GME_CS_interpreter_wxi.py  "$file"; done

import sys
import os

_template = '''<?xml version="1.0" encoding="utf-8"?>
#set $backslash = '\\\\'
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
<Fragment>
  <DirectoryRef Id="INSTALLDIR_BIN" />
</Fragment>
<Fragment>
<ComponentGroup Id="${DllName}">
  <Component Id="${DllName}.dll" Directory="INSTALLDIR_BIN">
    <File Id="${DllName}.dll" Name="${DllName}.dll" KeyPath="yes" Source="${DllPath}" />
    <Class Id="{${guid}}" Context="InprocServer32" Description="${progID}" ThreadingModel="both" ForeignServer="mscoree.dll">
      <ProgId Id="${progID}" Description="${progID}" />
    </Class>
    <RegistryValue Root="HKCR" Key="CLSID\{${guid}}\Implemented Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}" Value="" Type="string" Action="write" />
    <RegistryValue Root="HKCR" Key="CLSID\{${guid}}\InprocServer32" Name="Class" Value="${componentName}.${componentName}Interpreter" Type="string" Action="write" />
    <RegistryValue Root="HKCR" Key="CLSID\{${guid}}\InprocServer32" Name="Assembly" Value="${DllName}, Version=${AssemblyVersion}, Culture=neutral" Type="string" Action="write" />
    <RegistryValue Root="HKCR" Key="CLSID\{${guid}}\InprocServer32" Name="RuntimeVersion" Value="v4.0.30319" Type="string" Action="write" />
    <RegistryValue Root="HKCR" Key="CLSID\{${guid}}\InprocServer32" Name="CodeBase" Value="file:///[#${DllName}.dll]" Type="string" Action="write" />
    <RegistryValue Root="HKCR" Key="Component Categories\{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}" Name="0" Value=".NET Category" Type="string" Action="write" />
    <RegistryKey Root='HKLM' Key='Software\GME\Components$backslash${progID}'>
      <RegistryValue Name='Description' Type='string' Value='$componentName'/>
      <RegistryValue Name='Icon' Type='string' Value='${DllName}.ico'/>
      <RegistryValue Name='Paradigm' Type='string' Value='$paradigmName'/>
      <!-- RegistryValue Name='Tooltip' Type='string' Value='TOOLTIP_TEXT'/ -->
#if $componentType.find('ADDON') != -1
      <RegistryValue Name='Type' Type='integer' Value='2'/>
#else
      <RegistryValue Name='Type' Type='integer' Value='1'/>
#end if

#if $paradigmName != '*'
      <RegistryKey Key='Associated'>
        <RegistryValue Name='$paradigmName' Type='string' Value=''/>
      </RegistryKey>
#end if

    </RegistryKey>
  </Component>
  <!-- <Component Directory="INSTALLDIR_BIN">
    <File Source="${ProjectPath}/${DllName}.ico" />
  </Component> -->
</ComponentGroup>
</Fragment>
</Wix>
'''

if __name__=='__main__':
    if os.environ.has_key("UDM_3RDPARTY_PATH"):
        sys.path.append(os.path.join(os.environ["UDM_3RDPARTY_PATH"], r"Cheetah-2.4.4\build\lib.win32-2.6"))
    from Cheetah.Template import Template
    import re
    if len(sys.argv) != 2:
        sys.stderr.write("Need ComponentConfig.cs as an argument")
        sys.exit(4)
    with open(sys.argv[1], 'r') as config:
        lines = config.readlines()
    defines = {}
    for line in filter(lambda line: line.find('public') != -1, lines):
        match = re.search(r"public\s+(?:const|static)\s+\w+\s+(\w+)\s*=\s*\"?([\w. \(\)/|\*-]+)\"?;", line)
        if match:
            defines[match.groups()[0]] = match.groups()[1]
        else:
            sys.stderr.write("Warning: nonmatching line " + line + "\n")
    import os.path
    with open(os.path.dirname(sys.argv[1]) + "/Properties/AssemblyInfo.cs") as ai:
        AssemblyInfo = ai.readlines()
    AssemblyVersion = filter(lambda line: (line.find('//') != 0) and (line.find('assembly: AssemblyVersion') != -1), AssemblyInfo)[-1]
    defines['AssemblyVersion'] = re.search(r'AssemblyVersion\("(.*)"\)', AssemblyVersion).groups()[0]
    #defines['PublicKeyToken'] = 'f240a760fe751c2e'
    defines['DllName'] = defines['componentName'].replace(' ', '_') # TODO: should be the same name as .csproj
    defines['DllPath'] = os.path.dirname(sys.argv[1]) + "\\bin\Release\\" + defines['DllName'] + ".dll"
    defines['ProjectPath'] = os.path.dirname(sys.argv[1])
    with open(defines['DllName'] + ".wxi", 'wb') as output:
        output.write(str(Template(_template, searchList=(defines,))))
