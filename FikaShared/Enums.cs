using System.ComponentModel;

namespace FikaShared
{
    public class Enums
    {
        public enum EFikaLocation
        {
            None = 0,
            Hideout,
            Factory,
            Customs,
            Woods,
            Shoreline,
            Interchange,
            Reserve,
            [Description("Streets of Tarkov")]
            Streets,
            Lighthouse,
            [Description("Ground Zero")]
            GroundZero,
            Laboratory,
            Labyrinth
        }
    }
}
