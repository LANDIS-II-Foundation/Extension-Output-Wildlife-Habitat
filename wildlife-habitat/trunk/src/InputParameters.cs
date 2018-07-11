//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    /// <summary>
    /// The parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private int outputTimestep;
        private string mapFileNames;
        private List<string> suitabilityFiles;
        private List<ISuitabilityParameters> suitabilityParameters;
      
        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(),"Value must be = or > 0.");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Output Timestep (years)
        /// </summary>
        public int OutputTimestep
        {
            get
            {
                return outputTimestep;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                if (value < Timestep)
                    throw new InputValueException(value.ToString(), "Value must be = or > Timestep.");

                outputTimestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for reclass maps.
        /// </summary>
        public string MapFileNames
        {
            get
            {
                return mapFileNames;
            }
            set
            {
                WildlifeHabitat.MapFileNames.CheckTemplateVars(value);
                mapFileNames = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability input files
        /// </summary>
        public List<string> SuitabilityFiles
        {
            get {
                return suitabilityFiles;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Suitability input files
        /// </summary>
        public List<ISuitabilityParameters> SuitabilityParameters
        {
            get
            {
                return suitabilityParameters;
            }
            set
            {
                suitabilityParameters = value;
            }
        }

        //---------------------------------------------------------------------

        public InputParameters(int speciesCount)
        {
            suitabilityFiles = new List<string>();
        }
        //---------------------------------------------------------------------


    }
}
