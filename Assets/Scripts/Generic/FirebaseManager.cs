/*using System;
using System.Collections.Generic;
using System.Globalization;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager
{
    private static FirebaseFirestore _db;

    public FirebaseManager()
    {
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Si Firebase está listo, inicializa la app y Firestore
                FirebaseApp app = FirebaseApp.DefaultInstance;
                _db = FirebaseFirestore.GetInstance(app);
                Debug.Log("Firebase listo!");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                // Firebase Unity SDK no está disponible.
                throw new Exception("Firebase no disponible");
            }
        });
    }

    // Función para crear una nueva partida y agregar un jugador
    public string CreateGame(GameToPlay gamePlayed)
    {
        
        DateTime utcNow = DateTime.UtcNow;
        
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        // Convertir a la hora local de la zona horaria elegida
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

        // Crea un nuevo documento para la partida con un ID automático
        DocumentReference partidaRef = _db.Collection("TFG").Document(localTime.ToString("dd-MM-yyyy || HH:mm:ss"));

        // Crea un diccionario para los datos de la partida (solo la fecha de creación)
        Dictionary<string, object> partida = new()
        {
            { "Juego", gamePlayed.ToString()},
            { "Fecha de la partida", localTime }
        };

        // Guarda los datos de la partida en Firestore
        partidaRef.SetAsync(partida).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al crear la partida: " + task.Exception);
            }
            else
            {
                Debug.Log("Partida creada con éxito!");
            }
        });

        return partidaRef.Id;
    }

    // Función para agregar un jugador a una partida existente
    public void AddPlayerToGame(string gameId, string nombreJugador, string nombreClase, int puntuacion)
    {
        // Obtiene la referencia al documento de la partida
        DocumentReference partidaRef = _db.Collection("TFG").Document(gameId);

        // Crea un nuevo documento para el jugador en la subcolección "jugadores"
        DocumentReference jugadorRef = partidaRef.Collection("Jugadores").Document(nombreJugador);

        // Crea un diccionario con los datos del jugador
        Dictionary<string, object> jugador = new()
        {
            { "Nombre", nombreClase },
            { "Puntuacion", puntuacion }
        };

        // Guarda los datos del jugador en Firestore
        jugadorRef.SetAsync(jugador).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al agregar el jugador a la partida: " + task.Exception);
            }
            else
            {
                Debug.Log("Jugador agregado a la partida con éxito!");
            }
        });
    }
}*/