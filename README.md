<h1 align="center">TravelingNurse</h1>

<p align="center">
  <img alt="Github top language" src="https://img.shields.io/github/languages/top/MrHencke/TravelingNurse?color=56BEB8">

  <img alt="Github language count" src="https://img.shields.io/github/languages/count/MrHencke/TravelingNurse?color=56BEB8">

  <img alt="Repository size" src="https://img.shields.io/github/repo-size/MrHencke/TravelingNurse?color=56BEB8">

  <img alt="License" src="https://img.shields.io/github/license/MrHencke/TravelingNurse?color=56BEB8">
</p>

<p align="center">
  <a href="#about">About</a> &#xa0; | &#xa0; 
  <a href="#features">Features</a> &#xa0; | &#xa0;
  <a href="#starting">Starting</a> &#xa0; | &#xa0;
  <a href="#license">License</a> &#xa0; | &#xa0;
  <a href="https://github.com/MrHencke" target="_blank">Author</a>
</p>

<br>

## About ##

TravelingNurse is a C# application that leverages a genetic algorithm to solve complex scheduling and routing problems, inspired by the Traveling Salesman Problem but adapted for healthcare scenarios. The project models nurses, patients, and depots, aiming to optimize routes and assignments for traveling nurses visiting multiple patients efficiently.

The program was developed as an obligatory assignement in IT3708 Bio-Inspired Artificial Intelligence at NTNU during the spring of 2023.

## Features ##

- **Instance Loader:** Flexible loading of problem instances from JSON files, allowing for easy experimentation with different scenarios.
- **Genotype Representation:** Individuals encode nurse routes and assignments, supporting both permutation and assignment-based genotypes.
- **Fitness Functions:** Multiple fitness evaluation strategies, including total distance, time windows, and workload balancing, to guide the evolution toward practical solutions.
- **Parent Selection Strategies:** Implements several parent selection methods such as tournament selection, roulette wheel selection, and rank-based selection, enabling exploration of different evolutionary pressures.
- **Crossover Operators:** Supports various crossover techniques (e.g., Order Crossover, Partially Mapped Crossover, Uniform Crossover) to combine parent solutions and maintain genetic diversity.
- **Mutation Operators:** Includes multiple mutation strategies like swap, inversion, and scramble mutations to introduce variability and prevent premature convergence.
- **Survivor Selection:** Offers configurable survivor selection mechanisms, including elitism and generational replacement, to control population dynamics.
- **Extensible Design:** Modular architecture allows for easy addition of new strategies and operators, making the algorithm highly customizable for research and practical use.

The algorithm is designed for experimentation, enabling users to tweak parameters and strategies to study their impact on solution quality and convergence speed. The project also provides utilities for visualizing and analyzing results, making it a valuable tool for both learning and applied optimization in healthcare logistics.

## :checkered_flag: Starting ##

```bash
# Clone this project
$ git clone https://github.com/MrHencke/TravelingNurse

# Navigate to project folder
$ cd TravelingNurse

# Install dependencies
$ dotnet restore --project TravelingNurse\TravelingNurse.csproj

# Run the project
$ dotnet run --project TravelingNurse\TravelingNurse.csproj


# Alternatively open the solution and run from an IDE
$ TravelingNurse.sln
```

## License ##

This project is under license from MIT. For more details, see the [LICENSE](LICENSE.md) file.

Made by <a href="https://github.com/MrHencke" target="_blank">Henrik</a>

&#xa0;

<a href="#top">Back to top</a>
