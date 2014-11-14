using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSystemWeb.Models {
    internal struct Vector {
        public static readonly Vector Zero = new Vector(0, 0);

        public static readonly Vector One = new Vector(1, 1);
        public readonly double X, Y;

        public Vector(double X, double Y) {
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

        public double Dot(Vector that) {
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

        public static Vector operator *(Vector a, double b) {
            return new Vector(a.X * b, a.Y * b);
        }

        public static Vector operator +(Vector a, double b) {
            return new Vector(a.X + b, a.Y + b);
        }

        public static bool operator ==(Vector a, Vector b) {
            return a.Equals(b);
        }

        public static bool operator !=(Vector a, Vector b) {
            return !(a.Equals(b));
        }
    }

    internal interface TurtleAction {
    }

    internal class TurtleForward: TurtleAction {
        public TurtleForward(Vector newVector) {
            NewVector = newVector;
        }

        public Vector NewVector { get; private set; }
    }

    internal class TurtleRotate: TurtleAction {
        public TurtleRotate(double newAngle) {
            NewAngle = newAngle;
        }

        public double NewAngle { get; private set; }
    }

    internal class TurtlePush: TurtleAction {
        public TurtlePush(Vector savedPosition, double savedAngle) {
            SavedAngle = savedAngle;
            SavedPosition = savedPosition;
        }

        public Vector SavedPosition { get; private set; }
        public double SavedAngle { get; private set; }
    }

    internal class TurtlePop: TurtleAction {
        public TurtlePop(Vector restoredPosition, double restoredAngle) {
            RestoredAngle = restoredAngle;
            RestoredPosition = restoredPosition;
        }

        public Vector RestoredPosition { get; private set; }
        public double RestoredAngle { get; private set; }
    }

    internal class Turtle {
        private readonly List<TurtleAction> actions;
        private readonly Stack<Turtle> stack;
        private double angle;
        private double xPos;
        private double yPos;

        public Turtle(double xStart, double yStart) {
            xPos = xStart;
            yPos = yStart;

            angle = 0.0;
            MaxX = double.MinValue;
            MaxY = double.MinValue;
            MinX = double.MaxValue;
            MinY = double.MaxValue;

            stack = new Stack<Turtle>();
            actions = new List<TurtleAction>();
        }

        public double MaxX { get; private set; }
        public double MinX { get; private set; }
        public double MaxY { get; private set; }
        public double MinY { get; private set; }

        public void Forward(double distance) {
            var newX = xPos + Math.Cos(angle) * distance;
            var newY = yPos + Math.Sin(angle) * distance;
            actions.Add(new TurtleForward(new Vector(newX, newY)));

            MinX = Math.Min(MinX, newX);
            MaxX = Math.Max(MaxX, newX);
            MinY = Math.Min(MinY, newY);
            MaxY = Math.Max(MaxY, newY);

            xPos = newX;
            yPos = newY;
        }

        public void Rotate(double x) {
            angle += x;
            actions.Add(new TurtleRotate(angle));
        }

        public void Push() {
            stack.Push(this);

            actions.Add(new TurtlePush(new Vector(xPos, yPos), angle));
        }

        public void Pop() {
            var p = stack.Pop();
            actions.Add(new TurtlePop(new Vector(p.xPos, p.yPos), p.angle));

            xPos = p.xPos;
            yPos = p.yPos;
            angle = p.angle;
        }

        public List<TurtleAction> GetAllActions() {
            return actions;
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

        private static Dictionary<string, List<string>> svgCache = createSvgCache();

        public static string GetSvg(string name, int iterations, int BorderSize = 2) {
            if (svgCache.ContainsKey(name)) {
                var svgs = svgCache[name];
                if (iterations < svgs.Count) {
                    return svgs[iterations];
                } else {
                    var lsys = LSystem.AllLSystems.First(l => l.Name == name);
                    var state = lsys.Step(iterations);
                    var svg = lsys.ToSvg(state, BorderSize);
                    svgCache[name].Add(svg);

                    return svg;
                }
            }
            return null;
        }

        private static Dictionary<string, List<string>> createSvgCache() {
            var all = AllLSystems;
            return all.Select(ls => {
                var svgs = Enumerable.Range(0, ls.MaxIterations).Select(i => {
                    var state = ls.Step(i);
                    return ls.ToSvg(state, 2);
                });
                return Tuple.Create(ls.Name, svgs.ToList());
            }).ToDictionary(s => s.Item1, s => s.Item2);
        }

        public string Step(int iterations) {
            var state = axiom;

            for (var i = 0; i < iterations; i++) {
                state = StepOnce(state);
            }

            return state;
        }

        public string ToSvg(string state, int borderSize) {
            var turtle = new Turtle(0.0, 0.0);

            foreach (var c in state) {
                switch (c) {
                    case 'F':
                        turtle.Forward(5);
                        break;
                    case '+':
                        turtle.Rotate(angle);
                        break;
                    case '-':
                        turtle.Rotate(-angle);
                        break;
                    case '[':
                        turtle.Push();
                        break;
                    case ']':
                        turtle.Pop();
                        break;
                }
            }

            var allActions = turtle.GetAllActions();
            var offset = new Vector(Math.Abs(turtle.MinX) + borderSize, Math.Abs(turtle.MinY) + borderSize);

            var strings = allActions.Where(a => a is TurtleForward || a is TurtlePop).Select(action => {
                if (action is TurtleForward) {
                    var forward = action as TurtleForward;
                    var pos = forward.NewVector + offset;

                    return String.Format("L {0} {1} ", Math.Round(pos.X), Math.Round(pos.Y));
                }
                else {
                    var pop = action as TurtlePop;
                    var pos = pop.RestoredPosition + offset;

                    return String.Format("M {0} {1}", Math.Round(pos.X), Math.Round(pos.Y));
                }
            });

            var xSize = Math.Abs(turtle.MinX) + Math.Abs(turtle.MaxX) + borderSize * 2;
            var ySize = Math.Abs(turtle.MinY) + Math.Abs(turtle.MaxY) + borderSize * 2;
            var svg = new StringBuilder();
            svg.AppendFormat("<svg height='{0}' width='{1}' id='svg'><path d='", Math.Round(ySize), Math.Round(xSize));

            var firstForward = allActions.FirstOrDefault(a => a is TurtleForward);
            if (firstForward == null) {
                return "";
            }

            var firstPosition = ((TurtleForward) firstForward).NewVector + offset;

            svg.AppendFormat("M {0} {1} ", Math.Round(firstPosition.X), Math.Round(firstPosition.Y));

            foreach (var str in strings) {
                svg.Append(str);
            }

            svg.Append("' stroke='black' fill='none' stroke-width='1' /> </svg>");
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