using System.Collections.Generic;
using UnityEngine;

public class Gens : MonoBehaviour
{
    public List<GameObject> citiesList = new List<GameObject>();

    Vector2[] cities; // Города в виде массива Vector2
    public int populationSize = 100; // Размер популяции
    public int generations = 500; // Количество поколений
    public float mutationRate = 0.1f; // Шанс мутации
    private List<int[]> population; // Популяция маршрутов
    private System.Random random = new System.Random(); // Для случайных значений

    void Start()
    {
        cities = new Vector2[citiesList.Count];
        for (int i = 0; i < citiesList.Count; i++)
            cities[i] = citiesList[i].transform.position;

        InitializePopulation();
        RunGeneticAlgorithm();
    }

    // 1. Инициализация популяции
    void InitializePopulation()
    {
        population = new List<int[]>();

        for (int i = 0; i < populationSize; i++)
        {
            int[] route = GenerateRandomRoute();
            population.Add(route);
        }
    }

    int[] GenerateRandomRoute()
    {
        int[] route = new int[cities.Length];
        for (int i = 0; i < cities.Length; i++)
        {
            route[i] = i;
        }

        // Перемешиваем маршрут случайным образом
        for (int i = 0; i < route.Length; i++)
        {
            int randomIndex = random.Next(i, route.Length);
            int temp = route[i];
            route[i] = route[randomIndex];
            route[randomIndex] = temp;
        }

        return route;
    }

    // 2. Фитнес-функция: обратное расстояние
    float CalculateRouteDistance(int[] route)
    {
        float distance = 0f;

        for (int i = 0; i < route.Length - 1; i++)
        {
            Vector2 cityA = cities[route[i]];
            Vector2 cityB = cities[route[i + 1]];
            distance += Vector2.Distance(cityA, cityB);
        }

        // Замыкаем маршрут
        distance += Vector2.Distance(cities[route[route.Length - 1]], cities[route[0]]);
        return distance;
    }

    float EvaluateFitness(int[] route)
    {
        return 1f / CalculateRouteDistance(route); // Чем короче маршрут, тем выше фитнес
    }

    // 3. Селекция: турнирный отбор
    int[] SelectParent()
    {
        int tournamentSize = 5; // Размер турнира
        int[] best = null;
        float bestFitness = 0;

        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = random.Next(population.Count);
            int[] candidate = population[randomIndex];
            float fitness = EvaluateFitness(candidate);

            if (fitness > bestFitness)
            {
                best = candidate;
                bestFitness = fitness;
            }
        }

        return best;
    }

    // 4. Кроссовер: упорядоченное скрещивание
    int[] Crossover(int[] parent1, int[] parent2)
    {
        int[] child = new int[parent1.Length];
        HashSet<int> addedCities = new HashSet<int>();

        int start = random.Next(0, parent1.Length / 2);
        int end = start + parent1.Length / 2;

        // Копируем часть маршрута от первого родителя
        for (int i = start; i < end; i++)
        {
            child[i] = parent1[i];
            addedCities.Add(parent1[i]);
        }

        // Заполняем оставшиеся города от второго родителя
        int index = 0;
        for (int i = 0; i < parent2.Length; i++)
        {
            if (!addedCities.Contains(parent2[i]))
            {
                while (index >= start && index < end)
                {
                    index++;
                }

                child[index++] = parent2[i];
            }
        }

        return child;
    }

    // 5. Мутация
    void Mutate(int[] route)
    {
        if (random.NextDouble() < mutationRate)
        {
            int indexA = random.Next(0, route.Length);
            int indexB = random.Next(0, route.Length);

            // Меняем местами два города
            int temp = route[indexA];
            route[indexA] = route[indexB];
            route[indexB] = temp;
        }
    }

    // 6. Основной цикл алгоритма
    void RunGeneticAlgorithm()
    {
        for (int generation = 0; generation < generations; generation++)
        {
            List<int[]> newPopulation = new List<int[]>();

            for (int i = 0; i < populationSize; i++)
            {
                int[] parent1 = SelectParent();
                int[] parent2 = SelectParent();
                int[] child = Crossover(parent1, parent2);
                Mutate(child);
                newPopulation.Add(child);
            }

            population = newPopulation;

            // Находим лучший маршрут текущего поколения
            int[] bestRoute = GetBestRoute();
            Debug.Log($"Generation {generation}: Distance = {CalculateRouteDistance(bestRoute)}");
            DrawRoute(bestRoute);
        }
    }

    int[] GetBestRoute()
    {
        int[] bestRoute = null;
        float bestFitness = 0;

        foreach (int[] route in population)
        {
            float fitness = EvaluateFitness(route);
            if (fitness > bestFitness)
            {
                bestRoute = route;
                bestFitness = fitness;
            }
        }

        return bestRoute;
    }

    // Отображение маршрута (необязательно)
    void DrawRoute(int[] route)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = route.Length + 1;

        for (int i = 0; i < route.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(cities[route[i]].x, cities[route[i]].y, 0));
        }

        // Возвращаемся в начальный город
        lineRenderer.SetPosition(route.Length, new Vector3(cities[route[0]].x, cities[route[0]].y, 0));
    }
}
