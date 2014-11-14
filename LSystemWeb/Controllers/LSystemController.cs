using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using LSystemWeb.Models;

namespace LSystemWeb.Controllers {
    public class LSystemController: ApiController {
        public string Get(string name, int iterations) {
            var svg = LSystem.GetSvg(name, iterations);
            return svg ?? "<p id='error'>Not found!</p>";
        }
    }
}