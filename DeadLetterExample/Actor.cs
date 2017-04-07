using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit
{
    public abstract class Actor
    {
        protected abstract void Run();
        private Thread thread;
        public ConsoleColor Color { get; set; }
        public string Name { get; set; }

        public void Start()
        {
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        protected void WriteLine(string text)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = this.Color;
            Console.WriteLine(text);
            Console.ForegroundColor = prev;
        }
    }
}
