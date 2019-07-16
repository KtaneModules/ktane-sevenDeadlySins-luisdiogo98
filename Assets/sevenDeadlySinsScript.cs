using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class sevenDeadlySinsScript : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	int[] nodes = {1, 2, 3, 4, 5, 6, 7};
	int[][] edges = new int[][] { 
									new int[] {2, 3, 5},
									new int[] {3, 4, 6},
									new int[] {4, 5, 7},
									new int[] {1, 5, 6},
									new int[] {2, 6, 7},
									new int[] {1, 3, 7},
									new int[] {1, 2, 4},
								};

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;

	public KMSelectable[] btns;
	public Material[] colors;
	public Material[] symbols;

	int[] possibleSolution;
	int[] btnOrder;
	int adjLeft = -1;
	int adjRight = -1;
	int lastPress;

	List<int> presses = new List<int>();

	void Awake()
	{
		moduleId = moduleIdCounter++;

		btns[0].OnInteract += delegate () { HandlePress(0); return false; };
		btns[1].OnInteract += delegate () { HandlePress(1); return false; };
		btns[2].OnInteract += delegate () { HandlePress(2); return false; };
		btns[3].OnInteract += delegate () { HandlePress(3); return false; };
		btns[4].OnInteract += delegate () { HandlePress(4); return false; };
		btns[5].OnInteract += delegate () { HandlePress(5); return false; };
		btns[6].OnInteract += delegate () { HandlePress(6); return false; };
	}

	void Start () 
	{
		CalcPossibleSolution();
		SetUpButtons();
	}
	
	void CalcPossibleSolution()
	{
		HamiltonPath graph = new HamiltonPath(nodes, edges);
		possibleSolution = graph.GetPath();

		Debug.LogFormat("[Seven Deadly Sins #{0}] A possible button press order is [{1}] (there may be others).", moduleId, GetSinString(possibleSolution));
	}

	void SetUpButtons()
	{
		btnOrder = new int[7];

		int startPos = rnd.Range(0, 7);
		int leftBound = startPos - 1;
		int rightBound = startPos + 1;

		if(leftBound == -1)
			leftBound = 6;
		if(rightBound == 7)
			rightBound = 0;

		btnOrder[startPos] = possibleSolution[1];
		btns[startPos].GetComponentInChildren<Renderer>().material = colors[possibleSolution[1] - 1];

		for(int i = 2; i <= 7; i++)
		{
			if(rnd.Range(0, 2) == 0)
			{
				btnOrder[leftBound] = possibleSolution[i];
				btns[leftBound].GetComponentInChildren<Renderer>().material = colors[possibleSolution[i] - 1];
				leftBound--;
				if(leftBound == -1)
					leftBound = 6;
			}
			else
			{
				btnOrder[rightBound] = possibleSolution[i];
				btns[rightBound].GetComponentInChildren<Renderer>().material = colors[possibleSolution[i] - 1];
				rightBound++;
				if(rightBound == 7)
					rightBound = 0;
			}
		}

		Debug.LogFormat("[Seven Deadly Sins #{0}] Button positions are [ {1}].", moduleId, GetSinString(btnOrder));
	}

	void HandlePress(int n)
	{
        btns[n].AddInteractionPunch(.5f);

		if(moduleSolved)
		{
			Audio.PlaySoundAtTransform(GetSinName(btnOrder[n]), transform);
			return;
		}

		if(Array.Exists(presses.ToArray(), x => x == btnOrder[n]))
		{
			Audio.PlaySoundAtTransform(GetSinName(btnOrder[n]), transform);
			return;
		}

		if(adjLeft == -1)
		{
			Audio.PlaySoundAtTransform(GetSinName(btnOrder[n]), transform);

			btns[n].transform.Find("Plane").GetComponentInChildren<Renderer>().material = symbols[btnOrder[n] - 1];
			btns[n].transform.Find("Plane").gameObject.SetActive(true);

			adjLeft = n - 1;
			adjRight = n + 1;

			if(adjLeft == -1)
				adjLeft = 6;
			if(adjRight == 7)
				adjRight = 0;

			lastPress = btnOrder[n];
			presses.Add(btnOrder[n]);

			Debug.LogFormat("[Seven Deadly Sins #{0}] Succefully pressed the {1} button. Current sequence in [ {2}].", moduleId, GetSinName(btnOrder[n]), GetSinString(presses.ToArray()));
		}
		else if(n == adjLeft)
		{
			if(!Array.Exists(edges[lastPress - 1], x => x == btnOrder[n]))
			{
				Debug.LogFormat("[Seven Deadly Sins #{0}] Strike. Pressed the {1} button when there was no connection from {2} to it. Valid connections in the graph were [ {3}].", moduleId, GetSinName(btnOrder[n]), GetSinName(lastPress), GetSinString(edges[lastPress - 1]));
				HandleStrike();
				return;
			}

			Audio.PlaySoundAtTransform(GetSinName(btnOrder[n]), transform);

			btns[n].transform.Find("Plane").GetComponentInChildren<Renderer>().material = symbols[btnOrder[n] - 1];
			btns[n].transform.Find("Plane").gameObject.SetActive(true);

			adjLeft--;
			if(adjLeft == -1)
				adjLeft = 6;

			lastPress = btnOrder[n];
			presses.Add(btnOrder[n]);

			if(presses.Count == 7)
			{
				moduleSolved = true;
				Debug.LogFormat("[Seven Deadly Sins #{0}] Succefully pressed the {1} button. Module solved.", moduleId, GetSinName(btnOrder[n]));
				GetComponent<KMBombModule>().HandlePass();
			}	
			else
			{
				Debug.LogFormat("[Seven Deadly Sins #{0}] Succefully pressed the {1} button. Current sequence in [ {2}].", moduleId, GetSinName(btnOrder[n]), GetSinString(presses.ToArray()));
			}
		}
		else if(n == adjRight)
		{
			if(!Array.Exists(edges[lastPress - 1], x => x == btnOrder[n]))
			{
				Debug.LogFormat("[Seven Deadly Sins #{0}] Strike. Pressed the {1} button when there was no connection from {2} to it. Valid connections in the graph were [ {3}].", moduleId, GetSinName(btnOrder[n]), GetSinName(lastPress), GetSinString(edges[lastPress - 1]));
				HandleStrike();
				return;
			}

			Audio.PlaySoundAtTransform(GetSinName(btnOrder[n]), transform);

			btns[n].transform.Find("Plane").GetComponentInChildren<Renderer>().material = symbols[btnOrder[n] - 1];
			btns[n].transform.Find("Plane").gameObject.SetActive(true);

			adjRight++;
			if(adjRight == 7)
				adjRight = 0;

			lastPress = btnOrder[n];
			presses.Add(btnOrder[n]);

			if(presses.Count == 7)
			{
				moduleSolved = true;
				Debug.LogFormat("[Seven Deadly Sins #{0}] Succefully pressed the {1} button. Module solved.", moduleId, GetSinName(btnOrder[n]));
				GetComponent<KMBombModule>().HandlePass();
			}	
			else
			{
				Debug.LogFormat("[Seven Deadly Sins #{0}] Succefully pressed the {1} button. Current sequence in [ {2}].", moduleId, GetSinName(btnOrder[n]), GetSinString(presses.ToArray()));
			}
		}
		else
		{
			Debug.LogFormat("[Seven Deadly Sins #{0}] Strike. The pressed {1} button was not adjacent to a priviously pressed button. Adjacent buttons were {2} and {3}.", moduleId, GetSinName(btnOrder[n]), GetSinName(btnOrder[adjLeft]), GetSinName(btnOrder[adjRight]));
			HandleStrike();
		}
	}

	void HandleStrike()
	{
		GetComponent<KMBombModule>().HandleStrike();

		foreach(KMSelectable btn in btns)
		{
			btn.transform.Find("Plane").gameObject.SetActive(false);
		}

		adjLeft = -1;
		adjRight = -1;
		presses = new List<int>();
	}

	String GetSinString(int[] seq)
	{
		string res = "";
		for(int i = 0; i < seq.Length; i++)
		{
			res += GetSinName(seq[i]) + " ";
		}
		return res;
	}

	String GetSinName(int n)
	{
		switch(n)
		{
			case 1: return "Lust";
			case 2: return "Gluttony";
			case 3: return "Greed";
			case 4: return "Sloth";
			case 5: return "Wrath";
			case 6: return "Envy";
			case 7: return "Pride";
		}

		return "";
	}
	#region Twitch Plays
    public string TwitchHelpMessage = "Use '!{0} press 1 2 3 4 5 6 7' to press button 1, 2, 3, 4, 5, 6 and 7. The buttons are numbered from 1 to 7 going clockwise starting from the top left.";

    public IEnumerator ProcessTwitchCommand(string command)
    {
		string commfinal=command.Replace("press ", "");
		string[] digitstring = commfinal.Split(' ');
		int tried;
		int index =1;
		foreach(string digit in digitstring){
			if(int.TryParse(digit, out tried)){
				if(index<=7){
					tried=int.Parse(digit);
					index+=1;
					if(tried<8){
						if(tried>0){
					yield return null;
					yield return btns[tried-1];
					yield return btns[tried-1];
						}
						else{
							yield return null;
							yield return "sendtochaterror Number too small!";
							yield break;
						}
					}
					else{
						yield return null;
						yield return "sendtochaterror Number too big!";
						yield break;
					}
				}
				else{
					yield return null;
					yield return "sendtochaterror Too many digits!";
					yield break;
				}
			}
			else{
				yield return null;
				yield return "sendtochaterror Digit not valid.";
				yield break;
			}
		}
	}
    #endregion
}


