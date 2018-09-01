//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.Output.WildlifeHabitat
{
    public class BytePixel : Pixel
    {
        public Band<byte> MapCode  = "The numeric code for each ecoregion";

        public BytePixel() 
        {
            SetBands(MapCode);
        }
    }
}
