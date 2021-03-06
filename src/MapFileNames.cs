//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    /// <summary>
    /// Methods for working with the template for filenames of reclass maps.
    /// </summary>
    public static class MapFileNames
    {
        public const string NameVar = "wildlifeName";
        public const string TimestepVar = "timestep";

        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static MapFileNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[NameVar] = true;
            knownVars[TimestepVar] = true;

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------

        public static void CheckTemplateVars(string template)
        {
            OutputPath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 string wildlifeName,
                                                 int    timestep)
        {
            varValues[NameVar] = wildlifeName;
            varValues[TimestepVar] = timestep.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
    }
}
