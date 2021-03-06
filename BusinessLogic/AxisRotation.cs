using System;
using System.Collections.Generic;
using TspAxesRot.Domain;
using TspAxesRot.Interfaces;

namespace TspAxesRot.BusinessLogic
{
    public class AxisRotation : IAxisRotation
    {
        public double GetDistanceBetweenNodes(Coordinate n1, Coordinate n2)
        {
            return Math.Round(Math.Sqrt(Math.Pow(n2.X - n1.X, 2) + Math.Pow(n2.Y - n1.Y, 2)), 2);
        }

        // This evaluates the angle the current xy-coordinate will have to rotate so that 
        // it is parallel to the line created by joining current node and destination node.
        public double GetRotationAngle(Coordinate currentNodeCoord, Coordinate destNodeCoord){
            return Math.Round(
                            RadiansToDegrees(
                                Math.Atan2(destNodeCoord.Y - currentNodeCoord.Y, destNodeCoord.X - currentNodeCoord.X)), 2);
        }

        public Coordinate GetMappedCoord(double rotationAngle, Coordinate coord){
            // Math.Sin and Math.Cos from the System assembly only take radians
            // so we need to convert the angle to radians
            var angleRad = DegreesToRadians(rotationAngle);

            var x = Math.Round(coord.X * Math.Cos(angleRad) + coord.Y * Math.Sin(angleRad), 2);
            var y = Math.Round(-coord.X * Math.Sin(angleRad) + coord.Y * Math.Cos(angleRad), 2);
            return new Coordinate{
                X = x,
                Y = y
            };
        }

        public TspProcessedData DoGreedyTspWithNoReturn(List<Node> graphNodes)
        {
            var path = new Queue<Coordinate>();
            var curNode = graphNodes[0].Coord;
            path.Enqueue(curNode);

            graphNodes.Remove(graphNodes[0]);
            double totalDist = 0.0;
            while (graphNodes.Count > 0)
            {
                Console.WriteLine($"Current node is {curNode}");
                var minDist = Double.PositiveInfinity;
                var nextNode = (Node)null;
                foreach (var node in graphNodes)
                {
                    // don't process starting or ending node
                    // if it is not the last one
                    if (node.IsStartOrEnd && graphNodes.Count != 1)
                    {
                        continue;
                    }
                    var dist = GetDistanceBetweenNodes(curNode, node.Coord);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nextNode = node;
                    }
                }

                Console.WriteLine($"****Next node is {nextNode.Coord}****");
                curNode = nextNode.Coord;
                path.Enqueue(curNode);
                graphNodes.Remove(nextNode);
                totalDist += minDist;
            }

            return new TspProcessedData {
                Path = path,
                DistanceTravelled = totalDist
            };
        }

        public TspProcessedData DoAxesRotationTspWithNoReturn(List<Node> graphNodes)
        {

            var path = new Queue<Coordinate>();
            var curNode = graphNodes[0].Coord;
            path.Enqueue(curNode);
            graphNodes.Remove(graphNodes[0]);
            double totalDist = 0.0;
            var destNode = graphNodes[graphNodes.Count - 1];
            
            while (graphNodes.Count > 0)
            {
                var rotAngle = GetRotationAngle(curNode, destNode.Coord);
                Console.WriteLine($"Current node is {curNode}, rot angle={rotAngle}deg");
                var xx = Double.PositiveInfinity;   // x' values of the next node
                var nextNode = (Node)null;

                foreach (var node in graphNodes)
                {
                    // don't process starting or ending node
                    // if it is not the last one
                    if (node.IsStartOrEnd && graphNodes.Count != 1)
                    {
                        continue;
                    }
                    // mapped coordinates in x'y' plane
                    var coord_primes = GetMappedCoord(rotAngle, node.Coord);

                    if(coord_primes.X < xx){
                        nextNode = node;
                        xx = coord_primes.X;
                    }
                }

                totalDist += GetDistanceBetweenNodes(curNode, nextNode.Coord);

                // now update current node and add to path
                curNode = nextNode.Coord;
                path.Enqueue(curNode);
                graphNodes.Remove(nextNode);
                Console.WriteLine($"****Next node is {nextNode.Coord}**** d={totalDist}");
            }

            return new TspProcessedData
            {
                Path = path,
                DistanceTravelled = totalDist
            };
        }

        public void PrintPath(Queue<Coordinate> path){
            Console.Write("Path: ");
            while(path.Count > 0){
                var node = path.Dequeue();
                Console.Write(node + "=>");
            }
            Console.WriteLine();
        }

        private double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}