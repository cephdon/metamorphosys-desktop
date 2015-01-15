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

import sys
import os
from common import PostProcess, update_metrics_in_report_json, read_limits, check_limits_and_add_to_report_json

if __name__ == '__main__':
    if len(sys.argv) > 1:
        try:
            mat_file_name = sys.argv[1]
            if not os.path.exists(mat_file_name):
                print 'Given result file does not exist: {0}'.format(sys.argv[1])
                os._exit(3)
            
            limits_dict, filter = read_limits()
            
            filter.append('VoltageSensor.v')
            # loads results with the filtered out variables (and 'time' which is default)
            pp = PostProcess(mat_file_name, filter)

            metrics = {}
            # No need to convert values into string that is done in update_metrics_in_report_json
            metrics.update({'VoltageOverInductor' : {'value': pp.global_max('VoltageSensor.v'), 'unit': 'Volt'}})
            metrics.update({'VoltageOverInductor10' : {'value': pp.global_max('VoltageSensor.v') * 10, 'unit': 'deciVolt'}})
            metrics.update({'VoltageOverInductor100' : {'value': pp.global_max('VoltageSensor.v') * 100, 'unit': 'centiVolt'}})
            cwd = os.getcwd()
            os.chdir('..')      
            update_metrics_in_report_json(metrics)
            
            check_limits_and_add_to_report_json(pp, limits_dict)
            os.chdir(cwd)
        except Exception as err:
            print err.message
            if os.name == 'nt':
                import win32api
                win32api.TerminateProcess(win32api.GetCurrentProcess(), 1)
            else:
                sys.exit(1)


