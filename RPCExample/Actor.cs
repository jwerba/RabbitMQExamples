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
        protected ConsoleColor color = ConsoleColor.White;
        
        public void Start()
        {
            this.thread = new Thread(new ThreadStart(this.Run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        protected void WriteLine(string text)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = this.color;
            Console.WriteLine(text);
            Console.ForegroundColor = prev;
        }
    }
}
