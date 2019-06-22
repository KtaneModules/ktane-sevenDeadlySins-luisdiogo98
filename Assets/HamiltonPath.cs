using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class HamiltonPath {
	static System.Random rnd = new System.Random();
    int[] nodes;
    int[][] edges;

    public HamiltonPath(int[] nodes, int[][] edges)
    {
        this.nodes = nodes;
        this.edges = edges;
    }

    public int[] GetPath()
    {
        if(nodes == null || edges == null || edges.Length != nodes.Length)
            return null;

        int[] solution = new int[nodes.Length + 1];
        Dictionary<int, int[]> candidates = new Dictionary<int, int[]>();
    
        nodes = nodes.OrderBy(x => rnd.Next()).ToArray();  
        candidates.Add(1, nodes);
        return this.BuildPath(1, solution, candidates);
    }

    int[] BuildPath(int k, int[] solution, Dictionary<int, int[]> candidates)
    {
        if(k == nodes.Length + 1)
            return solution;

        int[] candListTemp;
        candidates.TryGetValue(k, out candListTemp);
        List<int> candList = candListTemp.ToList();

        if(candList == null || candList.Count == 0)
        {
            CleanSolution(k, solution);
            return BuildPath(k-1, solution, candidates);
        }
        else
        {
            int firstCandidate = candList[0];
            candList.RemoveAt(0);
            candidates.Remove(k);
            candidates.Add(k, candList.ToArray());
            solution[k] = firstCandidate;

            int[] neighbors = ((int[]) edges[firstCandidate - 1].Clone()).OrderBy(x => rnd.Next()).ToArray();
            int[] prevElems = GetPrevElems(k, solution);
            candidates.Remove(k+1);
            candidates.Add(k+1, GenerateCandidates(neighbors, prevElems));

            return BuildPath(k+1, solution, candidates);
        }
    }

    int[] GenerateCandidates(int[] neighbors, int[] prevElems)
    {
        List<int> res = new List<int>();

        for(int i=0; i < neighbors.Length; i++)
            if(!Array.Exists(prevElems, x => x == neighbors[i]))
                res.Add(neighbors[i]);

        return res.ToArray();
    }

    int[] GetPrevElems(int k, int[] solution)
    {
        List<int> res = new List<int>();
        for(int i=1; i <= k-1; i++)
            res.Add(solution[i]);

        return res.ToArray();
    }

    void CleanSolution(int k, int[] solution)
    {
        for(int i = k; i < solution.Length; i++)
            solution[i] = 0;
    }
}