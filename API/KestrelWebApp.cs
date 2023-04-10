using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.VisualBasic.Logging;

namespace API
{
    public class KestrelWebApp
    {
        /// <summary>
        /// Inicia servidor Kestrel
        /// </summary>
        /// <param name="url">URL inicio</param>
        /// <returns>Host de Kestrel</returns>
        public static IWebHost StaticHost;
        public static IWebHost Up(string [] urls, string clientCertificateName = null)
        {
            StaticHost = new WebHostBuilder()            
            .ConfigureKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(o =>
                    {   
                        //permitir cualquier certificado del cliente
                        o.AllowAnyClientCertificate();

                        //solicitamos el certificado al cliente
                        //o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

                        //para cargar un certificado instalado localmente
                        X509Certificate2 localCert = null;
                        if (clientCertificateName != null)
                        {
                            localCert = KestrelWebApp.GetCertificateFromPersonalStore(clientCertificateName);
                        }                        

                        o.ClientCertificateValidation = (cert, validationChain, policyErrors) =>
                        {
                            if (localCert == null)
                               return false;

                            // validamos la huella digital del certificado 
                            // se ignora si AllowAnyClientCertificate esta activo
                            var valid = validationChain.ChainElements
                                 .Cast<X509ChainElement>()
                                 .Any(x => x.Certificate.Thumbprint == localCert.Thumbprint);

                            // otros aspoecto por los cuales puedo validar un certificado
                            /* validationChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                             validationChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                             validationChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                             validationChain.ChainPolicy.VerificationTime = DateTime.Now;
                             validationChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
                             validationChain.ChainPolicy.ExtraStore.Add(serverCert);                            
                            
                             var valid = validationChain.Build(cert);
                             if (!valid)
                                 return false;*/

                            return valid;
                        };
                    });

                })
            .UseKestrel()            
            .UseContentRoot(Directory.GetCurrentDirectory())            
            .UseStartup<KestrelStartUp>()
            .UseUrls(urls)            
            .Build();

            StaticHost.RunAsync();           

            return StaticHost;      
        }

        public static X509Certificate2 GetCertificateFromPersonalStore(string thumbprint)
        {
            var store = GetPersonalCertificates();
            X509Certificate2 certificate = null;
            foreach (var cert in store.Cast<X509Certificate2>().Where(cert => cert.Thumbprint.Equals(thumbprint.ToUpper())))
            {
                certificate = cert;
            }
            return certificate;
        }

        private static X509Certificate2Collection GetPersonalCertificates()
        {
            var localMachineStore = new X509Store(StoreLocation.CurrentUser);
            localMachineStore.Open(OpenFlags.ReadOnly);
            var certificates = localMachineStore.Certificates;
            localMachineStore.Close();
            return certificates;
        }

        /// <summary>
        /// Apaga servidor Kestrel
        /// </summary>
        public static void Down()
        {
            if(StaticHost != null)
                StaticHost.Dispose();
        }
    }
}
