using Microsoft.AspNetCore.SignalR;
using WebApiBiblioteca.Classes;
using System;

namespace WebApiBiblioteca.Hubs
{
    // Definición de la clase ChatHub que hereda de Hub<IChat>
    public class ChatHub : Hub<IChat>
    {
        // URL predeterminada para el avatar del host
        public readonly string HostAvatar = "https://cdn-icons-png.flaticon.com/512/126/126486.png";

        // Lista de palabras prohibidas en los mensajes
        public static List<string> PalabrasProhibidas = new List<string>()
    {
        "cabroncete",
    };

        // Método privado que verifica si un mensaje contiene palabras prohibidas
        private bool ContienePalabrasProhibidas(string mensaje)
        {
            return PalabrasProhibidas.Any(p => mensaje.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        // Lista estática de conexiones al hub
        public static List<Connection> conexiones { get; set; } = new List<Connection>();

        // Método para enviar un mensaje al hub
        // Se manejan diferentes casos: envío de mensajes de chat y gestión de conexiones de usuarios
        public async Task SendMessage(Message message)
        {
            // Verificar si el mensaje no está vacío
            if (!string.IsNullOrEmpty(message.Text))
            {
                // Verificar si el mensaje contiene palabras prohibidas
                if (ContienePalabrasProhibidas(message.Text))
                {
                    // Notificar a todos los clientes sobre el mensaje censurado
                    await Clients.All.GetMessage(new Message { Avatar = HostAvatar, User = "Host", Text = $"Al usuario {message.User} se le ha anulado un mensaje por vocabulario inapropiado." });
                }
                else
                {
                    // Enviar el mensaje a todos los clientes conectados
                    await Clients.All.GetMessage(message);
                }
            }
            // Verificar si el mensaje está vacío pero se proporciona un nombre de usuario
            else if (!string.IsNullOrEmpty(message.User))
            {
                // Agregar la nueva conexión a la lista de conexiones
                conexiones.Add(new Connection { Id = Context.ConnectionId, User = message.User, Avatar = message.Avatar });

                // Notificar a todos excepto al nuevo usuario sobre la conexión
                await Clients.AllExcept(Context.ConnectionId).GetMessage(new Message() { Avatar = message.Avatar, User = message.User, Text = " se ha conectado!" });

                // Enviar la lista actualizada de usuarios a todos los clientes
                await Clients.All.GetUsers(conexiones);
            }
        }

        // Sobrescribir el método OnConnectedAsync para dar la bienvenida al nuevo usuario
        public override async Task OnConnectedAsync()
        {
            // Enviar un mensaje de bienvenida al nuevo usuario
            await Clients.Client(Context.ConnectionId).GetMessage(new Message() { Avatar = HostAvatar, User = "Host", Text = "Hola, Bienvenido al Chat" });

            // Llamar al método base para realizar la conexión
            await base.OnConnectedAsync();
        }

        // Sobrescribir el método OnDisconnectedAsync para manejar desconexiones de usuarios
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Obtener la conexión que se desconecta
            var conexion = conexiones.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();

            // Notificar a todos excepto al usuario que se desconecta sobre la salida
            await Clients.AllExcept(Context.ConnectionId).GetMessage(new Message() { Avatar = HostAvatar, User = "Host", Text = $"{conexion.User} ha salido del chat" });

            // Eliminar la conexión de la lista de conexiones
            conexiones.Remove(conexion);

            // Enviar la lista actualizada de usuarios a todos los clientes
            await Clients.All.GetUsers(conexiones);
            }

            // Llamar al método base para manejar la desconexión
            await base.OnDisconnectedAsync(exception);
        }
    }
}
