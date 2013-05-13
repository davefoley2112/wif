using System;
using System.Collections.Generic;
using ReservationService.Entities;

namespace ReservationService.Service
{
    static class Database
    {
        static Database()
        {
            Items = new Dictionary<int, Reservation>
                {
                    { 1, new Reservation{ Id = 1, GuestName = "Adam Smith", ReservationDate = DateTime.Now.AddDays(10)} },
                    { 2, new Reservation{ Id = 2, GuestName = "George Michael", ReservationDate = DateTime.Now.AddDays(15)} }
                };
        }
        public static Dictionary<int, Reservation> Items;

        public static Reservation AddReservation(Reservation res)
        {
            res.Id = Items.Count + 1;
            Items.Add(res.Id, res);
            return res; 
        }
    }
}
