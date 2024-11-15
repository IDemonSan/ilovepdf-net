using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using iLovePdf;
using System.IO;
using System.Threading.Tasks;
using iLovePdf.Core;
using iLovePdf.Model.Task;
using iLovePdf.Model.TaskParams;
using iLovePdf.Model.Enums;


namespace ServiceILovePDF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IlovePDFController : ControllerBase
    {

        private readonly iLovePdfApi _api;

        public IlovePDFController(IConfiguration config)
        {
            // Configura tu API con las claves de configuración
            var publicKey = config["IlovePDF:PublicKey"];
            var secretKey = config["IlovePDF:SecretKey"];
            _api = new iLovePdfApi(publicKey, secretKey);
        }

        [HttpPost("compress")]
        public async Task<IActionResult> CompressFiles([FromBody] string[] filePaths)
        {
            try
            {
                // Crear una nueva tarea de compresión
                var taskCompress = _api.CreateTask<CompressTask>();

                // Agregar archivos a la tarea
                foreach (var filePath in filePaths)
                {
                    taskCompress.AddFile(filePath);
                }

                // Configurar parámetros de compresión
                var compressParams = new CompressParams
                {
                    CompressionLevel = CompressionLevels.Extreme,
                    OutputFileName = "compressed"
                };

                // Procesar la tarea de manera asincrónica
                await Task.Run(() => taskCompress.Process(compressParams));

                // Obtener la ruta de la carpeta de Descargas del usuario
                var outputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Descargar el archivo comprimido a la carpeta de Descargas
                await Task.Run(() => taskCompress.DownloadFile(outputFilePath));

                // Convertir el archivo descargado a un array de bytes
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);

                // Retornar el archivo comprimido
                return File(fileBytes, "application/pdf", "compressed.pdf");
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return Ok(new { message = "Mensaje durante la compresión" + ex.Message });
            }
        }
    }
}
