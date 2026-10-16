using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Envía mails reales usando SMTP de Gmail.
///
/// IMPORTANTE (seguridad): las credenciales quedan visibles en el código
/// del juego. Para un proyecto de facultad es un compromiso aceptable,
/// pero en un producto real esto se hace desde un servidor, nunca desde
/// el cliente.
///
/// COMPLETAR ANTES DE USAR:
///   - EMAIL_ORIGEN: la cuenta de Gmail que vas a usar para mandar los mails.
///   - APP_PASSWORD: la contraseña de aplicación de 16 caracteres (NO tu
///     contraseña normal de Gmail) que generaste en myaccount.google.com/apppasswords
/// </summary>
public static class EmailSender
{
    private const string SMTP_HOST = "smtp.gmail.com";
    private const int SMTP_PORT = 587;

    // ---------- COMPLETAR ESTOS DOS VALORES ----------
    private const string EMAIL_ORIGEN = "pp3i12.dev@gmail.com";
    private const string APP_PASSWORD = "nkaefnxawpfwhpgq";
    // ---------------------------------------------------

    /// <summary>
    /// Manda el código de recuperación. Se ejecuta en un hilo aparte
    /// (Task.Run) para no trabar el juego mientras se conecta al servidor
    /// de mail. Devuelve true si se mandó bien, false si falló.
    /// </summary>
    public static async Task<bool> EnviarCodigoAsync(string destinatario, string codigo)
    {
        try
        {
            await Task.Run(() =>
            {
                var mensaje = new MailMessage
                {
                    From = new MailAddress(EMAIL_ORIGEN, "QuestLearn"),
                    Subject = "Tu código de recuperación de contraseña - QuestLearn",
                    Body = $"Hola,\n\nTu código de verificación es: {codigo}\n\n" +
                           "Este código expira en 30 minutos. Si no solicitaste " +
                           "este cambio, podés ignorar este mail.\n\nQuestLearn",
                    IsBodyHtml = false
                };
                mensaje.To.Add(destinatario);

                using (var cliente = new SmtpClient(SMTP_HOST, SMTP_PORT))
                {
                    cliente.Credentials = new NetworkCredential(EMAIL_ORIGEN, APP_PASSWORD);
                    cliente.EnableSsl = true;
                    cliente.Send(mensaje);
                }
            });

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EmailSender] Error enviando mail: {ex.Message}");
            return false;
        }
    }
}
