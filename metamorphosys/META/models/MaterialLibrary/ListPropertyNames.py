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

from MaterialLibraryInterface import LibraryManager
import uuid
import ctypes
import os

if __name__ == "__main__": #run only if main
    
    #get path to public documents
    path = ctypes.c_wchar_p(chr(0x00) * 256)
    FOLDERID_Documents = ctypes.c_char_p(uuid.UUID("ED4824AF-DCE4-45A8-81E2-FC7965083634").bytes_le)
    ctypes.windll.shell32.SHGetKnownFolderPath(FOLDERID_Documents, 0, None, ctypes.byref(path))
    libraryLocation = os.path.join(path.value, "META Documents", "MaterialLibrary", "material_library.json")
    
    library_manager = LibraryManager(libraryLocation)
    mySet = set()
    for material in library_manager.data:
        materialData = library_manager.materialData(material)
        for keys in materialData:
            
            unit = ""
            if "unit" in materialData[keys]:
                unit = materialData[keys]["unit"]
            propTuple = keys, unit
            mySet.add(propTuple)
               
           
    propList = list(mySet)
    propList.sort()
    for prop in propList:
        propName, unit = prop
        print propName + ": " + unit