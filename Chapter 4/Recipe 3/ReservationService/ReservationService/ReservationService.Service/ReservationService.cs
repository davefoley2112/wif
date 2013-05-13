using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using ReservationService.Entities;

namespace ReservationService.Service
{
    // Start the service and browse to http://<machine_name>:<port>/SampleService/help to view the service's generated help page
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ReservationService : IReservationService
    {
        public Reservation Create(Reservation instance)
        {
            // Add the new instance of Reservation to the collection
            return Database.AddReservation(instance);
        }
    }
}
