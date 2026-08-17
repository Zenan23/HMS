namespace Persistence.Models
{
    /// <summary>
    /// Historija pregleda detalja hotela po korisniku — ponašajni signal za recommender
    /// (popularity/trending komponenta). Upisuje se stvarno pri svakom pregledu detalja hotela
    /// (HotelsController.GetById), ne samo simulira; koristi se u HotelService pri rangiranju
    /// preporuka (vidi CalculateViewBoost).
    /// </summary>
    public class HotelViewHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
