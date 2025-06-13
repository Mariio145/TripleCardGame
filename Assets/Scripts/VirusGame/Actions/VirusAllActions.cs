using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirusGame.Actions
{
    public class VirusAllActions
    {
        public List<VirusAction> AllPosibleActions;
        
        public VirusAllActions(int numPlayers)
        {
            List<VirusColor> virusColors = ((VirusColor[])Enum.GetValues(typeof(VirusColor))).ToList();
            AllPosibleActions = new List<VirusAction>();
            virusColors.Remove(VirusColor.None);
            
            AllPosibleActions.Add(new VirusActionLatexGlove(-1));  //LatexGolve
            for (int i = 0; i < numPlayers; i++) 
            {
                foreach (VirusColor colorSelf in virusColors)
                { 
                    AllPosibleActions.Add(new VirusActionPlayOrgan(colorSelf, i, -1)); //PlayOrgan
                    
                    foreach (VirusColor colorVirus in virusColors)
                    {
                        if (colorVirus == VirusColor.Rainbow || colorSelf == VirusColor.Rainbow ||
                            colorSelf == colorVirus)
                        {
                            AllPosibleActions.Add(new VirusActionPlayMedicine(colorSelf, i, colorVirus, -1)); //PlayMedicine
                            AllPosibleActions.Add(new VirusActionPlayVirus(colorSelf, i, colorVirus, -1)); //PlayVirus
                        }
                        
                        for (int j = 0; j < numPlayers; j++)
                        {
                            if (i == j) continue;
                            AllPosibleActions.Add(new VirusActionPlayVirus(colorSelf, i, colorVirus, -1)); //Spread
                            AllPosibleActions.Add(new VirusActionPlayVirus(colorSelf, i, colorVirus, -1)); //Transplant
                        }
                    }
                    
                    for (int j = 0; j < numPlayers; j++)
                    {
                        if (i == j) continue;
                        AllPosibleActions.Add(new VirusActionOrganThief(colorSelf, i,j, -1)); //OrganThief
                    }
                }
                
               
                for (int j = 0; j < numPlayers; j++)
                {
                    if (i == j) continue;
                    AllPosibleActions.Add(new VirusActionMedicalError( i,j, -1)); // MedicalError
                }
                
                int[] cardIndex = { 0, 1, 2};
                AllPosibleActions.AddRange(Auxiliar<int>.GetCombinations(cardIndex).Select(combination => new VirusActionDiscard(combination, i)));
            }
            
            AllPosibleActions.Add(new VirusAction());
        }
    }
}