using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using LSystemWeb.Models;

namespace LSystemWeb.Controllers {
    public class LSystemController: ApiController {
        private readonly Dictionary<string, List<string>> lsystemCache;

        public LSystemController() {
            var all = LSystem.AllLSystems;
            lsystemCache = all.Select(ls => {
                var svgs = Enumerable.Range(0, ls.MaxIterations).Select(i => {
                    var state = ls.Step(i);
                    return ls.ToSvg(state, 800, 800, 5);
                });
                return Tuple.Create(ls.Name, svgs.ToList());
            }).ToDictionary(s => s.Item1, s => s.Item2);
        }

        [HttpGet]
        public string FindLSystem(string name, int iterations) {
            if (!lsystemCache.ContainsKey(name)) {
                return "Not found";
            }
            var all = lsystemCache[name];
            if (iterations <= all.Count) {
                return all[iterations];
            }

            var lsys = LSystem.AllLSystems.First(l => l.Name == name);
            var state = lsys.Step(iterations);
            var svg = lsys.ToSvg(state, 800, 800, 5);
            lsystemCache[name].Add(svg);
            return svg;
        }
    }
}