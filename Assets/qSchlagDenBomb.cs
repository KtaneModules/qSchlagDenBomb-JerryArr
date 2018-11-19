using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;

public class qSchlagDenBomb : MonoBehaviour
{
    public KMSelectable[] buttonsB;
    public KMSelectable[] buttonsC;
    public KMSelectable[] buttonsU;
	public KMSelectable buttonSubmit;
    public MeshRenderer contenderName;
    public MeshRenderer contenderScore;
    public MeshRenderer bombScore;

    public MeshRenderer[] meshB;
    public MeshRenderer[] meshC;
    public MeshRenderer[] meshUn;

    public KMBombInfo bomb;
    //public new KMAudio audio;

    string[] gameType = new string[15];
    string[] contender = new string[27] 
        { "RON", "DON", "JULIA", "CORY", "GREG", "PAULA", "VAL", "LISA", "OZY",
        "OZZY", "ELSA", "CORI", "HARRY", "GALE", "DANIEL", "ALBERT", "SPIKE", "TOMMY",
        "GRETA", "TINA", "ROB", "EDGAR", "JULIE", "PETER", "MILLIE", "ISOLDE", "ERIS"};
    // Physical, Mental, Quiz
    int[] ratings = new int[27]
      { 000, 001, 002, 010, 011, 012, 020, 021, 022,
        100, 101, 102, 110, 111, 112, 120, 121, 122,
        200, 201, 202, 210, 211, 212, 220, 221, 222};
    string[] alphabet = new string[26]
        { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
          "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

    int[,] portSteps = new int[7, 3] {
        {2, 19, 7 },
        {3, 12, 4 },
        {11, 1, 6 },
        {3, 3, 3 },
        {11, 9, 10 },
        {7, 2, 9 },
        {6, 14, 3 },    };

    int[,] batterySteps = new int[6, 3] {
        {11, 3, 4 },
        {6, 2, 8 },
        {3, 7, 1 },
        {4, 9, 1 },
        {11, 9, 2 },
        {7, 11, 4 },    };

    int[,] indicatorSteps = new int[4, 3] {
        {1, 1, 1 },
        {1, 2, 1 },
        {1, 1, 1 },
        {1, 2, 1 },    };

    bool[] unplayedGames = new bool[15];
    string realUnplayed = "";
    int contenderNumber;
    int contenderRatings;
    int scoreC;
    int scoreB;
    bool[] contenderWins = new bool[15];
    string[] curGameState = new string[15];

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    int numBatteries;
    int numIndicators;
    int numPorts;
    string[] snCharacter = new string[6];
    int[] snValue = new int[6];

    bool pressedAllowed = false;

    string[] testCharacters = new string[10]
    {
        "", "", "", "", "", "", "", "", "", ""
    };

    // TWITCH PLAYS SUPPORT
    int tpStages;
    // TWITCH PLAYS SUPPORT
    //                           bg is off             bg and unplayed text if bg off          bomb text if bg off      contestant text if bg off
    Color[] colory = { new Color(0.05f, 0.05f, 0.05f), new Color(0.95f, 0.95f, 0.95f), new Color(.45f, .45f, .9f), new Color(.9f, .3f, .3f) };

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        Init();
    }

    void Init()
    {
        realUnplayed = testCharacters[1];
        for (int q = 0; q < 15; q++)
        {
            gameType[q] = "X";
            contenderWins[q] = false;
            unplayedGames[q] = false;
            curGameState[q] = "B";
            meshC[q].material.color = colory[0];
            meshB[q].material.color = colory[1];
            buttonsB[q].GetComponentInChildren<TextMesh>().color = colory[0];
            buttonsC[q].GetComponentInChildren<TextMesh>().color = colory[3];
        }
        DumbDelegationThing();
        buttonSubmit.OnInteractEnded += delegate () { PressedSubmit(); buttonSubmit.AddInteractionPunch(0.4f); };
        buttonsU[0].OnInteractEnded += delegate () { PressedUnplayed(0); buttonsU[0].AddInteractionPunch(0.2f); };
        buttonsU[1].OnInteractEnded += delegate () { PressedUnplayed(1); buttonsU[1].AddInteractionPunch(0.2f); };
        buttonsU[2].OnInteractEnded += delegate () { PressedUnplayed(2); buttonsU[2].AddInteractionPunch(0.2f); };
        buttonsU[3].OnInteractEnded += delegate () { PressedUnplayed(3); buttonsU[3].AddInteractionPunch(0.2f); };
        meshUn[0].material.color = colory[0];
        meshUn[1].material.color = colory[0];
        meshUn[2].material.color = colory[0];
        meshUn[3].material.color = colory[0];
        buttonsU[0].GetComponentInChildren<TextMesh>().color = colory[1];
        buttonsU[1].GetComponentInChildren<TextMesh>().color = colory[1];
        buttonsU[2].GetComponentInChildren<TextMesh>().color = colory[1];
        buttonsU[3].GetComponentInChildren<TextMesh>().color = colory[1];
        numPorts = Math.Min(6, bomb.GetPortCount());
        numBatteries = Math.Min(5, bomb.GetBatteryCount());
        numIndicators = Math.Min(3, bomb.GetIndicators().Count());

        for (int i = 0; i < 6; i++)
        {
            snCharacter[i] = bomb.GetSerialNumber().Substring(i,1);
            if (snCharacter[i].Contains("1") || snCharacter[i].Contains("2") || snCharacter[i].Contains("3") ||
                snCharacter[i].Contains("4") || snCharacter[i].Contains("5") || snCharacter[i].Contains("6") ||
                snCharacter[i].Contains("7") || snCharacter[i].Contains("8") || snCharacter[i].Contains("9") ||
                snCharacter[i].Contains("0"))
            {
                snValue[i] = Int32.Parse(snCharacter[i]);
            }
            else
            {
                int cLetterNumber = 0;
                while (alphabet[cLetterNumber] != snCharacter[i])
                {
                    cLetterNumber++;
                }
                snValue[i] = 1 + cLetterNumber;
            }
        }
        
        tpStages = 0;
        contenderNumber = UnityEngine.Random.Range(0, 27);
        //TextMesh contenderText = c.GetComponentInChildren<TextMesh>();

        TextMesh contenderText = contenderName.GetComponent<TextMesh>();

        contenderText.text = contender[contenderNumber];
        contenderRatings = ratings[contenderNumber];
        Debug.LogFormat("[Schlag den Bomb #{0}] Contender name is {1}. Ports = {2}, Batteries = {3}, Indicators = {4} (Over 6/5/3 counts as 6/5/3)", _moduleId, 
            contender[contenderNumber], numPorts, numBatteries, numIndicators);
        Debug.LogFormat("[Schlag den Bomb #{0}] SN {1}, values {2} {3} {4} {5} {6} {7}", _moduleId, bomb.GetSerialNumber(), snValue[0], snValue[1], snValue[2], snValue[3], snValue[4], snValue[5]);

        bool inLoop = false;
        int curGameTypeNum = 0;

        if (snValue[0] == 0)
        {
            snValue[0] = 15;
        }
        else if(snValue[0] > 15)
        {
            snValue[0] = snValue[0] - 15;
        }
        for (int pl = 0; pl < 6; pl++)
        {
            if (snValue[pl] == 0)
            {
                snValue[pl] = 1;
            }
        }
        gameType[snValue[0] - 1] = "O";
        //Debug.LogFormat("[Schlag den Bomb #{0}] Game {1} is now an O.", _moduleId, snValue[0]);
        int assignsLeft = 5;
        int stepsLeft = 0;
        int currentStep = snValue[0] - 1;
        //Debug.Log("Starting on game " + (snValue[0] - 1));
        
        while (assignsLeft > 0)
        {
            // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4
            // 3 2 1 0
            stepsLeft = snValue[6 - assignsLeft];

            while (stepsLeft > 0 || !inLoop)
            {
                inLoop = true;
                if (gameType[currentStep] != "X")
                {
                    stepsLeft++;
                    //Debug.Log("Game " + currentStep + " is already type " + gameType[currentStep] + ".");
                }
                stepsLeft--;
                if (stepsLeft != 0 && (snValue[6 - assignsLeft] != 0))
                {
                    currentStep++;
                }
                

                if (currentStep > 14)
                {
                    currentStep = 0;
                }
                //Debug.LogFormat("Step to {0}, which is a {1}. {2} step(s) left.", currentStep, gameType[currentStep], stepsLeft);
            }

            gameType[currentStep] = "O";
            //Debug.LogFormat("[Schlag den Bomb #{0}] Game {0} is now an O.", _moduleId, currentStep + 1);
            assignsLeft--;
            inLoop = false;
        }
        curGameTypeNum = (contenderRatings / 100);
        Debug.LogFormat("[Schlag den Bomb #{0}] Phys rating = {1}", _moduleId, curGameTypeNum);
        assignsLeft = 3;
        stepsLeft = 0;
        while (assignsLeft > 0)
        {
            // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4
            // 3 2 1 0
            stepsLeft = portSteps[numPorts, 3 - assignsLeft];

            while (stepsLeft > 0 || !inLoop)
            {
                inLoop = true;
                if (gameType[currentStep] != "X")
                {
                    stepsLeft++;
                    //Debug.Log("Game " + currentStep + " is already type " + gameType[currentStep] + ".");
                }
                stepsLeft--;
                if (stepsLeft != 0 && (snValue[3 - assignsLeft] != 0))
                {
                    currentStep++;
                }


                if (currentStep > 14)
                {
                    currentStep = 0;
                }
                //Debug.LogFormat("Step to {0}, which is a {1}. {2} step(s) left.", currentStep, gameType[currentStep], stepsLeft);
            }

            gameType[currentStep] = "P";
            Debug.LogFormat("[Schlag den Bomb #{0}] Game {1} is now a P.", _moduleId, currentStep + 1);
            if (3 - assignsLeft < curGameTypeNum)
            {
                Debug.LogFormat("[Schlag den Bomb #{0}] Contender wins game {1}.", _moduleId, currentStep + 1);
                contenderWins[currentStep] = true;
            }
            assignsLeft--;
        }
        curGameTypeNum = (contenderRatings - ((contenderRatings / 100)*100)) / 10;
        Debug.LogFormat("[Schlag den Bomb #{0}] Mental rating = {1}", _moduleId, curGameTypeNum);
        assignsLeft = 3;
        stepsLeft = 0;
        inLoop = false;
        while (assignsLeft > 0)
        {
            // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4
            // 3 2 1 0
            stepsLeft = batterySteps[numBatteries, 3 - assignsLeft];

            while (stepsLeft > 0 || !inLoop)
            {
                inLoop = true;
                if (gameType[currentStep] != "X")
                {
                    stepsLeft++;
                    //Debug.Log("Game " + currentStep + " is already type " + gameType[currentStep] + ".");
                }
                stepsLeft--;
                if (stepsLeft != 0 && (snValue[3 - assignsLeft] != 0))
                {
                    currentStep++;
                }


                if (currentStep > 14)
                {
                    currentStep = 0;
                }
                //Debug.LogFormat("Step to {0}, which is a {1}. {2} step(s) left.", currentStep, gameType[currentStep], stepsLeft);
            }

            gameType[currentStep] = "M";
            Debug.LogFormat("[Schlag den Bomb #{0}] Game {1} is now an M.", _moduleId, currentStep + 1);
            if (3 - assignsLeft < curGameTypeNum)
            {
                Debug.LogFormat("[Schlag den Bomb #{0}] Contender wins game {1}.", _moduleId, currentStep + 1);
                contenderWins[currentStep] = true;
            }
            assignsLeft--;
        }
        curGameTypeNum = (contenderRatings) - (100 * (contenderRatings / 100)) - (10 * ((contenderRatings - ((contenderRatings / 100) * 100)) / 10));
        Debug.LogFormat("[Schlag den Bomb #{0}] Quiz rating = {1}", _moduleId, curGameTypeNum);
        assignsLeft = 3;
        stepsLeft = 0;
        inLoop = false;
        while (assignsLeft > 0)
        {
            // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4
            // 3 2 1 0
            stepsLeft = indicatorSteps[numIndicators, 3 - assignsLeft];

            while (stepsLeft > 0 || !inLoop)
            {
                inLoop = true;
                if (gameType[currentStep] != "X")
                {
                    stepsLeft++;
                    //Debug.Log("Game " + (currentStep + 1) + " is already type " + gameType[currentStep] + ".");
                }
                stepsLeft--;
                if (stepsLeft != 0 && (snValue[3 - assignsLeft] != 0))
                {
                    currentStep++;
                }


                if (currentStep > 14)
                {
                    currentStep = 0;
                }
                //Debug.LogFormat("Step to {0}, which is a {1}. {2} step(s) left.", currentStep + 1, gameType[currentStep], stepsLeft);
            }

            gameType[currentStep] = "Q";
            Debug.LogFormat("[Schlag den Bomb #{0}] Game {1} is now a Q.", _moduleId, currentStep + 1);
            if (3 - assignsLeft < curGameTypeNum)
            {
                Debug.LogFormat("[Schlag den Bomb #{0}] Contender wins game {1}.", _moduleId, currentStep + 1);
                contenderWins[currentStep] = true;
            }
            assignsLeft--;
        }
        //Debug.LogFormat("Game order: ");
        string oddities = "";
        for (int j = 0; j < 15; j++)
        {
            //Debug.LogFormat("Game {0} is {1}", ((j + 1)), gameType[j]);
            // Debug.LogFormat("Game {0} is {1}", (j + 1); gameType[j]);
            //Debug.Log("Game " + (j + 1) + " is " + gameType[j]);
            if (gameType[j] == "O")
            {
                oddities = oddities + (j+1) + " ";

                if (UnityEngine.Random.Range(0,2) == 1)
                {
                    //Debug.LogFormat("Contender wins game {0}.", (j + 1));
                    contenderWins[j] = true;
                }
            }

        }
        Debug.LogFormat("[Schlag den Bomb #{0}] Oddball games = {1}", _moduleId, oddities);
        scoreC = 0;
        scoreB = 0;
        for (int fs = 0; fs < 15; fs++)
        {
            if (scoreC > 60 || scoreB > 60)
            {
                unplayedGames[fs] = true;
                Debug.LogFormat("[Schlag den Bomb #{0}] Game number {1} is unplayed.", _moduleId, fs+1);
                realUnplayed = realUnplayed + (1 + fs) + " ";
                //Debug.Log(realUnplayed);
            }
            else if (contenderWins[fs])
            {
                scoreC = scoreC + 1 + fs;
                //Debug.Log("Contender score " + scoreC);
            }
            else
            {
                scoreB = scoreB + 1 + fs;
                //Debug.Log("Bomb score " + scoreB);
            }
        }
        if (scoreC == 60)
        {
            Debug.LogFormat("[Schlag den Bomb #{0}] 60-60 tie detected, flipping result of first oddball game. (Remember, oddball games need not be exactly what is determined, as long as all selected games total up correctly)", _moduleId);
            bool uhOh = true;
            curGameTypeNum = 0;
            while (uhOh)
            {
                if (gameType[curGameTypeNum] == "O")
                {
                    contenderWins[curGameTypeNum] = !contenderWins[curGameTypeNum];
                    Debug.LogFormat("[Schlag den Bomb #{0}] Game number {1} result flipped, contestant wins this game = {2}", _moduleId, curGameTypeNum + 1, contenderWins[curGameTypeNum]);
                    uhOh = false;
                    scoreC = 0;
                    scoreB = 0;
                    for (int fs = 0; fs < 15; fs++)
                    {
                        if (contenderWins[fs])
                        {
                            scoreC = scoreC + 1 + fs;
                        }
                        else
                        {
                            scoreB = scoreB + 1 + fs;
                        }
                    }
                }
            }
        }
        Debug.LogFormat("[Schlag den Bomb #{0}] Final score: Contender {1} - {2} Bomb. Contender {3}.", _moduleId, scoreC, scoreB, scoreC > 60 ? "wins" : "loses");
        TextMesh contyScore = contenderScore.GetComponent<TextMesh>();
        TextMesh bombaScore = bombScore.GetComponent<TextMesh>();

        contyScore.text = Convert.ToString(scoreC);
        bombaScore.text = Convert.ToString(scoreB);
        pressedAllowed = true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Assign games with !{0} (contestant/bomb/unplayed) 1 2 3 or !{0} (c/b/u) 1 2 3 with a space in between numbers, for example, !{0} u 13 14 15. Submit with !{0} submit.";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        //var pieces = command.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //var presses = command.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string theError;
        /* theError = "sendtochat Pieces I got were, starting with the first one = ";
        for (int jojo = 0; jojo < pieces.Length; jojo++)
        {
            theError = theError + jojo + " was " + pieces[jojo] + ", ";
        }
        Debug.Log(theError); */
        //yield return theError;
        //Debug.Log(pieces.Count());
        //Debug.Log(pieces.Length);
        if (pieces.Count() == 0)
        {
            theError = "sendtochat Not enough arguments! You need at least 'contestant/bomb/unplayed', 'c/b/u', or 'submit', then one or more numbers, separated by spaces.";
            yield return theError;
        }
        if (pieces.Count() == 1 && pieces[0] == "submit")
        {
            PressedSubmit();
            yield return null;
        }
        if (pieces.Count() == 1 && pieces[0] != "submit")
        {
            theError = "sendtochat Not enough arguments! You need at least 'contestant/bomb/unplayed' or 'c/b/u', or 'submit', then one or more numbers, separated by spaces.";
            yield return theError;
        }
        else if (pieces[0] != "contestant" && pieces[0] != "c" &&
            pieces[0] != "bomb" && pieces[0] != "b" &&
            pieces[0] != "unplayed" && pieces[0] != "u" && pieces[0] != "submit"
            )
        {
            theError = "sendtochat You made a boo boo! Command '" + pieces[0] + "' is invalid. You must use 'contestant/bomb/unplayed' or 'c/b/u'.";
            yield return theError;
        }
        else if ((pieces.Count() > 1))
        {
            tpStages = pieces.Length - 1;
            //Debug.Log(pieces.Length - tpStages);
            if (pieces[0] == "contestant" || pieces[0] == "c")
            {
                while (tpStages > 0)
                {
                    yield return new WaitForSeconds(.1f);
                    var curPiece = Int32.Parse(pieces[pieces.Count() - tpStages]);

                    if (curPiece > 0 && curPiece < 16)
                    {
                        PressedContender(Int32.Parse(pieces[pieces.Count() - tpStages]) - 1);
                        tpStages--;

                    }
                    else
                    {
                        tpStages = 0;
                        theError = "sendtochat You made a boo boo! 'contestant/c' command '" + pieces[(pieces.Length - tpStages) - 1] + 
                            "' is invalid. You must use a number from 1 to 15.";
                        yield return theError;
                    }
                    
                }
            }
            else if (pieces[0] == "bomb" || pieces[0] == "b")
            {
                while (tpStages > 0)
                {
                    yield return new WaitForSeconds(.1f);
                    var curPiece = Int32.Parse(pieces[pieces.Count() - tpStages]);

                    if (curPiece > 0 && curPiece < 16)
                    {
                        PressedBomb(Int32.Parse(pieces[pieces.Count() - tpStages]) - 1);
                        tpStages--;

                    }
                    else
                    {
                        tpStages = 0;
                        theError = "sendtochat You made a boo boo! 'bomb/b' command '" + pieces[(pieces.Length - tpStages) - 1] + 
                            "' is invalid. You must use a number from 1 to 15.";
                        yield return theError;
                    }
                    
                }
            }
            else if (pieces[0] == "unplayed" || pieces[0] == "u")
            {
                while (tpStages > 0)
                {
                    yield return new WaitForSeconds(.1f);
                    var curPiece = Int32.Parse(pieces[pieces.Count() - tpStages]);

                    if (curPiece > 11 && curPiece < 16)
                    {
                        PressedUnplayed(curPiece - 12);
                        tpStages--;

                    }
                    else
                    {
                        tpStages = 0;
                        theError = "sendtochat You made a boo boo! 'unplayed/u' command '" + pieces[(pieces.Length - tpStages) - 1] + 
                            "' is invalid. You must use a number from 12 to 15 for unplayed games.";
                        yield return theError;
                    }
                }
            }

            yield return null;
        }
        
    }
    void PressedBomb(int pressedButton)
    {
        curGameState[pressedButton] = "B";
        meshC[pressedButton].material.color = colory[0];
        meshB[pressedButton].material.color = colory[1];
        buttonsC[pressedButton].GetComponentInChildren<TextMesh>().color = colory[3];
        buttonsB[pressedButton].GetComponentInChildren<TextMesh>().color = colory[0];
        if (pressedButton > 10)
        {
            meshUn[pressedButton - 11].material.color = colory[0];
            buttonsU[pressedButton - 11].GetComponentInChildren<TextMesh>().color = colory[1];
        }
    }

    void PressedContender(int pressedButton)
    {
        curGameState[pressedButton] = "C";
        meshB[pressedButton].material.color = colory[0];
        meshC[pressedButton].material.color = colory[1];
        buttonsB[pressedButton].GetComponentInChildren<TextMesh>().color = colory[2];
        buttonsC[pressedButton].GetComponentInChildren<TextMesh>().color = colory[0];
        if (pressedButton > 10)
        {
            meshUn[pressedButton - 11].material.color = colory[0];
            buttonsU[pressedButton - 11].GetComponentInChildren<TextMesh>().color = colory[1];
        }
    }

    void PressedUnplayed(int pressedButton)
    {
        curGameState[pressedButton + 11] = "U";
        meshUn[pressedButton].material.color = colory[1];
        meshB[pressedButton + 11].material.color = colory[0];
        meshC[pressedButton + 11].material.color = colory[0];

        buttonsU[pressedButton].GetComponentInChildren<TextMesh>().color = colory[0];
        buttonsB[pressedButton + 11].GetComponentInChildren<TextMesh>().color = colory[2];
        buttonsC[pressedButton + 11].GetComponentInChildren<TextMesh>().color = colory[3];
    }

    void PressedSubmit()
    {
        int testingCScore = 0;
        string testingCGames = "";
        string screwedUp = "";
        string unplayedGamesZone = "";
        if (pressedAllowed)
        {

            for (int trying = 0; trying < 15; trying++)
            {
                //Debug.Log("Game " + (trying + 1));
                if (curGameState[trying] == "C")
                {
                    testingCScore = testingCScore + (1 + trying);
                    testingCGames = testingCGames + (1 + trying) + " ";
                    if (gameType[trying] != "O" && contenderWins[trying] == false)
                    {
                        screwedUp = "shoulda lost";
                    }
                }
                else if (curGameState[trying] == "B" && unplayedGames[trying] == false)
                {
                    if (gameType[trying] != "O" && contenderWins[trying] == true)
                    {
                        screwedUp = "shoulda won";
                    }
                }
                else if (curGameState[trying] == "U")
                {
                    unplayedGamesZone = unplayedGamesZone + (1 + trying) + " ";
                    //Debug.Log("Game u" + (trying + 1));
                    if (unplayedGames[trying] == true)
                    {
                        //Debug.Log("Game r" + (trying + 1));
                        
                    }

                }

 
            }
            //Debug.Log("Testing = " + unplayedGamesZone + " Real = " + realUnplayed);
            if (testingCScore != scoreC && (screwedUp != "shoulda lost" && screwedUp != "shoulda won"))
            {
                //Debug.Log("Testing " + testingCScore);
                //Debug.Log("Real " + scoreC);
                screwedUp = "shoulda oddballed";
            }
            if (screwedUp == "")
            {
                Debug.LogFormat("[Schlag den Bomb #{0}] Submitted {1}for contender. All required games marked and your total games marked equals {2}, a match.", _moduleId,
                    testingCGames, testingCScore);
                if (unplayedGamesZone == realUnplayed)
                {
                    if (unplayedGamesZone == "")
                    {
                        Debug.LogFormat("[Schlag den Bomb #{0}] Submitted no games as unplayed. This is correct.", _moduleId);
                    }
                    else
                    {
                        Debug.LogFormat("[Schlag den Bomb #{0}] Submitted ({1}) as unplayed and ({2}) was expected. This is correct.", _moduleId, unplayedGamesZone, realUnplayed);
                    }
                    Debug.LogFormat("[Schlag den Bomb #{0}] Richtig! Module defused! One moment in time...", _moduleId);
                    pressedAllowed = false;
                    GetComponent<KMBombModule>().HandlePass();
                    GetComponent<KMAudio>().PlaySoundAtTransform("OneMomentInTime", transform);
                    return;
                }
                else
                {
                    if (unplayedGamesZone == "")
                    {
                        Debug.LogFormat("[Schlag den Bomb #{0}] Submitted no games as unplayed. This is incorrect.", _moduleId);
                    }
                    else
                    {
                        Debug.LogFormat("[Schlag den Bomb #{0}] Submitted ({1}) as unplayed when ({2}) was expected. This is incorrect.", _moduleId, unplayedGamesZone, realUnplayed);
                    }
                    Debug.LogFormat("[Schlag den Bomb #{0}] Falsch! Strike given...", _moduleId);
                    GetComponent<KMBombModule>().HandleStrike();
                    return;
                }

            }
            else
            {
                //Debug.Log(screwedUp);
                if (screwedUp == "shoulda won" || screwedUp == "shoulda lost")
                {
                    Debug.LogFormat("[Schlag den Bomb #{0}] Submitted {1}for contender. Incorrect!", _moduleId, testingCGames);
                }
                else //if (screw)
                {
                    Debug.LogFormat("[Schlag den Bomb #{0}] Submitted {1}for contender. Oddball games are incorrect!", _moduleId, testingCGames);
                }
                Debug.LogFormat("[Schlag den Bomb #{0}] Falsch! Strike given...", _moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                return;
            }
        }
        

    }

    void OnPress()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
    }

    void OnRelease(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if (pressedAllowed)
        {

            return;
        }

    }
    void DumbDelegationThing()
    {
        buttonsB[0].OnInteractEnded += delegate () { PressedBomb(0); buttonsB[0].AddInteractionPunch(0.2f); };
        buttonsB[1].OnInteractEnded += delegate () { PressedBomb(1); buttonsB[1].AddInteractionPunch(0.2f); };
        buttonsB[2].OnInteractEnded += delegate () { PressedBomb(2); buttonsB[2].AddInteractionPunch(0.2f); };
        buttonsB[3].OnInteractEnded += delegate () { PressedBomb(3); buttonsB[3].AddInteractionPunch(0.2f); };
        buttonsB[4].OnInteractEnded += delegate () { PressedBomb(4); buttonsB[4].AddInteractionPunch(0.2f); };
        buttonsB[5].OnInteractEnded += delegate () { PressedBomb(5); buttonsB[5].AddInteractionPunch(0.2f); };
        buttonsB[6].OnInteractEnded += delegate () { PressedBomb(6); buttonsB[6].AddInteractionPunch(0.2f); };
        buttonsB[7].OnInteractEnded += delegate () { PressedBomb(7); buttonsB[7].AddInteractionPunch(0.2f); };
        buttonsB[8].OnInteractEnded += delegate () { PressedBomb(8); buttonsB[8].AddInteractionPunch(0.2f); };
        buttonsB[9].OnInteractEnded += delegate () { PressedBomb(9); buttonsB[9].AddInteractionPunch(0.2f); };
        buttonsB[10].OnInteractEnded += delegate () { PressedBomb(10); buttonsB[10].AddInteractionPunch(0.2f); };
        buttonsB[11].OnInteractEnded += delegate () { PressedBomb(11); buttonsB[11].AddInteractionPunch(0.2f); };
        buttonsB[12].OnInteractEnded += delegate () { PressedBomb(12); buttonsB[12].AddInteractionPunch(0.2f); };
        buttonsB[13].OnInteractEnded += delegate () { PressedBomb(13); buttonsB[13].AddInteractionPunch(0.2f); };
        buttonsB[14].OnInteractEnded += delegate () { PressedBomb(14); buttonsB[14].AddInteractionPunch(0.2f); };
        buttonsC[0].OnInteractEnded += delegate () { PressedContender(0); buttonsC[0].AddInteractionPunch(0.2f); };
        buttonsC[1].OnInteractEnded += delegate () { PressedContender(1); buttonsC[1].AddInteractionPunch(0.2f); };
        buttonsC[2].OnInteractEnded += delegate () { PressedContender(2); buttonsC[2].AddInteractionPunch(0.2f); };
        buttonsC[3].OnInteractEnded += delegate () { PressedContender(3); buttonsC[3].AddInteractionPunch(0.2f); };
        buttonsC[4].OnInteractEnded += delegate () { PressedContender(4); buttonsC[4].AddInteractionPunch(0.2f); };
        buttonsC[5].OnInteractEnded += delegate () { PressedContender(5); buttonsC[5].AddInteractionPunch(0.2f); };
        buttonsC[6].OnInteractEnded += delegate () { PressedContender(6); buttonsC[6].AddInteractionPunch(0.2f); };
        buttonsC[7].OnInteractEnded += delegate () { PressedContender(7); buttonsC[7].AddInteractionPunch(0.2f); };
        buttonsC[8].OnInteractEnded += delegate () { PressedContender(8); buttonsC[8].AddInteractionPunch(0.2f); };
        buttonsC[9].OnInteractEnded += delegate () { PressedContender(9); buttonsC[9].AddInteractionPunch(0.2f); };
        buttonsC[10].OnInteractEnded += delegate () { PressedContender(10); buttonsC[10].AddInteractionPunch(0.2f); };
        buttonsC[11].OnInteractEnded += delegate () { PressedContender(11); buttonsC[11].AddInteractionPunch(0.2f); };
        buttonsC[12].OnInteractEnded += delegate () { PressedContender(12); buttonsC[12].AddInteractionPunch(0.2f); };
        buttonsC[13].OnInteractEnded += delegate () { PressedContender(13); buttonsC[13].AddInteractionPunch(0.2f); };
        buttonsC[14].OnInteractEnded += delegate () { PressedContender(14); buttonsC[14].AddInteractionPunch(0.2f); };

    }
}
