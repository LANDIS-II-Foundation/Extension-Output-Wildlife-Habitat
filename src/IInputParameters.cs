//  Authors:  Robert M. Scheller, Jimm Domingo

using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    /// <summary>
    /// The parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep
        {
            get;set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Output Timestep (years)
        /// </summary>
        int OutputTimestep
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for reclass maps.
        /// </summary>
        string MapFileNames
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability Input files
        /// </summary>
        List<string> SuitabilityFiles
        {
            get;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability parameters
        /// </summary>
        List<ISuitabilityParameters> SuitabilityParameters
        {
            get;
        }


    }
}
