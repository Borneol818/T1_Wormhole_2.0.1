using NetTopologySuite.Geometries;

namespace T1_Wormhole_2._0._1.Models.DTOs
{
    public class UpdateLocationDTO
    {
        public int UserId { get; set; }
        public Position Position { get; set; }
    }

    public class Position
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
