# Consola Chat

Esta solución incluye una aplicación de chat en tiempo real basada en SignalR y un exportador de conversaciones.

## Requisitos
- .NET 8
- MongoDB en `mongodb://localhost:27017`

## Exportar conversación
Para generar un archivo `conversation.txt` con los mensajes almacenados en MongoDB y mostrar su contenido como lo haría `cat`:

```bash
dotnet run --project ChatExporter
```
