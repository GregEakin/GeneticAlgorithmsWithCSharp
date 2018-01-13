using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneticAlgorithms.LawnmowerProblem
{
    [TestClass]
    public class LawnmowerTests
    {
        [TestMethod]
        public void MowTurnTest()
        {
            var width = 8;
            var height = 8;
            var geneSet = new INode[] { };
            var minGenes = width * height;
            var maxGenes = (int) (1.5 * minGenes);
            var maxMutationRounds = 3;
            var expectedNumberOfInstructions = 78;

            //       def fnCreateField():
            // return lawnmower.ToroidField(width, height,
            //     lawnmower.FieldContents.Grass)

//            RunWith(geneSet, width, height, minGenes, maxGenes,
//                expectedNumberOfInstructions, maxMutationRounds,
//                fnCreateField, expectedNumberOfInstructions);
        }

        private void RunWith(INode[] geneSet, int width, int height, int minGenes, int maxGenes,
            int expectedNumberOfInstructions, int maxMutationRounds, object o, int expectedNumberOfSteps)
        {
//            var lawnmower = new Mower(new Location(0,0), new Direction(0,0,0,' '), 0);
//            var mowerStartLocation = lawnmower.Location(width / 2, height / 2);
//            var mowerStartDirection = lawnmower.Directions.South.value;

            var genetic = new Genetic<int, int>();
            var watch = new Stopwatch();
//            var optimalFitness = Fitness(width * height, expectedNumberOfInstructions, expectedNumberOfSteps);
//            var best = genetic.BestFitness();
//            Assert.IsTrue(best.Fitness.CompareTo(optimalFitness) <= 0);
        }
    }
}