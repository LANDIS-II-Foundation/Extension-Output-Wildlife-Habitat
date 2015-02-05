//  Authors:  Brian R. Miranda

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Output.WildlifeHabitat
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParametersParser
        : TextParser<IInputParameters>
    {
        public static ISpeciesDataset SpeciesDataset = null;


        //---------------------------------------------------------------------

        public InputParametersParser()
        {
        }
        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }
        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != PlugIn.ExtensionName)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", PlugIn.ExtensionName);

            InputParameters parameters = new InputParameters(SpeciesDataset.Count);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<int> outputTimestep = new InputVar<int>("OutputTimestep");
            ReadVar(outputTimestep);
            parameters.OutputTimestep = outputTimestep.Value;

            // Template for filenames of reclass maps

            InputVar<string> mapFileNames = new InputVar<string>("MapFileNames");
            ReadVar(mapFileNames);
            parameters.MapFileNames = mapFileNames.Value;


            InputVar<string> speciesName = new InputVar<string>("Species");

            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();
            
            //  Read list of Suitability Files
            InputVar<string> suitabilityFile = new InputVar<string>("SuitabilityFiles");
            ReadVar(suitabilityFile);

            List<ISuitabilityParameters> suitabilityParameterList = new List<ISuitabilityParameters>();
            SuitabilityFileParametersParser suitabilityParser = new SuitabilityFileParametersParser();

            ISuitabilityParameters suitabilityParameters = Landis.Data.Load<ISuitabilityParameters>(suitabilityFile.Value, suitabilityParser);
            suitabilityParameterList.Add(suitabilityParameters);

            while (!AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(suitabilityFile, currentLine);
                CheckForRepeatedName(suitabilityFile.Value, "suitabilty file", lineNumbers);

                suitabilityParameters = Landis.Data.Load<ISuitabilityParameters>(suitabilityFile.Value, suitabilityParser);
                suitabilityParameterList.Add(suitabilityParameters);
                
                GetNextLine();
            }


            CheckNoDataAfter(string.Format("the {0} parameter", "SuitabilityFiles"));

            return parameters; //.GetComplete();
        }

        //---------------------------------------------------------------------

        protected ISpecies GetSpecies(InputValue<string> name)
        {
            ISpecies species = SpeciesDataset[name.Actual];
            if (species == null)
                throw new InputValueException(name.String,
                                              "{0} is not a species name.",
                                              name.String);
            return species;
        }

        //---------------------------------------------------------------------

        private void CheckForRepeatedName(InputValue<string>      name,
                                          string                  description,
                                          Dictionary<string, int> lineNumbers)
        {
            int lineNumber;
            if (lineNumbers.TryGetValue(name.Actual, out lineNumber))
                throw new InputValueException(name.String,
                                              "The {0} {1} was previously used on line {2}",
                                              description, name.String, lineNumber);
            lineNumbers[name.Actual] = LineNumber;
        }
    }
}
