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

        Turtle(Point start) {
            position = start;
            angle = 0.0;
            stack = new Stack<Turtle>();
            svgString = new StringBuilder();
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

        public string Step(int iterations) {
            var state = axiom;

            for (int i = 0; i < iterations; i++) {
                state = StepOnce(state);
            }

            return state;
        }

        private string StepOnce(string state) {
            var newState = new StringBuilder();

            for (char c in state) {
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