using static FikaShared.Enums;

namespace FikaWebApp
{
    public static class Extensions
    {
        public static string ToMapURL(this EFikaLocation location)
        {
            return location switch
            {
                EFikaLocation.None => "images/tarkovlogo.jpg",
                EFikaLocation.Hideout => "images/maps/hideout.png",
                EFikaLocation.Factory => "images/maps/factory.png",
                EFikaLocation.Customs => "images/maps/customs.png",
                EFikaLocation.Woods => "images/maps/woods.png",
                EFikaLocation.Shoreline => "images/maps/shoreline.png",
                EFikaLocation.Interchange => "images/maps/customs.png",
                EFikaLocation.Reserve => "images/maps/reserve.png",
                EFikaLocation.Streets => "images/maps/streets.png",
                EFikaLocation.Lighthouse => "images/maps/lighthouse.png",
                EFikaLocation.GroundZero => "images/maps/groundzero.png",
                EFikaLocation.Laboratory => "images/maps/labs.png",
                EFikaLocation.Labyrinth => "images/maps/labyrinth.png",
                _ => string.Empty,
            };
        }
    }
}
