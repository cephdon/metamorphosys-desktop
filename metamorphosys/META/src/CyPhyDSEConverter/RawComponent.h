// Copyright (C) 2013-2015 MetaMorph Software, Inc

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this data, including any software or models in source or binary
// form, as well as any drawings, specifications, and documentation
// (collectively "the Data"), to deal in the Data without restriction,
// including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Data, and to
// permit persons to whom the Data is furnished to do so, subject to the
// following conditions:

// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Data.

// THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  

// =======================
// This version of the META tools is a fork of an original version produced
// by Vanderbilt University's Institute for Software Integrated Systems (ISIS).
// Their license statement:

// Copyright (C) 2011-2014 Vanderbilt University

// Developed with the sponsorship of the Defense Advanced Research Projects
// Agency (DARPA) and delivered to the U.S. Government with Unlimited Rights
// as defined in DFARS 252.227-7013.

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this data, including any software or models in source or binary
// form, as well as any drawings, specifications, and documentation
// (collectively "the Data"), to deal in the Data without restriction,
// including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Data, and to
// permit persons to whom the Data is furnished to do so, subject to the
// following conditions:

// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Data.

// THE DATA IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS, SPONSORS, DEVELOPERS, CONTRIBUTORS, OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE DATA OR THE USE OR OTHER DEALINGS IN THE DATA.  

#ifndef RAWCOMPONENT_H
#define RAWCOMPONENT_H

#include "Mga.h"


// Declaration of the main RAW COM component interface class


#ifdef BUILDER_OBJECT_NETWORK
#error   This file should only be included in the RAW COM configurations
#endif

#include <comutil.h>
#include <map>
#include <string>

#include "atlsafe.h"

class RawComponent {
////////////////////
// Insert your application specific member and method definitions here
public:
	std::map<std::string, _variant_t> componentParameters;
	std::string delimiter;

	void UpdateParameters();

	RawComponent()
	{
		delimiter = ";\n";
		std::string Value = "";
		// initialize component parameters
		componentParameters.clear();
		//componentParameters["key"]            = _variant_t("value");
		componentParameters["name"]             = _variant_t("Design Space Refactorer");

		Value.erase();
		Value += "Provides various facilities to convert design space elements into other types of design space elements while preserving the connections and creating new ports as needed.\n";
		componentParameters["description"]      = _variant_t(Value.c_str());

		componentParameters["help"]             = _variant_t("Help string message. Usage: TBA...");
		componentParameters["console_messages"] = _variant_t("on");
		componentParameters["refactored_type_or_action"]       = _variant_t("");
		componentParameters["automation"]       = _variant_t("false");
		componentParameters["runCommand"]       = _variant_t("main_post_process.py testbench_manifest.json");  // JS modified to allow batch from Master T/Job Processor
		componentParameters["labels"]       = _variant_t(L""); // empty string means use default label
		componentParameters["original_project_file"]	= _variant_t("");
		componentParameters["results_zip_py"]	= _variant_t("");

/*		CComSafeArray<BSTR> safe;
		// list of all supported kinds
		safe.Add(CComBSTR(L"DesignContainer"));
		safe.Add(CComBSTR(L"ComponentAssembly"));
		safe.Add(CComBSTR(L"Component"));

		_variant_t pVal("");
		CComVariant(safe).Detach(&pVal);
		componentParameters["supports"]         = _variant_t(pVal);
*/
		componentParameters["supports"]         = _variant_t("");
	}
private:


// Try not to modify the code below this line
////////////////////
public:	
#ifdef GME_ADDON
	CComPtr<IMgaProject> project;  // this is set before Initialize() is called
	CComPtr<IMgaAddOn> addon;      // this is set before Initialize() is called
#endif
	bool interactive;
	
	STDMETHODIMP Initialize(struct IMgaProject *);
	STDMETHODIMP Invoke(IMgaProject* gme, IMgaFCOs *models, long param);
	STDMETHODIMP InvokeEx( IMgaProject *project,  IMgaFCO *currentobj,  IMgaFCOs *selectedobjs,  long param);
	STDMETHODIMP ObjectsInvokeEx( IMgaProject *project,  IMgaObject *currentobj,  IMgaObjects *selectedobjs,  long param);
	STDMETHODIMP get_ComponentParameter(BSTR name, VARIANT *pVal);
	STDMETHODIMP put_ComponentParameter(BSTR name, VARIANT newVal);


#ifdef GME_ADDON
	STDMETHODIMP GlobalEvent(globalevent_enum event);
	STDMETHODIMP ObjectEvent(IMgaObject * obj, unsigned long eventmask, VARIANT v);
#endif
};



#endif //RAWCOMPONENT_H