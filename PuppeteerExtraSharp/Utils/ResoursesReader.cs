﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PuppeteerExtraSharp.Utils
{
    internal static class ResourcesReader
    {
        public static string ReadFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path);
            if(stream is null)
                throw new FileNotFoundException($"File with path {path} not found!");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}