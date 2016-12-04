// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleSpiner.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A utility class for displaying an animated processing cursor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Console
{
    /// <summary>
    /// A utility class for displaying an animated processing cursor
    /// </summary>
    public class ConsoleSpiner
    {
        /// <summary>
        /// A counter for which cursor animation to show
        /// </summary>
        private int _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleSpiner"/> class.
        /// </summary>
        public ConsoleSpiner()
        {
            this._counter = 0;
        }

        /// <summary>
        /// Causes the cursor to spin one tick
        /// </summary>
        public void Turn()
        {
            this._counter++;
            switch (this._counter % 4)
            {
                case 0:
                    System.Console.Write("/");
                    break;
                case 1:
                    System.Console.Write("-");
                    break;
                case 2:
                    System.Console.Write("\\");
                    break;
                case 3:
                    System.Console.Write("|");
                    break;
            }

            System.Console.SetCursorPosition(System.Console.CursorLeft - 1, System.Console.CursorTop);
        }
    }
}
