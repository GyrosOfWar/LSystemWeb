using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace LSystemWeb.Models {
    class Point {
        public double X, Y;
        public Point(float x, float y) {
            X = x;
            Y = y;
        }
    }

    class Turtle {
        private Point position;
        private double angle;
        private Stack<Turtle> stack;
        private StringBuilder svgString;

        public Turtle(int xStart, int yStart) {
            position = new Point(xStart, yStart);
            angle = 0.0;
            stack = new Stack<Turtle>();
            svgString = new StringBuilder();
        }

        public string GetSvgString() {
            return svgString.ToString();
        }

        public void Forward(double distance) {
            position.X += Math.Sin(angle) * distance;
            position.Y += Math.Cos(angle) * distance;

            svgString.AppendFormat("L {0} {1} ", (int) position.X, (int) position.Y);
        }

        public void Push() {
            stack.Push(this);
        }

        public void Pop() {
            var t = stack.Pop();

            this.position = t.position;
            this.angle = t.angle;
            svgString.AppendFormat("M {0} {1} ", (int) position.X, (int) position.Y);
        }

        public void Rotate(double t) {
            angle += t;
        }
    }

    public class LSystem {
        private string axiom;
        private Dictionary<char, string> rules;
        private double angle;

        public LSystem(string axiom, Dictionary<char, string> rules, double angle) {
            this.axiom = axiom;
            this.rules = rules;
            this.angle = angle;
        }

        public string Step(int iterations) {
            var state = axiom;

            for (int i = 0; i < iterations; i++) {
                state = StepOnce(state);
            }

            return state;
        }

        public string ToSvg(string state, int width, int height, int distance) {
            var svg = new StringBuilder();
            svg.AppendFormat("<svg height='{0}' width='{1}'>", width, height);
            var turtle = new Turtle(0, 0);

            foreach (var ch in state) {
                switch(ch) {
                    case 'F':
                        turtle.Forward(distance);
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

            svg.AppendFormat("<path d='{0}' />", turtle.GetSvgString());

            svg.Append("</svg>");
            return svg.ToString();
        }

        private string StepOnce(string state) {
            var newState = new StringBuilder();

            foreach (char c in state) {
                if (rules.ContainsKey(c)) {
                    newState.Append(rules[c]);
                } else {
                    newState.Append(c);
                }
            }

            return newState.ToString();
        }
    }
}