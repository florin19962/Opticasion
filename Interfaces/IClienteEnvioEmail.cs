using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Interfaces
{
    public interface IClienteEnvioEmail
    {
        void EnviarEmail(String emailcliente, String subjectString, String cuerpoEmail);
    }
}
