using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

public class Manager : MonoBehaviour
{
    public List<GameObject> cities = new List<GameObject>();

    void Start()
    {
        if (true) //40,7331
        {
            float startTime = DateTime.Now.Second + DateTime.Now.Minute * 60f + DateTime.Now.Millisecond * 0.001f;

            Vector3[] citiesV3 = new Vector3[cities.Count];
            for (int i = 0; i < cities.Count; i++)
                citiesV3[i] = cities[i].transform.position;

            (List<int> bestPath, float minDistance) = SolveTSP(citiesV3);

            float endTime = DateTime.Now.Second + DateTime.Now.Minute * 60f + DateTime.Now.Millisecond * 0.001f;

            Debug.Log("Время расчёта: " + (endTime - startTime) + " секунд");
            //Debug.Log("Время расчёта: " + (Time.time - startTime));
            Debug.Log("Минимальное растояние:  " + minDistance);
            Debug.Log("Путь:  " + StringPath(bestPath.ToArray()));
        }
        else if (false)
        {
            Debug.Log(StringPath(CreatetPath(1)));
            int[] parent1 = CreatetPath(2);
            int[] parent2 = CreatetPath(2);
            //Debug.Log(StringPath(parent1) + " + " + StringPath(parent1) + " = " + StringPath(CreatetPath(3, parent1, parent2)));
            Debug.Log("");
            Debug.Log(StringPath(parent1));
            Debug.Log(StringPath(parent2));
            Debug.Log("");

        }
        else if (false)
        {
            int[][] paths = new int[childsCount][];
            for (int i = 0; i < childsCount; i++)
            {
                paths[i] = CreatetPath(1);
            }
            Debug.Log(StringPath(paths[1]));
        }
        //GenAlg();
    }


    string StringPath(int[] path)
    {
        string st = "";
        foreach (int ch in path)
            st += ch + " ";
        return st;
    }

    int[] CreatetPath(int type)
    {
        int[] path = new int[cities.Count];

        if (type == 1)
        {
            for (int i = 0; i < path.Length - 1; i++)
                path[i] = i + 1;
            path[path.Length - 1] = 0;
        }
        else if (type == 2)
        {
            List<int> unvisitedCities = new List<int>();
            for (int i = 0; i < path.Length - 1; i++)
                unvisitedCities.Add(i + 1);

            for (int i = 0; i < path.Length - 1; i++)
            {
                int rand = UnityEngine.Random.Range(0, unvisitedCities.Count);
                path[i] = unvisitedCities[rand];
                unvisitedCities.RemoveAt(rand);
            }

            path[path.Length - 1] = 0;
        }
        else
            Debug.LogError("Incorrect type");

        return path;
    }

    int[] MergePath(int type, int[] parent1, int[] parent2)
    {
        int[] path = new int[cities.Count];

        if (type == 1)
        {
            List<int> unvisitedCities = new List<int>();
            for (int i = 0; i < path.Length - 1; i++)
                unvisitedCities.Add(i + 1);

            for (int i = 0; i < path.Length - 1; i++)
                    path[i] = 0;


            for (int i = 0; i < path.Length - 1; i++)
            {
                if (parent1[i] == parent2[i])
                {
                    path[i] = parent1[i];
                    unvisitedCities.Remove(parent1[i]);
                }
                else if (unvisitedCities.Contains(parent1[i]) && unvisitedCities.Contains(parent2[i]))
                {
                    int r = UnityEngine.Random.Range(0, 2);
                    if (r == 0)
                    {
                        path[i] = parent1[i];
                        unvisitedCities.Remove(parent1[i]);
                    }
                    else
                    {
                        path[i] = parent2[i];
                        unvisitedCities.Remove(parent2[i]);
                    }
                }
                else if (unvisitedCities.Contains(parent1[i]))
                {
                    path[i] = parent1[i];
                    unvisitedCities.Remove(parent1[i]);
                }
                else if (unvisitedCities.Contains(parent2[i]))
                {
                    path[i] = parent2[i];
                    unvisitedCities.Remove(parent2[i]);
                }
                else
                {
                    int rand = UnityEngine.Random.Range(0, unvisitedCities.Count);
                    path[i] = unvisitedCities[rand];
                    unvisitedCities.RemoveAt(rand);
                }
            }

            path[path.Length - 1] = 0;
        }
        else if (type == 2)
        {
            List<int> unvisitedCities = new List<int>();
            for (int i = 0; i < path.Length - 1; i++)
                unvisitedCities.Add(i + 1);

            for (int i = 0; i < path.Length - 1; i++)
                path[i] = 0;


            for (int i = 0; i < path.Length - 1; i++)
            {
                if (unvisitedCities.Contains(parent1[i]) && unvisitedCities.Contains(parent2[i]))
                {
                    if ((float)i%2 != 0)
                    {
                        path[i] = parent1[i];
                        unvisitedCities.Remove(parent1[i]);
                    }
                    else
                    {
                        path[i] = parent2[i];
                        unvisitedCities.Remove(parent2[i]);
                    }
                }
                else if (unvisitedCities.Contains(parent1[i]))
                {
                    path[i] = parent1[i];
                    unvisitedCities.Remove(parent1[i]);
                }
                else if (unvisitedCities.Contains(parent2[i]))
                {
                    path[i] = parent2[i];
                    unvisitedCities.Remove(parent2[i]);
                }
                else
                {
                    int rand = UnityEngine.Random.Range(0, unvisitedCities.Count);
                    path[i] = unvisitedCities[rand];
                    unvisitedCities.RemoveAt(rand);
                }
            }

            path[path.Length - 1] = 0;
        }
        else
            Debug.LogError("Incorrect type");

        return path;
    }

    int[] MutatePath(int[] path, int mutationsCount)
    {
        List<int> mutatedIdexes = path.ToList();
        List<int> unmutatedIdexes = path.ToList();
        for (int i = 0; i < path.Length-1; i++)
        {
            unmutatedIdexes.Add(i);
        }

        for (int i = mutationsCount; i > 0;)
        {

            if (i > 1)
            {
                int i1 = unmutatedIdexes[UnityEngine.Random.Range(0, unmutatedIdexes.Count)];
                unmutatedIdexes.Remove(i1);
                int i2 = unmutatedIdexes[UnityEngine.Random.Range(0, unmutatedIdexes.Count)];
                unmutatedIdexes.Remove(i2);
                mutatedIdexes.Add(i1);
                mutatedIdexes.Add(i2);
                int a1 = path[i1];
                int a2 = path[i2];
                path[i1] = a1;
                path[i2] = a2;
                i -= 2;
            }
            else if (i == 1)
            {
                int i1 = unmutatedIdexes[UnityEngine.Random.Range(0, unmutatedIdexes.Count)];
                unmutatedIdexes.Remove(i1);
                int i2 = mutatedIdexes[UnityEngine.Random.Range(0, mutatedIdexes.Count)];
                mutatedIdexes.Add(i1);
                int a1 = path[i1];
                int a2 = path[i2];
                path[i1] = a1;
                path[i2] = a2;
                i -= 1;
            }
        }

        return path;
    }


    //Генетический алгаритм
    int iterationsCount = 100000;
    int currentIteration = 0;
    int childsCount = 15;

    string pnathJSON => Path.Combine(UnityEngine.Application.persistentDataPath, "data.json");

    int[] bestPath;
    float bestPathDist = 9999;

    int[] bestPath1;
    float bestPath1Dist = 9999;

    int[] bestPath2;
    float bestPath2Dist = 9999;

    void GenAlg()
    {
        float startTime = GetTime();

        int[][] paths = new int[childsCount][];
        for (int i = 0; i < childsCount; i++)
        {
            paths[i] = CreatetPath(2);
        }


        for (int currentIteration = 1; currentIteration <= iterationsCount; currentIteration++)
        {
            for (int i = 0; i < childsCount; i++)
            {
                float dist = 0;
                int currentCity = 0;
                for (int j = 0; j < cities.Count; j++)
                {
                    dist += Vector2.Distance(cities[currentCity].transform.position, cities[paths[i][j]].transform.position);
                    currentCity = paths[i][j];
                }

                if (dist< bestPath1Dist)
                {
                    bestPath1 = paths[i];
                    bestPath1Dist = dist;   
                }
                else if (dist < bestPath2Dist)
                {
                    bestPath2 = paths[i];
                    bestPath2Dist = dist;
                }

                if (dist < bestPathDist)
                {
                    bestPath = paths[i];
                    bestPathDist = dist;
                }
            }


            for (int i = 0; i < childsCount; i++)
            {
                int[] newChild = MergePath(2, bestPath1, bestPath2);
                newChild = MutatePath(newChild, 2);
                paths[i] = newChild;
            }
        }

        Debug.Log("Время расчёта: " + (GetTime() - startTime) + " секунд");
        Debug.Log("Минимальное растояние:  " + bestPathDist);
    }



    float GetTime()
    {
        return DateTime.Now.Second + DateTime.Now.Minute * 60f + DateTime.Now.Millisecond * 0.001f;
    }


    //Расчёт
    static (List<int>, float) SolveTSP(Vector3[] cities)
    {
        int n = cities.Length;
        List<int> indices = Enumerable.Range(0, n).ToList();
        float minDistance = float.MaxValue;
        List<int> bestPath = null;

        // Перебор всех перестановок
        foreach (var permutation in GetPermutations(indices, n))
        {
            float currentDistance = CalculateRouteDistance(cities, permutation);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                bestPath = new List<int>(permutation);
            }
        }

        return (bestPath, minDistance);
    }

    static IEnumerable<List<int>> GetPermutations(List<int> list, int length)
    {
        if (length == 1) yield return list;
        else
        {
            for (int i = 0; i < length; i++)
            {
                foreach (var perm in GetPermutations(list, length - 1))
                {
                    yield return perm;
                }
                Swap(list, length - 1, i);
            }
        }
    }

    static void Swap(List<int> list, int i, int j)
    {
        int temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    static float CalculateRouteDistance(Vector3[] cities, List<int> path)
    {
        float totalDistance = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(cities[path[i]], cities[path[i + 1]]);
        }

        // Добавляем расстояние для возврата в начальную точку
        totalDistance += Vector3.Distance(cities[path[path.Count - 1]], cities[path[0]]);

        return totalDistance;
    }
}
