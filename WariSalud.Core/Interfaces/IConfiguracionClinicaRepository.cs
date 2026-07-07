using WariSalud.Core.Entities;

namespace WariSalud.Core.Interfaces;

public interface IConfiguracionClinicaRepository
{
    /// <summary>
    /// Devuelve la configuración global de la clínica (única fila).
    /// </summary>
    Task<ConfiguracionClinica> ObtenerConfiguracionAsync();
    Task ActualizarAsync(ConfiguracionClinica configuracion);
}
