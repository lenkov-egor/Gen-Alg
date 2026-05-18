using com.cyborgAssets.inspectorButtonPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Manager2 : MonoBehaviour
{
    public GameObject cityPref;
    public List<GameObject> cities = new List<GameObject>();


    public int populationSize = 100; // Размер популяции
    public int generations = 750; // Количество поколений
    public float mutationRate = 0.1f; // Шанс мутации

    private List<List<int>> population; // Популяция маршрутов


    float interractDist = 0.25f;
    GameObject city = null;

    Text logs;
    GameObject loadingScreen;


    private void Start()
    {
        logs = transform.Find("Canvas").Find("Menu").Find("LogsText").GetComponent<Text>();
        loadingScreen = transform.Find("Canvas").Find("LoadingScreen").gameObject;
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonUp(1))
            city = null;

        if (Input.GetMouseButtonDown(1))
        {
            city = null;
            for (int i = 0; i < cities.Count; i++)
            {
                float distToCity = Vector2.Distance(mousePos, cities[i].transform.position);
                if (distToCity <= interractDist && (city == null ||
                    distToCity < Vector2.Distance(mousePos, city.transform.position)))
                    city = cities[i];
            }
        }

        if (city != null)
            city.transform.position = mousePos;
    }

    public void Gens()
    {
        StartCoroutine(GeneticAlgorithm());
    }

    public void Math()
    {
        StartCoroutine(SolveTSP());
    }



    string StringPath(int[] path)
    {
        string st = "";
        foreach (int ch in path)
            st += ch + " ";
        return st;
    }


    [ProButton]
    public void CreateCity()
    {
        GameObject newCity = Instantiate(cityPref, Vector3.zero, Quaternion.identity);
        newCity.transform.Find("Text").GetComponent<TextMeshPro>().text = "" + cities.Count;
        newCity.name = "City " + cities.Count;

        cities.Add(newCity);
    }


    [ProButton]
    public void DeleteCity()
    {
        if (cities.Count != 0)
        {
            DestroyImmediate(cities[cities.Count - 1]);
            cities.RemoveAt(cities.Count - 1);
        }
    }


    void DrawPath(List<int> path, int type)
    {
        LineRenderer lineRenderer = null;
        if (type == 1)
        {
            lineRenderer = transform.Find("MathLR").GetComponent<LineRenderer>();
            transform.Find("GenAlgLR").gameObject.SetActive(false);
            transform.Find("MathLR").gameObject.SetActive(true);
        }
        else if (type == 2)
        {
            lineRenderer = transform.Find("GenAlgLR").GetComponent<LineRenderer>();
            transform.Find("MathLR").gameObject.SetActive(false);
            transform.Find("GenAlgLR").gameObject.SetActive(true);
        }
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, cities[path[i]].transform.position);
        }
    }

    float GetTime()
    {
        return DateTime.Now.Second + DateTime.Now.Minute * 60f + DateTime.Now.Millisecond * 0.001f;
    }





    // 1. Инициализация популяции

    void InitializePopulation()
    {
        population = new List<List<int>> ();

        for (int i = 0; i < populationSize; i++)
        {
            List<int> path = GenerateRandomPath();
            population.Add(path);
        }
    }

    List<int> GenerateRandomPath()
    {
        List<int> path = new List<int>();
        path.Add(0);

        List<int> unvisitedCities = new List<int>();
        for (int i = 1; i < cities.Count; i++)
            unvisitedCities.Add(i);

        while (unvisitedCities.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, unvisitedCities.Count);
            path.Add(unvisitedCities[r]);
            unvisitedCities.RemoveAt(r);
        }

        path.Add(0);
        return path;
    }

    // 3. Селекция: турнирный отбор

    List<int> SelectParent()
    {
        int tournamentSize = 5; // Размер турнира
        List<int> best = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, population.Count);
            List<int> candidate = population[randomIndex];
            float dist = CalculatePathDistance(candidate);

            if (dist < minDist)
            {
                best = candidate;
                minDist = dist;
            }
        }

        return best;
    }

    // 4. Кроссовер: упорядоченное скрещивание
    List<int> Crossover(List<int> parent1, List<int> parent2)
    {
        List<int> par1 = new List<int>(parent1);
        List<int> par2 = new List<int>(parent2);

        par1.RemoveAt(0);
        par1.RemoveAt(par1.Count-1);
        par2.RemoveAt(0);
        par2.RemoveAt(par2.Count - 1);

        List<int> child = new List<int>();
        HashSet<int> addedCities = new HashSet<int>();

        child.Add(0);

        int start = UnityEngine.Random.Range(0, par1.Count / 2);
        int end = start + par1.Count / 2;

        // Копируем часть маршрута от первого родителя
        for (int i = start; i < end; i++)
        {
            child.Add(par1[i]);
            addedCities.Add(par1[i]);
        }

        // Заполняем оставшиеся города от второго родителя
        int index = 0;
        for (int i = 0; i < par2.Count; i++)
        {
            if (!addedCities.Contains(par2[i]))
            {
                while (index >= start && index < end)
                {
                    index++;
                }

                child.Add(par2[i]);
                index++;
            }
        }

        child.Add(0);
        return child;
    }

    // 5. Мутация
    void Mutate(List<int> path)
    {
        if (UnityEngine.Random.value < mutationRate)
        {
            int indexA = UnityEngine.Random.Range(1, path.Count-1);
            int indexB = UnityEngine.Random.Range(1, path.Count-1);

            // Меняем местами два города
            int temp = path[indexA];
            path[indexA] = path[indexB];
            path[indexB] = temp;
        }
    }

    // 6. Основной цикл алгоритма
    IEnumerator GeneticAlgorithm()
    {
        loadingScreen.SetActive(true);
        yield return null;

        InitializePopulation();
        float startTime = GetTime();

        for (int generation = 0; generation < generations; generation++)
        {
            List<List<int>> newPopulation = new List<List<int>>();

            for (int i = 0; i < populationSize; i++)
            {
                List<int> parent1 = SelectParent();
                List<int> parent2 = SelectParent();
                List<int> child = Crossover(parent1, parent2);
                Mutate(child);
                newPopulation.Add(child);
            }
            population = newPopulation;
        }

        float endTime = GetTime();
        List<int> bestPath = GetBestPath();
        DrawPath(bestPath, 2);

        loadingScreen.SetActive(false);
        logs.text =
            "Генетический алгоритм" +
            "\nВремя расчёта: " + (endTime - startTime) + " секунд" +
            "\nМинимальное растояние: " + CalculatePathDistance(bestPath) +
            "\nПуть: " + StringPath(bestPath.ToArray());
    }

    List<int> GetBestPath()
    {
        List<int> bestPath = null;
        float minDist = float.MaxValue;

        foreach (List<int> path in population)
        {
            float dist = CalculatePathDistance(path);
            if (dist < minDist)
            {
                bestPath = path;
                minDist = dist;
            }
        }

        return bestPath;
    }


    IEnumerator SolveTSP()
    {
        loadingScreen.SetActive(true);
        yield return null;

        float startTime = GetTime();

        int n = cities.Count;
        List<int> indices = Enumerable.Range(1, n-1).ToList();
        float minDistance = float.MaxValue;
        List<int> bestPath = null;

        // Перебор всех перестановок
        foreach (var permutation in GetPermutations(indices))
        {
            List<int> path = new List<int>();
            path.Add(0);
            path.AddRange(permutation);
            path.Add(0);

            float currentDistance = CalculatePathDistance(path);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                bestPath = path;
            }
        }

        loadingScreen.SetActive(false);
        DrawPath(bestPath, 1);
        float endTime = GetTime();
        logs.text = 
            "Перебор всех значений" +
            "\nВремя расчёта: " + (endTime - startTime) + " секунд" +
            "\nМинимальное растояние: " + minDistance +
            "\nПуть: " + StringPath(bestPath.ToArray());
    }


    IEnumerable<List<int>> GetPermutations(List<int> list)
    {
        if (list.Count == 0)
        {
            yield return new List<int>();
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                int current = list[i];
                List<int> remaining = new List<int>();
                for (int j = 0; j < list.Count; j++)
                {
                    if (i != j)
                        remaining.Add(list[j]);
                }

                foreach (var permutation in GetPermutations(remaining))
                {
                    var result = new List<int> { current };
                    result.AddRange(permutation);
                    yield return result;
                }

            }
        }
    }


    //Длинна пути
    float CalculatePathDistance(List<int> path)
    {
        float totalDistance = 0;
        for (int i = 0; i < path.Count - 1; i++)
            totalDistance += Vector3.Distance(cities[path[i]].transform.position,
                cities[path[i+1]].transform.position);    
        return totalDistance;
    }
}
