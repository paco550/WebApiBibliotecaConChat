using Microsoft.AspNetCore.SignalR;
using WebApiBiblioteca.Classes;
using System;

namespace WebApiBiblioteca.Hubs;

// Definición de la clase ChatHub que hereda de Hub<IChat>
public class ChatHub : Hub<IChat>
{
    #region props    
    // Semáforo para asegurar el acceso seguro a las salas y Conexiones
    private static readonly SemaphoreSlim SalasSemaphore = new SemaphoreSlim(1, 1);

    // URL predeterminada para el avatar del host
    private readonly string HostAvatar = "https://cdn-icons-png.flaticon.com/512/126/126486.png";

    // Diccionario que mapea las salas a las Conexiones en esa sala
    private static Dictionary<string, List<Connection>> Salas = new Dictionary<string, List<Connection>>();

    // Lista de palabras prohibidas en los mensajes
    private static readonly List<string> PalabrasProhibidas = new List<string> { "cabroncete" };

    // Lista estática de Conexiones al hub
    private static List<Connection> Conexiones { get; } = new List<Connection>();
    #endregion

    #region METODOS   
    // Método privado que verifica si un mensaje contiene palabras prohibidas
    private bool ContienePalabrasProhibidas(string mensaje)
    {
        return PalabrasProhibidas.Any(p => mensaje.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    // Método para unirse a una sala
    public async Task UnirseASala(Message message)
    {
       // await SalasSemaphore.WaitAsync();
      
            if (!Salas.ContainsKey(message.Sala))
            {
                Salas[message.Sala] = new List<Connection>();
            }

            var nuevaConexion = new Connection 
            { 
                Id = Context.ConnectionId, 
                User = message.User, 
                Avatar = message.Avatar,
                Sala = message.Sala, 
            };

            Conexiones.Add(nuevaConexion);
            Salas[message.Sala].Add(nuevaConexion);

            await Groups.AddToGroupAsync(Context.ConnectionId, message.Sala);

            await Clients.Group(message.Sala)
                .GetMessage(new Message 
                { 
                    Avatar = message.Avatar, 
                    User = message.User, 
                    Text = $" se ha unido a la sala {message.Sala}." 
                });

            await Clients.All.GetUsers(Conexiones);
       
            //SalasSemaphore.Release();
        
    }

    // Método para enviar un mensaje al hub
    public async Task SendMessage(Message message)
    {
       // await SalasSemaphore.WaitAsync();
       
            // Verificar si el mensaje no está vacío
            if (!string.IsNullOrEmpty(message.Text))
            {
                // Verificar si el mensaje contiene palabras prohibidas
                if (ContienePalabrasProhibidas(message.Text))
                {
                    // Notificar a todos los clientes sobre el mensaje censurado
                    await Clients.Group(message.Sala)
                        .GetMessage(new Message 
                        { 
                            Avatar = HostAvatar, 
                            User = "Host", 
                            Text = $"Al usuario {message.User} se le ha anulado un mensaje por vocabulario inapropiado.",
                            Sala = message.Sala
                        });
                }
                else
                {
                    // Enviar el mensaje a todos los clientes conectados en la sala
                    await Clients.Group(message.Sala).GetMessage(message);
                }
            }
            // Verificar si el mensaje está vacío pero se proporciona un nombre de usuario
            else if (!string.IsNullOrEmpty(message.User))
            {
                // Agregar la nueva conexión a la lista de Conexiones
                Conexiones.Add(new Connection 
                { 
                    Id = Context.ConnectionId, 
                    User = message.User, 
                    Avatar = message.Avatar,
                    Sala = message.Sala,
                });

                // Notificar a todos excepto al nuevo usuario sobre la conexión
                await Clients.AllExcept(Context.ConnectionId)
                    .GetMessage(new Message 
                    { 
                        Avatar = message.Avatar, 
                        User = message.User, 
                        Text = " se ha conectado!", 
                        Sala = message.Sala,
                    });

                // Enviar la lista actualizada de usuarios a todos los clientes
                await Clients.All.GetUsers(Conexiones);
            }     
          //  SalasSemaphore.Release();
    }

    // Sobrescribir el método OnConnectedAsync para dar la bienvenida al nuevo usuario
    public override async Task OnConnectedAsync()
    {
        // await SalasSemaphore.WaitAsync();
        var conexion = Conexiones.FirstOrDefault(x => x.Id == Context.ConnectionId);
        // Enviar un mensaje de bienvenida al nuevo usuario
        await Clients.Client(Context.ConnectionId)
                .GetMessage(new Message 
                { 
                    Avatar = HostAvatar, 
                    User = "Host", 
                    Text = "Hola, Bienvenido al Chat", 
                    Sala = conexion?.Sala ?? string.Empty,
                });
       
            //SalasSemaphore.Release();
        

        // Llamar al método base para realizar la conexión
        await base.OnConnectedAsync();
    }

    // Sobrescribir el método OnDisconnectedAsync para manejar desconexiones de usuarios
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
       // await SalasSemaphore.WaitAsync();
        
            var conexion = Conexiones.FirstOrDefault(x => x.Id == Context.ConnectionId);

            if (conexion is not null)
            {
                await Clients.AllExcept(Context.ConnectionId)
                    .GetMessage(new Message 
                    { 
                        Avatar = HostAvatar, 
                        User = "Host", 
                        Text = $"{conexion.User} ha salido del chat",
                        Sala = conexion.Sala,
                    });
                Conexiones.Remove(conexion);
                await Clients.All.GetUsers(Conexiones);
            }   
            //SalasSemaphore.Release();
        await base.OnDisconnectedAsync(exception);
    } 
    #endregion
}
