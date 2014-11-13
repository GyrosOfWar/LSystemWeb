using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using MoreLinq;

namespace LSystemWeb.Models {
    internal struct Vector {
        public static readonly Vector Zero = new Vector(0, 0);

        public static readonly Vector One = new Vector(1, 1);
        public readonly int X, Y;

        public Vector(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }

        public override bool Equals(object obj) {
            if (obj is Vector) {
                var p = (Vector) obj;
                return (p.X == X) && (p.Y == Y);
            }
            return false;
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString() {
            return "[" + X + ", " + Y + "]";
        }

        public int Dot(Vector that) {
            return X * that.X + Y * that.Y;
        }

        public static Vector operator +(Vector a, Vector b) {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b) {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator *(Vector a, Vector b) {
            return new Vector(a.X * b.X, a.Y * b.Y);
        }

        public static Vector operator *(Vector a, int b) {
            return new Vector(a.X * b, a.Y * b);
        }

        public static Vector operator +(Vector a, int b) {
            return new Vector(a.X + b, a.Y + b);
        }

        public static bool operator ==(Vector a, Vector b) {
            return a.Equals(b);
        }

        public static bool operator !=(Vector a, Vector b) {
            return !(a.Equals(b));
        }
    }

    internal class Turtle {
        public readonly double Angle;
        public readonly Color DrawColor;
        //TODO Problem: SVG string is written when position is still unclear. Might need to do two passes over turtles.
        public readonly Vector Position;
        public readonly ImmutableStack<Turtle> Stack;
        public readonly string SvgString;
        public readonly double xMax;
        public readonly double xMin;
        public readonly double yMax;
        public readonly double yMin;

        public Turtle(Vector pos, double angle, ImmutableStack<Turtle> stack, Color c, string svgString,
                      double xMin, double xMax, double yMin, double yMax) {
            Position = pos;
            Angle = angle;
            Stack = stack;
            DrawColor = c;
            SvgString = svgString;
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        public Turtle Forward(int distance) {
            var newX = Math.Cos(Angle) * distance;
            var newY = Math.Sin(Angle) * distance;
            var newPos = Position + new Vector((int) newX, (int) newY);
            var newSvgString = String.Format("{0} L {1} {2}", SvgString, Math.Round(newX), Math.Round(newY));

            return new Turtle(newPos, Angle, Stack, DrawColor, newSvgString,
                              Math.Min(xMin, newX), Math.Max(xMax, newX), Math.Min(yMin, newY), Math.Max(yMax, newY));
        }

        public Turtle Rotate(double rot) {
            return new Turtle(Position, Angle + rot, Stack, DrawColor, SvgString,
                              xMin, xMax, yMin, yMax);
        }

        public Turtle Pop() {
            if (Stack.IsEmpty) {
                throw new InvalidOperationException("Stack must not be empty when using Pop().");
            }

            var state = Stack.Peek();
            var newSvgString = String.Format("{0} L {1} {2}", state.SvgString, state.Position.X, state.Position.Y);
            return new Turtle(state.Position, state.Angle, Stack.Pop(), state.DrawColor, newSvgString,
                              xMin, xMax, yMin, yMax);
        }

        public Turtle Push() {
            var newStack = Stack.Push(this);
            return new Turtle(Position, Angle, newStack, DrawColor, SvgString,
                              xMin, xMax, yMin, yMax);
        }

        public Turtle ChangeColor(Color c) {
            return new Turtle(Position, Angle, Stack, c, SvgString,
                              xMin, xMax, yMin, yMax);
        }

        public Turtle NewWithPosition(Vector position) {
            return new Turtle(position, Angle, Stack, DrawColor, SvgString,
                              xMin, xMax, yMin, yMax);
        }
    }

    public class LSystem {
        private readonly double angle;
        private readonly string axiom;
        private readonly Dictionary<char, string> rules;

        public LSystem(string name, string axiom, Dictionary<char, string> rules, double angle, int maxIterations) {
            this.axiom = axiom;
            this.rules = rules;
            this.angle = angle;
            Name = name;
            MaxIterations = maxIterations;
        }

        public string Name { get; private set; }
        public int MaxIterations { get; private set; }

        public static IEnumerable<LSystem> AllLSystems {
            get {
                return new[] {
                    new LSystem(
                        "DragonCurve",
                        "FX",
                        new Dictionary<char, string> {{'X', "X+YF+"}, {'Y', "-FX-Y"}},
                        Math.PI / 2.0,
                        14),
                    new LSystem(
                        "HilbertCurve",
                        "A",
                        new Dictionary<char, string> {{'A', "-BF+AFA+FB-"}, {'B', "+AF-BFB-FA+"}},
                        Math.PI / 2.0,
                        6)
                };
            }
        }

        public string Step(int iterations) {
            var state = axiom;

            for (var i = 0; i < iterations; i++) {
                state = StepOnce(state);
            }

            return state;
        }

        public string ToSvg(string state, int borderSize) {
            var start = new Turtle(new Vector(0, 0), 0.0, ImmutableStack<Turtle>.Empty, Color.Black, "",
                                   double.MinValue, double.MaxValue, double.MinValue, double.MaxValue);

            var turtles = state.Scan(start, (turtle, c) => {
                switch (c) {
                    case 'F':
                        return turtle.Forward(5);
                    case '+':
                        return turtle.Rotate(angle);
                    case '-':
                        return turtle.Rotate(-angle);
                    case '[':
                        return turtle.Push();
                    case ']':
                        return turtle.Pop();
                    default:
                        return turtle;
                }
            }).ToList();

            var last = turtles.Last();
            //var xOff = Math.Abs(last.xMin) + borderSize;
            //var yOff = Math.Abs(last.yMin) + borderSize;

            var offset = new Vector((int) Math.Abs(last.xMin) + borderSize, (int) Math.Abs(last.yMin) + borderSize);
            var turtlesWithOffset = turtles.Select(t => t.NewWithPosition(t.Position + offset));

            var xSize = Math.Abs(last.xMin) + Math.Abs(last.xMax) + borderSize * 2;
            var ySize = Math.Abs(last.yMin) + Math.Abs(last.yMax) + borderSize * 2;

            var svg = new StringBuilder();
            svg.AppendFormat("<svg height='{0}' width='{1}'>", ySize, xSize);
            svg.AppendFormat("<path d='{0}' /></svg>", last.SvgString);
            return svg.ToString();
        }

        private string StepOnce(string state) {
            var newState = new StringBuilder();

            foreach (var c in state) {
                if (rules.ContainsKey(c)) {
                    newState.Append(rules[c]);
                }
                else {
                    newState.Append(c);
                }
            }

            return newState.ToString();
        }
    }
}