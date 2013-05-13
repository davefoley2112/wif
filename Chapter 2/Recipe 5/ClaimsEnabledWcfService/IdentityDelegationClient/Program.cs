using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using Microsoft.IdentityModel.SecurityTokenService;
using ClaimsEnabledWcfService;
using System.ServiceModel.Channels;

namespace IdentityDelegationClient
{
    class Program
    {
        static void Main(string[] args)
        {
            EndpointAddress stsAddress = new EndpointAddress("http://localhost:53526/ClaimsEnabledWcfService_STS/Service.svc");
            EndpointAddress stsMexAddress = new EndpointAddress("http://localhost:53526/ClaimsEnabledWcfService_STS/Service.svc/mex");
            EndpointAddress serviceAddress = new EndpointAddress("http://localhost:53490/Service1.svc/");
            EndpointAddress serviceAddressWithDnsIdentity = new EndpointAddress(new Uri("http://localhost:53490/Service1.svc/"), EndpointIdentity.CreateDnsIdentity("DefaultApplicationCertificate"));

            WS2007HttpBinding stsBinding = new WS2007HttpBinding();
            stsBinding.Security.Message.EstablishSecurityContext = false;

            SecurityToken token = GetSecurityTokenFromTrustChannel(serviceAddress, stsAddress);

            WS2007FederationHttpBinding serviceBinding = new WS2007FederationHttpBinding();
            serviceBinding.Security.Mode = WSFederationHttpSecurityMode.Message;
            serviceBinding.Security.Message.IssuerAddress = stsAddress;
            serviceBinding.Security.Message.IssuerBinding = stsBinding;
            serviceBinding.Security.Message.IssuerMetadataAddress = stsMexAddress;

            ChannelFactory<IService1> serviceChannelFactory = new ChannelFactory<IService1>(serviceBinding, serviceAddressWithDnsIdentity);
            serviceChannelFactory.ConfigureChannelFactory();
            IService1 serviceChannel = serviceChannelFactory.CreateChannelWithIssuedToken(token);
            Console.WriteLine(serviceChannel.GetData(10));
            Console.ReadLine();
 
        }
        private static SecurityToken GetSecurityTokenFromTrustChannel(EndpointAddress appliesTo, EndpointAddress stsAddress)
        {
            WSTrustChannel channel = null;
            WSTrustChannelFactory channelFactory = null;
            try
            {
                WS2007HttpBinding stsBinding = new WS2007HttpBinding();
                stsBinding.Security.Message.EstablishSecurityContext = false;
                channelFactory = new WSTrustChannelFactory(stsBinding, stsAddress);
                channel = (WSTrustChannel)channelFactory.CreateChannel();
                RequestSecurityToken rst = new RequestSecurityToken(RequestTypes.Issue);
                rst.AppliesTo = appliesTo;
                RequestSecurityTokenResponse rstr = null;
                SecurityToken token = channel.Issue(rst, out rstr);
                
                ((IChannel)channel).Close();
                channel = null;
                channelFactory.Close();
                channelFactory = null;
                return token;
            }
            finally
            {
                if (channel != null)
                {
                    ((IChannel)channel).Abort();
                }

                if (channelFactory != null)
                {
                    channelFactory.Abort();
                }
            }
        }
    }
}
