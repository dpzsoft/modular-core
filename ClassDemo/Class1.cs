using System;
using dpz3;
using dpz3.Modular;

namespace ClassDemo {

    [Modular(ModularTypes.Api, "/Api/{ControllerName}")]
    public class Class1 : ApiControllerBase {

        [Modular(ModularTypes.Get, "Hello")]
        public string HelloWorld() {
            return "Hello World";
        }

        [Modular(ModularTypes.Get, "Hello2")]
        public IResult Hello2() {
            return Text("Hello2");
        }

    }
}
