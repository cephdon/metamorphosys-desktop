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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemCParser;

namespace SystemCParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Henry\repos\tonkalib\designs\SystemC_Examples\results\gvkbzzga\local_ccled\local_ccled.h";
            Uncomment nocomment = new Uncomment(filePath);
            Console.WriteLine("Done:\n{0}", nocomment.result);

            // ToDo: parse the system C info from the "nocomment.result" string.
            ScParse parsed = new ScParse(nocomment.result);
        }
    }
}