using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public class KittensForwardModel : IForwardModel
{ 
    public async Task<bool> PlayAction(IGameState gameState, IAction action)
    {
        KittensAction actionToExecute;
        if (action is null) actionToExecute = new KittensNothing();
        else  actionToExecute = (KittensAction)action;
        
        KittensGameState kittensGs = gameState as KittensGameState;
        
        kittensGs!.CanBlock = true;
        int playerIndex = gameState.GetPlayerTurnIndex();
        bool playsAction = true;

        /*if (actionToExecute is not KittensExploding and not KittensNothing)
        {
            bool specialCardPlayed;
            do
            {
                for (int i = 0; i < kittensGs.PlayersStatus.Count; i++)
                {
                    KittensPlayerStatus player = kittensGs.PlayersStatus[i];
                    foreach (KittensCard card in player.Hand.Cast<KittensCard>())
                    {
                        if (card.Type == KittensType.Nope) quantityNopeCardsPerPlayer[i]++;
                    }
                }
                
                int index = -1;
                specialCardPlayed = false;
                foreach (int unused in quantityNopeCardsPerPlayer)
                {
                    if (quantityNopeCardsPerPlayer[++index] <= 0 || index == playerIndex) continue;
                    
                    IObservation specialObservation = kittensGs.GetObservationFromPlayer(index);
                    IAction specialAction = new KittensAction();

                    Task<IAction> task = GetActionFromPlayer(specialObservation, specialObservation.GetPlayerTurnIndex());
                    Task delayTask = Task.Delay(GameManager.TimeToThink * 1000);

                    Task firstFinished = await Task.WhenAny(task, delayTask);

                    if (firstFinished == task) // Si la tarea principal termina antes del timeout
                        specialAction = task.Result;

                    if (specialAction is KittensNope)
                    {
                        //Se descarta la carta
                        Queue<Card> hand = kittensGs.PlayersStatus[index].Hand;
                        int cardIndex = 0;
                        foreach (KittensCard card in hand.Cast<KittensCard>())
                        {
                            if (card.Type == KittensType.Nope)
                            {
                                Card cardToDiscard = Auxiliar<Card>.GetAndRemoveCardFromQueue(ref hand, cardIndex);
                                kittensGs.DiscardCard(cardToDiscard);
                                break;
                            }
                            cardIndex++;
                        }
                        
                        specialCardPlayed = true;
                        playsAction = !playsAction;
                        playerIndex = index;
                        break; // Salir del bucle para verificar nuevamente desde el principio
                    }
                }
            } while (specialCardPlayed);
        }*/
        
        kittensGs.CanBlock = false;

        if (actionToExecute is KittensFavor favor)
        {
            kittensGs.IsDoingFavor = favor.CardIndex;

            IObservation specialObservation = kittensGs.GetObservationFromPlayer(favor.PlayerTarget);
            KittensObservation obs = specialObservation as KittensObservation;

            Task<IAction> task = GetActionFromPlayer(specialObservation, specialObservation.GetPlayerTurnIndex());
            Task delayTask = Task.Delay(GameManager.TimeToThink * 1000);

            Task firstFinished = await Task.WhenAny(task, delayTask);

            IAction specialAction = firstFinished == task ? task.Result : new KittensGiveCard(playerIndex, favor.PlayerTarget, favor.CardIndex, new Random().Next(0, obs.PlayersStatus[favor.PlayerTarget].Hand.Count));

            actionToExecute = (KittensAction)specialAction!;
            kittensGs.IsDoingFavor = -1;
        }

        if (playsAction)
        {
            await actionToExecute.PlayAction(kittensGs);
        }
        else
        {
            await new KittensNope(actionToExecute.CardIndex).PlayAction(kittensGs);
        }
        
        await EndTurn(kittensGs);
        await ChangeTurn(kittensGs);
        
        return true;
    }

    private async Task EndTurn(IGameState gameState)
    {
        await Task.Delay(250);
        KittensGameState kittensGs = gameState as KittensGameState;
        if (!kittensGs!.TurnEnded) return;
        if (!kittensGs.DrawAtEnd) return;
        KittensPlayerStatus player = kittensGs.PlayersStatus[kittensGs.GetPlayerTurnIndex()];
        KittensCard cardDrawn = (KittensCard)kittensGs.DrawCardFromDrawDeck(player.HandGObject);
        player.Hand.Enqueue(cardDrawn);

        if (cardDrawn.Type == KittensType.Exploding) _ = await PlayAction(gameState, new KittensExploding());
    }

    private Task ChangeTurn(IGameState gameState)
    {
        if (gameState is not KittensGameState kittensGs) throw new ArgumentNullException(nameof(kittensGs));
        
        kittensGs.UpdateHands();
        if (!kittensGs.TurnEnded) return Task.CompletedTask;
        //Debug.Log("Turnos restantes:" + kittensGs.TurnsToPlay);
        if (--kittensGs.TurnsToPlay <= 0) kittensGs.ChangeTurnIndex();
        kittensGs.TurnEnded = false;
        kittensGs.TurnsToPlay = kittensGs.NextTurnsToPlay;
        kittensGs.NextTurnsToPlay = 1;
        kittensGs.DrawAtEnd = true;

        return Task.CompletedTask;
    }
    
    public async Task<bool> TestAction(IObservation observation, IAction action)
    {
        KittensAction actionToExecute;
        if (action is null) actionToExecute = new KittensNothing();
        else  actionToExecute = (KittensAction)action;
        
        KittensObservation kittensOb = observation as KittensObservation;
        kittensOb!.ActionIsPlaying = true;
        int playerIndex = observation.GetPlayerTurnIndex();
        bool playsAction = true;

        if (actionToExecute is not KittensExploding and not KittensNothing)
        {
            bool specialCardPlayed;
            do
            {
                int[] quantityNopeCardsPerPlayer = new int[kittensOb.PlayersStatus.Count];

                for (int i = 0; i < kittensOb.PlayersStatus.Count; i++)
                {
                    KittensPlayerStatus player = kittensOb.PlayersStatus[i];
                    foreach (KittensCard card in player.Hand.Cast<KittensCard>())
                    {
                        if (card.Type == KittensType.Nope) quantityNopeCardsPerPlayer[i]++;
                    }
                }
                
                int index = -1;
                specialCardPlayed = false;
                foreach (int player in quantityNopeCardsPerPlayer)
                {
                    if (quantityNopeCardsPerPlayer[++index] <= 0 || index == playerIndex) continue;
                    
                    IObservation specialObservation = kittensOb.CopyFromPlayer(player);
                    
                    IAction specialAction = new KittensAction();

                    Task<IAction> task = GetActionFromPlayer(specialObservation, specialObservation.GetPlayerTurnIndex());
                    Task delayTask = Task.Delay(GameManager.TimeToThink * 1000);

                    Task firstFinished = await Task.WhenAny(task, delayTask);

                    if (firstFinished == task) // Si la tarea principal termina antes del timeout
                        specialAction = task.Result;

                    if (specialAction is KittensNope)
                    {
                        Queue<Card> hand = kittensOb.PlayersStatus[index].Hand;
                        int cardIndex = 0;
                        foreach (KittensCard card in hand.Cast<KittensCard>())
                        {
                            if (card.Type == KittensType.Nope)
                            {
                                kittensOb.DiscardCard((KittensCard)Auxiliar<Card>.GetAndRemoveCardFromQueue(ref hand, cardIndex));
                                break;
                            }
                            cardIndex++;
                        }
                        
                        specialCardPlayed = true;
                        playsAction = !playsAction;
                        playerIndex = index;
                        break; // Salir del bucle para verificar nuevamente desde el principio
                    }
                }
            } while (specialCardPlayed);
        }
        
        kittensOb.ActionIsPlaying = false;

        if (actionToExecute is KittensFavor favor)
        {
            kittensOb.IsDoingFavor = favor.CardIndex;
            IObservation specialObservation = kittensOb.CopyFromPlayer(favor.PlayerTarget);
            IAction specialAction = new KittensAction();

            Task<IAction> task = GetActionFromPlayer(specialObservation, specialObservation.GetPlayerTurnIndex());
            Task delayTask = Task.Delay(GameManager.TimeToThink * 1000);

            Task firstFinished = await Task.WhenAny(task, delayTask);

            if (firstFinished == task) // Si la tarea principal termina antes del timeout
                specialAction = task.Result;
            actionToExecute = (KittensAction) specialAction;
            kittensOb.IsDoingFavor = -1;
        }

        if (playsAction)
        {
            await Task.FromResult(actionToExecute.TestAction(observation));
        }
        else
        {
            await Task.FromResult(new KittensNope(actionToExecute.CardIndex).TestAction(observation));
        }
        
        await EndObTurn(observation);
        await ChangeObTurn(observation);
        
        return true;
    }

    private Task EndObTurn(IObservation observation)
    {
        KittensObservation kittensOb = observation as KittensObservation;

        if (!kittensOb!.TurnEnded) return Task.CompletedTask;

        if (!kittensOb.DrawAtEnd) return Task.CompletedTask;
        
        KittensPlayerStatus player = kittensOb.PlayersStatus[kittensOb.GetPlayerTurnIndex()];
        KittensCard cardDrawn = (KittensCard)kittensOb.DrawCardFromMixedDrawDeck();
        player.Hand.Enqueue(cardDrawn);
        
        if (cardDrawn.Type == KittensType.Exploding) _ = TestAction(kittensOb, new KittensExploding());
        return Task.CompletedTask;
    }

    private Task ChangeObTurn(IObservation observation)
    {
        if (observation is not KittensObservation kittensObs) return Task.CompletedTask;

        if (!kittensObs.TurnEnded || --kittensObs.TurnsToPlay > 0) return Task.CompletedTask;
        kittensObs.ChangeTurnIndex();
        kittensObs.TurnEnded = false;
        kittensObs.TurnsToPlay = kittensObs.NextTurnsToPlay;
        kittensObs.NextTurnsToPlay = 1;
        kittensObs.DrawAtEnd = true;

        return Task.CompletedTask;
    }
    
    private async Task<IAction> GetActionFromPlayer(IObservation specialObservation, int index)
    {
        KittensObservation kittensOb = (KittensObservation) specialObservation;
        return await Task.Run(() => kittensOb.PlayersStatus[index].Player.Think(specialObservation, GameManager.TimeToThink));
    }
}
