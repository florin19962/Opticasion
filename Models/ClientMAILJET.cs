using Microsoft.Extensions.Options;
using Opticasion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Opticasion.Models
{
    public class ClientMAILJET : IClienteEnvioEmail
    {
        private IOptions<EmailServerMAILJET> _serverSMTP;

        public ClientMAILJET(IOptions<EmailServerMAILJET> configServer)
        {
            this._serverSMTP = configServer; //<---ya tengo user,secret-key,host de mailjet
        }

        public void EnviarEmail(string emailcliente,
                        string subjectString,
                        string cuerpoEmail)
        {
            //necesito mandar el email atraves de clases de .net

            //1º paso me creo cliente de SMTP para poder mandar email:
            SmtpClient _clienteSMTP = new SmtpClient();
            _clienteSMTP.Host = this._serverSMTP.Value.HostSMTP;
            _clienteSMTP.Credentials = new NetworkCredential(this._serverSMTP.Value.UserAPI,
                                                             this._serverSMTP.Value.SecretAPI);

            //2º me creo un objeto de tipo MailMessage y lo envio usando ese cliente
            MailMessage _mensajeEmail = new MailMessage("florin19962@hotmail.com", emailcliente);
            _mensajeEmail.Subject = subjectString;
            _mensajeEmail.IsBodyHtml = true;
            _mensajeEmail.Body = cuerpoEmail;

            _clienteSMTP.Send(_mensajeEmail);
        }
    }
}
