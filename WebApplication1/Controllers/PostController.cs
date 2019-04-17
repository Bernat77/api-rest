using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
 

namespace WebApplication1.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]

    public class PostController:ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] FichajeDTO fichaje)
        {
            //  var body = HttpContext.Request.Body;
            //return fichaje.nombre;
            fichaje.CreateFile();


        }                     
    }

    public class FichajeDTO
    {
        DateTime hora;
        public string nombre { get; set; }
        DateTime fecha;
        string path;

        public void CreateFile()
        {
            path = @"C:\Users\Bernat Salleras\Desktop\" + nombre + ".txt";

            if (!System.IO.File.Exists(path)){
                using (StreamWriter str = System.IO.File.CreateText(path))
                {
                    str.WriteLine($"Nombre: {nombre}");
                    str.WriteLine($"Fecha: {fecha}");
                    str.WriteLine($"Hora: {hora}");
                    str.WriteLine($"Hora: {path}");
                }
            }
        }
    }

}


