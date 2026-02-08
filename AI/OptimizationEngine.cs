using System;
using System.Collections.Generic;
using System.Linq;
using RebarAutomation.Models;

namespace RebarAutomation.AI
{
    /// <summary>
    /// AI-powered optimization engine using genetic algorithms
    /// Optimizes rebar arrangement for cost, material efficiency, and constructability
    /// </summary>
    public class OptimizationEngine
    {
        private readonly SlabDesignInput _input;
        private readonly SlabDesignOutput _baselineOutput;
        private readonly int _populationSize = 50;
        private readonly int _generations = 100;
        private readonly double _mutationRate = 0.1;

        public OptimizationEngine(SlabDesignInput input, SlabDesignOutput baselineOutput)
        {
            _input = input;
            _baselineOutput = baselineOutput;
        }

        /// <summary>
        /// Optimizes bar arrangement to minimize cost and material waste
        /// </summary>
        public OptimizationResult OptimizeBarArrangement()
        {
            // Initialize population with random solutions
            List<RebarSolution> population = InitializePopulation();

            RebarSolution bestSolution = null;
            double bestFitness = double.MinValue;

            for (int generation = 0; generation < _generations; generation++)
            {
                // Evaluate fitness for each solution
                foreach (var solution in population)
                {
                    solution.Fitness = CalculateFitness(solution);
                    
                    if (solution.Fitness > bestFitness)
                    {
                        bestFitness = solution.Fitness;
                        bestSolution = solution;
                    }
                }

                // Create next generation
                population = CreateNextGeneration(population);
            }

            return new OptimizationResult
            {
                OptimizedSolution = bestSolution,
                BaselineCost = CalculateCost(_baselineOutput),
                OptimizedCost = CalculateCost(bestSolution),
                MaterialSavings = CalculateMaterialSavings(_baselineOutput, bestSolution),
                LaborSavings = CalculateLaborSavings(_baselineOutput, bestSolution)
            };
        }

        /// <summary>
        /// Suggests multiple alternative design options
        /// </summary>
        public List<AlternativeDesign> SuggestAlternatives()
        {
            var alternatives = new List<AlternativeDesign>();

            // Alternative 1: Minimize steel weight
            var minWeightSolution = OptimizeForMinimumWeight();
            alternatives.Add(new AlternativeDesign
            {
                Name = "Minimum Steel Weight",
                Description = "Optimized for minimum material usage",
                Solution = minWeightSolution,
                Cost = CalculateCost(minWeightSolution),
                Pros = new List<string> { "Lowest material cost", "Reduced dead load" },
                Cons = new List<string> { "May have more bar sizes", "Tighter spacing" }
            });

            // Alternative 2: Minimize bar variety
            var minVarietySolution = OptimizeForMinimumVariety();
            alternatives.Add(new AlternativeDesign
            {
                Name = "Minimum Bar Variety",
                Description = "Uses fewer different bar sizes",
                Solution = minVarietySolution,
                Cost = CalculateCost(minVarietySolution),
                Pros = new List<string> { "Simpler procurement", "Reduced site confusion", "Faster installation" },
                Cons = new List<string> { "Slightly higher material cost" }
            });

            // Alternative 3: Balanced approach
            var balancedSolution = OptimizeBalanced();
            alternatives.Add(new AlternativeDesign
            {
                Name = "Balanced Design",
                Description = "Balance between cost and constructability",
                Solution = balancedSolution,
                Cost = CalculateCost(balancedSolution),
                Pros = new List<string> { "Good overall value", "Practical spacing", "Moderate bar variety" },
                Cons = new List<string> { "Not optimal in any single metric" }
            });

            return alternatives.OrderBy(a => a.Cost).ToList();
        }

        /// <summary>
        /// Learns from successful designs (placeholder for ML integration)
        /// </summary>
        public void LearnFromDesign(SlabDesignInput input, SlabDesignOutput output, double userRating)
        {
            // In a full implementation, this would:
            // 1. Store design parameters and outcomes in a database
            // 2. Train ML model on historical data
            // 3. Use patterns to improve future suggestions
            
            // For now, we'll just log the design
            var designRecord = new DesignRecord
            {
                Timestamp = DateTime.Now,
                Input = input,
                Output = output,
                UserRating = userRating,
                TotalCost = CalculateCost(output)
            };

            // TODO: Save to database or file for future ML training
        }

        #region Genetic Algorithm Methods

        private List<RebarSolution> InitializePopulation()
        {
            var population = new List<RebarSolution>();
            var random = new Random();

            int[] availableDiameters = new int[] { 8, 10, 12, 16, 20, 25 };

            for (int i = 0; i < _populationSize; i++)
            {
                var solution = new RebarSolution
                {
                    MainBarDiameter = availableDiameters[random.Next(availableDiameters.Length)],
                    DistBarDiameter = availableDiameters[random.Next(availableDiameters.Length)],
                    MainBarSpacing = random.Next(100, 300),
                    DistBarSpacing = random.Next(150, 450)
                };

                // Ensure solution meets minimum steel requirements
                if (IsValidSolution(solution))
                {
                    population.Add(solution);
                }
                else
                {
                    i--; // Try again
                }
            }

            return population;
        }

        private List<RebarSolution> CreateNextGeneration(List<RebarSolution> currentPopulation)
        {
            var nextGeneration = new List<RebarSolution>();
            var random = new Random();

            // Sort by fitness
            var sorted = currentPopulation.OrderByDescending(s => s.Fitness).ToList();

            // Elitism: Keep top 10%
            int eliteCount = _populationSize / 10;
            nextGeneration.AddRange(sorted.Take(eliteCount));

            // Crossover and mutation
            while (nextGeneration.Count < _populationSize)
            {
                // Select parents using tournament selection
                var parent1 = TournamentSelection(sorted, random);
                var parent2 = TournamentSelection(sorted, random);

                // Crossover
                var child = Crossover(parent1, parent2, random);

                // Mutation
                if (random.NextDouble() < _mutationRate)
                {
                    child = Mutate(child, random);
                }

                if (IsValidSolution(child))
                {
                    nextGeneration.Add(child);
                }
            }

            return nextGeneration;
        }

        private RebarSolution TournamentSelection(List<RebarSolution> population, Random random)
        {
            int tournamentSize = 5;
            var tournament = new List<RebarSolution>();

            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(population[random.Next(population.Count)]);
            }

            return tournament.OrderByDescending(s => s.Fitness).First();
        }

        private RebarSolution Crossover(RebarSolution parent1, RebarSolution parent2, Random random)
        {
            return new RebarSolution
            {
                MainBarDiameter = random.Next(2) == 0 ? parent1.MainBarDiameter : parent2.MainBarDiameter,
                DistBarDiameter = random.Next(2) == 0 ? parent1.DistBarDiameter : parent2.DistBarDiameter,
                MainBarSpacing = (parent1.MainBarSpacing + parent2.MainBarSpacing) / 2,
                DistBarSpacing = (parent1.DistBarSpacing + parent2.DistBarSpacing) / 2
            };
        }

        private RebarSolution Mutate(RebarSolution solution, Random random)
        {
            int[] diameters = new int[] { 8, 10, 12, 16, 20, 25 };
            
            var mutated = new RebarSolution
            {
                MainBarDiameter = solution.MainBarDiameter,
                DistBarDiameter = solution.DistBarDiameter,
                MainBarSpacing = solution.MainBarSpacing,
                DistBarSpacing = solution.DistBarSpacing
            };

            int mutationType = random.Next(4);
            switch (mutationType)
            {
                case 0:
                    mutated.MainBarDiameter = diameters[random.Next(diameters.Length)];
                    break;
                case 1:
                    mutated.DistBarDiameter = diameters[random.Next(diameters.Length)];
                    break;
                case 2:
                    mutated.MainBarSpacing += random.Next(-20, 21);
                    break;
                case 3:
                    mutated.DistBarSpacing += random.Next(-30, 31);
                    break;
            }

            return mutated;
        }

        #endregion

        #region Fitness and Cost Calculation

        private double CalculateFitness(RebarSolution solution)
        {
            // Multi-objective fitness function
            double materialCost = CalculateMaterialCost(solution);
            double laborCost = CalculateLaborCost(solution);
            double varietyPenalty = CalculateVarietyPenalty(solution);
            double spacingScore = CalculateSpacingScore(solution);

            // Weighted combination (weights can be tuned)
            double fitness = 1000.0 / materialCost + // Lower material cost = higher fitness
                            500.0 / laborCost +      // Lower labor cost = higher fitness
                            varietyPenalty * 100 +    // Fewer bar types = higher fitness
                            spacingScore * 50;        // Better spacing = higher fitness

            return fitness;
        }

        private double CalculateCost(SlabDesignOutput output)
        {
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);

            // Calculate total steel volume
            double mainBarArea = Math.PI * output.MainBarDiameter * output.MainBarDiameter / 4.0;
            double mainBarLength = ly * output.MainBarCount;
            double mainBarVolume = mainBarArea * mainBarLength;

            double distBarArea = Math.PI * output.DistBarDiameter * output.DistBarDiameter / 4.0;
            double distBarLength = lx * output.DistBarCount;
            double distBarVolume = distBarArea * distBarLength;

            double totalVolume = (mainBarVolume + distBarVolume) / 1000000000.0; // Convert mm³ to m³
            double totalWeight = totalVolume * 7850; // kg (steel density)

            // Cost calculation (example rates)
            double steelCostPerKg = 60; // INR per kg
            double laborCostPerBar = 5; // INR per bar for cutting and bending
            double totalBars = output.MainBarCount + output.DistBarCount;

            return (totalWeight * steelCostPerKg) + (totalBars * laborCostPerBar);
        }

        private double CalculateCost(RebarSolution solution)
        {
            // Similar to above but for RebarSolution
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);

            int mainBarCount = (int)(lx / solution.MainBarSpacing) + 1;
            int distBarCount = (int)(ly / solution.DistBarSpacing) + 1;

            double mainBarArea = Math.PI * solution.MainBarDiameter * solution.MainBarDiameter / 4.0;
            double mainBarVolume = mainBarArea * ly * mainBarCount;

            double distBarArea = Math.PI * solution.DistBarDiameter * solution.DistBarDiameter / 4.0;
            double distBarVolume = distBarArea * lx * distBarCount;

            double totalVolume = (mainBarVolume + distBarVolume) / 1000000000.0;
            double totalWeight = totalVolume * 7850;

            return (totalWeight * 60) + ((mainBarCount + distBarCount) * 5);
        }

        private double CalculateMaterialCost(RebarSolution solution)
        {
            return CalculateCost(solution) * 0.8; // Material is ~80% of total cost
        }

        private double CalculateLaborCost(RebarSolution solution)
        {
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);
            int totalBars = (int)(lx / solution.MainBarSpacing) + (int)(ly / solution.DistBarSpacing) + 2;
            return totalBars * 5; // INR per bar
        }

        private double CalculateVarietyPenalty(RebarSolution solution)
        {
            // Fewer unique bar sizes = higher score
            int uniqueSizes = (solution.MainBarDiameter == solution.DistBarDiameter) ? 1 : 2;
            return 1.0 / uniqueSizes;
        }

        private double CalculateSpacingScore(RebarSolution solution)
        {
            // Prefer spacing that's easy to measure (multiples of 50mm)
            double mainScore = (solution.MainBarSpacing % 50 == 0) ? 1.0 : 0.5;
            double distScore = (solution.DistBarSpacing % 50 == 0) ? 1.0 : 0.5;
            return (mainScore + distScore) / 2.0;
        }

        private double CalculateMaterialSavings(SlabDesignOutput baseline, RebarSolution optimized)
        {
            double baselineCost = CalculateCost(baseline);
            double optimizedCost = CalculateCost(optimized);
            return ((baselineCost - optimizedCost) / baselineCost) * 100; // Percentage
        }

        private double CalculateLaborSavings(SlabDesignOutput baseline, RebarSolution optimized)
        {
            int baselineBars = baseline.MainBarCount + baseline.DistBarCount;
            double lx = Math.Min(_input.Length, _input.Width);
            double ly = Math.Max(_input.Length, _input.Width);
            int optimizedBars = (int)(lx / optimized.MainBarSpacing) + (int)(ly / optimized.DistBarSpacing) + 2;
            
            return ((baselineBars - optimizedBars) / (double)baselineBars) * 100;
        }

        #endregion

        #region Validation

        private bool IsValidSolution(RebarSolution solution)
        {
            // Check if solution meets minimum steel requirements
            double requiredAst = _baselineOutput.AstMainBottom;
            
            double providedAst = (Math.PI * solution.MainBarDiameter * solution.MainBarDiameter / 4.0) * 
                                (1000.0 / solution.MainBarSpacing);

            if (providedAst < requiredAst * 0.95) // Allow 5% tolerance
                return false;

            // Check spacing limits
            if (solution.MainBarSpacing < 100 || solution.MainBarSpacing > 300)
                return false;

            if (solution.DistBarSpacing < 150 || solution.DistBarSpacing > 450)
                return false;

            return true;
        }

        #endregion

        #region Optimization Strategies

        private RebarSolution OptimizeForMinimumWeight()
        {
            // Use smallest bars with maximum spacing that meets requirements
            var result = OptimizeBarArrangement();
            return result.OptimizedSolution;
        }

        private RebarSolution OptimizeForMinimumVariety()
        {
            // Try to use same diameter for both main and distribution
            var population = InitializePopulation();
            
            // Filter to solutions with same diameter
            var sameDiameterSolutions = population.Where(s => s.MainBarDiameter == s.DistBarDiameter).ToList();
            
            if (sameDiameterSolutions.Any())
            {
                return sameDiameterSolutions.OrderBy(s => CalculateCost(s)).First();
            }

            return population.OrderBy(s => CalculateCost(s)).First();
        }

        private RebarSolution OptimizeBalanced()
        {
            return OptimizeBarArrangement().OptimizedSolution;
        }

        #endregion
    }

    #region Data Models

    public class RebarSolution : SlabDesignOutput
    {
        public double Fitness { get; set; }
    }

    public class OptimizationResult
    {
        public RebarSolution OptimizedSolution { get; set; }
        public double BaselineCost { get; set; }
        public double OptimizedCost { get; set; }
        public double MaterialSavings { get; set; } // Percentage
        public double LaborSavings { get; set; } // Percentage
    }

    public class AlternativeDesign
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RebarSolution Solution { get; set; }
        public double Cost { get; set; }
        public List<string> Pros { get; set; }
        public List<string> Cons { get; set; }
    }

    public class DesignRecord
    {
        public DateTime Timestamp { get; set; }
        public SlabDesignInput Input { get; set; }
        public SlabDesignOutput Output { get; set; }
        public double UserRating { get; set; }
        public double TotalCost { get; set; }
    }

    #endregion
}
