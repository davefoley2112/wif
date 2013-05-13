using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using ReservationService.Entities;

namespace ReservationService.Service
{
    // Start the service and browse to http://<machine_name>:<port>/SampleService/help to view the service's generated help page
    [ServiceContract]
    public interface IReservationService
    {
        [WebInvoke(UriTemplate = "CreateReservation", Method = "POST")]
        Reservation Create(Reservation instance);
    }
}
