using System;

namespace ReservationService.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public string GuestName { get; set; }
        public DateTime ReservationDate { get; set; }
    }
}
