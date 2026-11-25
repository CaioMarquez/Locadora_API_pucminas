using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace LocadoraVeiculos.Middleware
{
    /// <summary>
    /// Captura qualquer exceção não tratada nos controllers e devolve uma
    /// resposta JSON padronizada, em vez de estourar uma tela de erro do IIS/Kestrel.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado ao processar {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,           // 409
                DbUpdateException => HttpStatusCode.BadRequest,                    // 400 (ex.: violação de UNIQUE/FK)
                ArgumentException => HttpStatusCode.BadRequest,                    // 400
                KeyNotFoundException => HttpStatusCode.NotFound,                   // 404
                _ => HttpStatusCode.InternalServerError                            // 500
            };

            var mensagem = exception switch
            {
                DbUpdateException => "Não foi possível salvar os dados. Verifique se não há duplicidade (placa, CPF ou e-mail já cadastrados) ou referência a um registro inexistente.",
                KeyNotFoundException => exception.Message,
                ArgumentException => exception.Message,
                _ => "Ocorreu um erro interno inesperado. Tente novamente mais tarde."
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var resposta = JsonSerializer.Serialize(new
            {
                sucesso = false,
                status = (int)statusCode,
                mensagem
            });

            return context.Response.WriteAsync(resposta);
        }
    }
}
